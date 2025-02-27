﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ched.Core
{
    /// <summary>
    /// 譜面ファイルを表すクラスです。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class ScoreBook
    {
        protected static readonly Version CurrentVersion = typeof(ScoreBook).Assembly.GetName().Version;

        internal static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };

        [Newtonsoft.Json.JsonProperty]
        private Version version = CurrentVersion;
        [Newtonsoft.Json.JsonProperty]
        private string title = "";
        [Newtonsoft.Json.JsonProperty]
        private string artistName = "";
        [Newtonsoft.Json.JsonProperty]
        private string notesDesignerName = "";
        [Newtonsoft.Json.JsonProperty]
        private decimal laneoffset = 0;
        [Newtonsoft.Json.JsonProperty]
        private Score score = new Score();
        [Newtonsoft.Json.JsonProperty]
        private Dictionary<string, string> exportArgs = new Dictionary<string, string>();
        [Newtonsoft.Json.JsonProperty]
        private double offset = 0;
        [Newtonsoft.Json.JsonProperty]
        private Dictionary<int, string> channelNames = new Dictionary<int, string>();
        [Newtonsoft.Json.JsonProperty]
        private Dictionary<int, string> exportSettings = new Dictionary<int, string>();

        public string Path { get; set; }

        /// <summary>
        /// ファイルを作成したアプリケーションのバージョンを設定します。
        /// </summary>
        public Version Version
        {
            get { return version; }
            set { version = value; }
        }

        /// <summary>
        /// 楽曲のタイトルを設定します。
        /// </summary>
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        /// <summary>
        /// 楽曲のアーティストを設定します。
        /// </summary>
        public string ArtistName
        {
            get { return artistName; }
            set { artistName = value; }
        }

        /// <summary>
        /// 譜面制作者名を設定します。
        /// </summary>
        public string NotesDesignerName
        {
            get { return notesDesignerName; }
            set { notesDesignerName = value; }
        }
        /// <summary>
        /// レーンオフセット(CCの拡張機能)を設定します。
        /// </summary>
        public decimal LaneOffset
        {
            get { return laneoffset; }
            set { laneoffset = value; }
        }

        /// <summary>
        /// オフセットを設定します。
        /// </summary>
        public double Offset
        {
            get { return offset; }
            set { offset = value; }
        }



        /// <summary>
        /// 譜面データを設定します。
        /// </summary>
        public Score Score
        {
            get { return score; }
            set { score = value; }
        }

        /// <summary>
        /// エクスポート用の設定を格納します。
        /// </summary>
        public Dictionary<string, string> ExportArgs
        {
            get { return exportArgs; }
            set { exportArgs = value; }
        }

        /// <summary>
        /// チャンネル名の設定を格納します。
        /// </summary>
        public Dictionary<int, string> ChannelNames
        {
            get { return channelNames; }
            set { channelNames = value; }
        }

        /// <summary>
        /// エクスポート設定を格納します。
        /// </summary>
        public Dictionary<int, string> ExportSettings
        {
            get { return exportSettings; }
            set { exportSettings = value; }
        }

        public void Save(string path)
        {
            Path = path;
            Save();
        }

        public void Save()
        {
            string data = JsonConvert.SerializeObject(this, SerializerSettings);
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            using (var stream = new MemoryStream(bytes))
            {
                using (var file = new FileStream(Path, FileMode.Create))
                using (var gz = new GZipStream(file, CompressionMode.Compress))
                {
                    stream.CopyTo(gz);
                }
            }
        }

        public ScoreBook Clone() => JsonConvert.DeserializeObject<ScoreBook>(JsonConvert.SerializeObject(this, SerializerSettings));

        /// <summary>
        /// 指定のファイルから<see cref="ScoreBook"/>のインスタンスを生成します。
        /// 古いバージョンのファイルは現在のバージョン用に変換されます。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ScoreBook LoadFile(string path)
        {
            Version fileVersion = GetFileVersion(path);
            string data = GetDecompressedData(path);

            var fileextension = System.IO.Path.GetExtension(path).Trim('.');
            var res = new ScoreBook();


            var doc = JObject.Parse(data);



            if (fileVersion.Major < 2)
            {
                // Major = 2用の変換
                foreach (var slide in doc["score"]["notes"]["slides"])
                {
                    var obj = (JObject)slide;
                    slide["startWidth"] = obj.Property("width").Value;
                    obj.Property("width").Remove();
                }
            }

            if (fileVersion.Major < 3)
            {
                var notes = doc["score"]["notes"];
                var types = new[] { notes["airs"], notes["airActions"] }.SelectMany(p => p.Select(q => (JObject)q["parentNote"])).Where(p => p.ContainsKey("$type"));
                foreach (var obj in types)
                {
                    string type = obj["$type"].ToString();
                    type = System.Text.RegularExpressions.Regex.Replace(type, "Ched$", "Ched.Core").Replace("Components", "Core");
                    obj["$type"] = type;
                }

                var susExportArgs = doc["exporterArgs"]["sus"];
                var exportArgs = new JObject();
                if (susExportArgs != null) exportArgs.Add("Ched.Plugins.SusExportPlugin", susExportArgs.ToString());
                doc.Remove("exporterArgs");
                doc.Add("exportArgs", exportArgs);
            }

            doc["version"] = JObject.FromObject(CurrentVersion);

            res = doc.ToObject<ScoreBook>(JsonSerializer.Create(SerializerSettings));

            if (res.Score.Events.TimeSignatureChangeEvents.Count == 0)
            {
                res.Score.Events.TimeSignatureChangeEvents.Add(new Events.TimeSignatureChangeEvent() { Tick = 0, Numerator = 4, DenominatorExponent = 2 });
            }



            res.Path = path;
            return res;
        }

        /// <summary>
        /// 指定のファイルが現在のバージョンと互換性があるかどうか調べます。
        /// </summary>
        /// <param name="path">ファイルへのパス</param>
        /// <returns>互換性があればtrue, 互換性がなければfalse</returns>
        public static bool IsCompatible(string path)
        {
            return GetFileVersion(path).Major <= CurrentVersion.Major;
        }

        /// <summary>
        /// 指定のファイルに対してバージョンアップが必要かどうか調べます。
        /// </summary>
        /// <param name="path">ファイルへのパス</param>
        /// <returns>バージョンアップが必要であればtrue, 必要でないならばfalse</returns>
        public static bool IsUpgradeNeeded(string path)
        {
            return GetFileVersion(path).Major < CurrentVersion.Major;
        }

        private static string GetDecompressedData(string path)
        {
            using (var gz = new GZipStream(new FileStream(path, FileMode.Open), CompressionMode.Decompress))
            using (var stream = new MemoryStream())
            {
                gz.CopyTo(stream);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private static string GetData(string path)
        {
            using (var file = new FileStream(path, FileMode.Open))
            using (var stream = new MemoryStream())
            {
                Console.Write(Encoding.UTF8.GetString(stream.ToArray()));
                file.CopyTo(stream);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// 指定のファイルに保存されたデータのバージョンを取得します。
        /// </summary>
        /// <param name="path">ファイルへのパス</param>
        /// <returns>ファイルが生成されたバージョン</returns>
        private static Version GetFileVersion(string path)
        {
            var doc = JObject.Parse(GetDecompressedData(path));
            return doc["version"].ToObject<Version>();
        }
    }
}
