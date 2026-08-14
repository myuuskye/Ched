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
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Xml.Linq;

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


            entities.Add(new LevelDataEntity("Initialization", new List<LevelDataData>() { new LevelDataValue("initialLife", ScoreBook.HP) }));
        

            //bpm
            foreach (var bpmevent in book.Score.Events.BpmChangeEvents)
            {

                var beat = new LevelDataValue("#BEAT", (double)bpmevent.Tick / 480);
                var bpm = new LevelDataValue("#BPM", bpmevent.Bpm);

                entities.Add(new LevelDataEntity("#BPM_CHANGE", new List<LevelDataData>() { beat, bpm }));
            }


            //timeScale 
            var timeScaleGroups = new List<LevelDataEntity>();

            int maxChannel = 0;
            if(book.Score.Events.HighSpeedChangeEvents.Count > 0)
            {
                maxChannel = book.Score.Events.HighSpeedChangeEvents.OrderBy(p => p.SpeedCh).Last().SpeedCh;
            }

            for (int i = 0; i <= maxChannel; i++)
            {
                var editorName = new LevelDataRef("editorName", "#" + i);
                var eventList = book.Score.Events.HighSpeedChangeEvents;
                var sortedeventList = eventList.Where(p => p.SpeedCh == i);

                Console.WriteLine(eventList.OrderBy(p => p.SpeedCh).ToList().Where(q => q.SpeedCh < i).Count());
                Console.WriteLine(i + " : " + eventList.OrderBy(p => p.SpeedCh).ToList().FindIndex(q => q.SpeedCh == i) + " : " + (i + eventList.OrderBy(p => p.SpeedCh).ToList().FindIndex(q => q.SpeedCh == i)));
                var gname = i + eventList.OrderBy(p => p.SpeedCh).ToList().Where(q => q.SpeedCh < i).Count();

                if (sortedeventList.Count() > 0) //Changeイベントを持ってるか
                {
                    var first = new LevelDataRef("first", (i + eventList.OrderBy(p => p.SpeedCh).ToList().Where(q => q.SpeedCh < i).Count() + 1).ToString());
                    entities.Add(new NamedLevelDataEntity("#TIMESCALE_GROUP", new List<LevelDataData>() { editorName, first }, gname.ToString()));

                    foreach (var timescaleevent in sortedeventList.OrderBy(q => q.Tick))
                    {
                        var timeScaleGroup = new LevelDataRef("#TIMESCALE_GROUP", (i + eventList.OrderBy(p => p.SpeedCh).ToList().Where(q => q.SpeedCh < i).Count()).ToString());
                        var beat = new LevelDataValue("#BEAT", (double)timescaleevent.Tick / 480);
                        var editorLane = new LevelDataValue("editorLane", -12);
                        var timeScale = new LevelDataValue("#TIMESCALE", (double)timescaleevent.SpeedRatio);
                        var timeScaleSkip = new LevelDataValue("#TIMESCALE_SKIP", timescaleevent.Skip);
                        var timeScaleEase = new LevelDataValue("#TIMESCALE_EASE", timescaleevent.Ease);
                        var hidenotes = new LevelDataValue("hideNotes", timescaleevent.HideNotes);
                        var next = new LevelDataRef("next", "");
                        var thisIndex = eventList.OrderBy(q => q.Tick).ToList().IndexOf(timescaleevent); //全体のリストでこの要素は何番目か
                        var name = i + thisIndex + 1;
                        if (sortedeventList.OrderBy(q => q.Tick).Last() != timescaleevent) //この要素が最後ではない
                        {
                            next = new LevelDataRef("next", (i + thisIndex + 2).ToString());
                            entities.Add(new NamedLevelDataEntity("#TIMESCALE_CHANGE", new List<LevelDataData>() { timeScaleGroup, beat, editorLane, timeScale, timeScaleSkip, timeScaleEase, hidenotes, next }, name.ToString()));
                        }
                        else
                        {
                            entities.Add(new NamedLevelDataEntity("#TIMESCALE_CHANGE", new List<LevelDataData>() { timeScaleGroup, beat, editorLane, timeScale, timeScaleSkip, timeScaleEase, hidenotes}, name.ToString()));
                        }
                        
                    }
                }
                else
                {
                    entities.Add(new NamedLevelDataEntity("#TIMESCALE_GROUP", new List<LevelDataData>() { editorName }, gname.ToString()));
                }
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
