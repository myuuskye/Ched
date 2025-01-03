using Ched.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ched.Core
{
    public class ExportBoolSetting : IExportSetting
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public int ID {  get; set; }
        public string Default { get; set; }
        public string Type {  get; set; } = "bool";
        public bool isChanged { get; set; } = false;


        public ExportBoolSetting(int id, bool value, string title, string description, bool @default, string type = "bool") { 
            Title = title;
            ID = id;
            Value = value.ToString();
            Description = description;
            Default = @default.ToString();
            Type = type;
            if(value != @default) isChanged = true;
        }



    }

    

}
