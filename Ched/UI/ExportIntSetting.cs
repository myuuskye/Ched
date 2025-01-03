using Ched.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ched.Core
{
    public class ExportIntSetting : IExportSetting
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public int ID {  get; set; }
        public string Default { get; set; }
        public string Type { get; set; } = "int";
        public List<string> Choices { get; set; } 
        public bool isChanged {  get; set; }


        public ExportIntSetting(int id, int value, string title, string description, int @default, string type, List<string> choices ) { 
            Title = title;
            ID = id;
            Value = value.ToString();
            Description = description;
            Default = @default.ToString();
            Type = type;
            Choices = choices;
            if (value != @default) isChanged = true;
        }



    }

    

}
