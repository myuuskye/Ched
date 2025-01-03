using Ched.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ched.Core
{
    public interface IExportSetting
    {
        string Title { get; set; }
        string Description { get; set; }
        int ID {  get; set; }
        string Value { get; set; }
        string Default {  get; set; }
        string Type { get; set; }
        bool isChanged { get; set; }



    }

    

}
