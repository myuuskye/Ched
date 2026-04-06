using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Newtonsoft.Json;
using Ched.Components.Exporter;
using Ched.UI.Windows;


namespace Ched.Plugins
{
    public class LevelDataExportPlugin : IScoreBookExportPlugin
    {
        public string DisplayName => "LevelData";

        public string FileFilter => "";
        public int ID => 2;

        public void Export(IScoreBookExportPluginArgs args, string path)
        {

            var book = args.GetScoreBook();
            
            var exporter = new LevelDataExporter(book);
            exporter.Export(args.Stream, path);
        }
    }
}
