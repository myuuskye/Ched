
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Ched.Core
{
    public interface IExportSetting
    {
        string Title { get; set; }
        string Description { get; set; }
        int ID {  get; set; }
        List<string> Value { get; set; }
        List<string> Default {  get; set; }
        SettingTypes Type { get; set; }
        bool IsChanged { get; set; }
        string Category { get; set; }
        int Category2 {  get; set; }
        string Category2Name { get; set; }
        Image Category2Image { get; set; }
        int Category3 { get; set; }
        string Category3Name { get; set; }
        Image Category3Image { get; set; }

    }

    public enum SettingTypes { b, i, list }

}
