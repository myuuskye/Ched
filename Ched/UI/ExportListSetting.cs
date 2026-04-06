using Ched.Localization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ched.Core
{
    public class ExportListSetting : IExportSetting
    {
        public string Title { get; set; }
        public List<string> Value { get; set; }
        public string Description { get; set; }
        public int ID {  get; set; }
        public List<string> Default { get; set; }
        public SettingTypes Type { get; set; } = SettingTypes.list;
        
        public List<string> Choices { get; set; } 
        public bool IsChanged {  get; set; }
        public int Min {  get; set; }
        public int Max { get; set; }
        public string Category { get; set; }
        public int Category2 { get; set; }
        public string Category2Name { get; set; }
        public Image Category2Image { get; set; }
        public int Category3 { get; set; }
        public string Category3Name { get; set; }
        public Image Category3Image { get; set; }

        public ExportListSetting(int id, List<int> value, string title, string description, List<int> @default, SettingTypes type, List<string> choices, int min, int max, string category = "Other", int category2 = 0, string cate2name = "", Image cate2image = null, int category3 = 0, string cate3name = "", Image cate3image = null) { 
            Title = title;
            ID = id;
            Value = value.ConvertAll<string>(delegate(int i) { return i.ToString(); });
            Description = description;
            Default = @default.ConvertAll<string>(delegate (int i) { return i.ToString(); });
            Type = type;
            Choices = choices;
            if (value != @default) IsChanged = true;
            Min = min;
            Max = max;
            Category = category;
            Category2 = category2;
            Category2Name = cate2name;
            Category2Image = cate2image;
            Category3 = category3;
            Category3Name = cate3name;
            Category3Image = cate3image;
        }

        public ExportListSetting(ExportListSetting setting)
        {
            Title = setting.Title;
            ID = setting.ID;
            Value = setting.Value;
            Description = setting.Description;
            Default = setting.Default;
            Type = setting.Type;
            Choices = setting.Choices;
            if (setting.Value[0] != setting.Default[0]) IsChanged = true;
            Min = setting.Min;
            Max = setting.Max;
            Category = setting.Category;
            Category2 = setting.Category2;
            Category2Name = setting.Category2Name;
            Category2Image = setting.Category2Image;
            Category3 = setting.Category3;
            Category3Name = setting.Category3Name;
            Category3Image = setting.Category3Image;
        }



    }

    

}
