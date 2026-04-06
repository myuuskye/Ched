using Ched.Configuration;
using Ched.Core;
using Ched.Core.Events;
using Ched.Core.Notes;
using Ched.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Ched.Components.Exporter
{
    public class LevelDataExporter
    {

        protected ScoreBook ScoreBook { get; set; }
        protected BarIndexCalculator BarIndexCalculator { get; }
        protected int StandardBarTick => ScoreBook.Score.TicksPerBeat * 4;

        private double offset = 0;


        [Newtonsoft.Json.JsonProperty]
        private double bgmOffset = 0;

        [Newtonsoft.Json.JsonProperty]
        private List<LevelDataEntity> entities = new List<LevelDataEntity>();


       

        internal static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            TypeNameHandling = TypeNameHandling.None,
            Formatting = Formatting.Indented
        };

        public LevelDataExporter(ScoreBook book)
        {
            ScoreBook = book;
            BarIndexCalculator = new BarIndexCalculator(book.Score.TicksPerBeat, book.Score.Events.TimeSignatureChangeEvents);
        }


        public void Export(Stream stream, string path)
        {
            
            var book = ScoreBook;
            var notes = book.Score.Notes;
            entities = new List<LevelDataEntity>();
            bgmOffset = book.Offset;

            var es = new Dictionary<int, IExportSetting>();


            int guideDefType = ApplicationSettings.Default.GuideDefaultFade;

            bool judgeAccurate = ApplicationSettings.Default.IsAccurateOverlap;

            es = ApplicationSettings.Default.DefaultExportSettings;

            foreach(var s in ScoreBook.ExportSettings)
            {
                es[s.Key] = s.Value;
            }

            entities.Add(new LevelDataEntity("Initialization", new List<LevelDataData>()));

            foreach (var bpmevent in book.Score.Events.BpmChangeEvents)
            {

                var beat = new LevelDataValue("#BEAT", (double)bpmevent.Tick / 480);
                var bpm = new LevelDataValue("#BPM", bpmevent.Bpm);

                entities.Add(new LevelDataEntity("#BPM_CHANGE", new List<LevelDataData>() { beat, bpm }));
            }









            string data = JsonConvert.SerializeObject(this, SerializerSettings);
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            using (var levelstream = new MemoryStream(bytes))
            {
                using (var file = new FileStream(path, FileMode.OpenOrCreate))
                using (var gz = new GZipStream(stream, CompressionMode.Compress))
                {
                    levelstream.CopyTo(gz);
                    
                }
            }




        }

       


    }
}
