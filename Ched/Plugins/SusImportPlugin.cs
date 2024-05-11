using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Newtonsoft.Json;
using Ched.Components.Exporter;
using Ched.UI.Windows;
using Ched.Core;
using Ched.UI;
using System.IO;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using Ched.Core.Events;
using Ched.Core.Notes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using System.Runtime.Remoting.Channels;
using Ched.Configuration;
using static Ched.Core.Notes.Guide;
using static Ched.Core.Notes.Slide;
using System.Security.Cryptography.X509Certificates;
using System.Drawing.Text;

namespace Ched.Plugins
{

    public class SusImportPlugin : IScoreBookImportPlugin
    {

        public string DisplayName => "Sliding Universal Score (*.sus)";

        public string FileFilter => "Sliding Universal Score (*.sus)|*.sus";

        [System.Runtime.InteropServices.DllImport("usctool.dll")]
        private extern static void usctool_convert(string path, string name);
        ScoreBook IScoreBookImportPlugin.Import(IScoreBookImportPluginArgs args)
        {
            Console.WriteLine(args.Path);
            ScoreBook result = new ScoreBook();
            

            string data = null;

            using (var file = new FileStream(args.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                data = Encoding.UTF8.GetString(stream.ToArray());
            }
            
            result.Score.Events.TimeSignatureChangeEvents.Add(new TimeSignatureChangeEvent() { Tick = 0, Numerator = 4, DenominatorExponent = 2 });
            usctool_convert(data, "test");


            return result;
        }
    }
}
