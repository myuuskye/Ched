using Ched.Configuration;
using Ched.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ched.Core
{
    public class ExportSetting
    {
        public Dictionary<int, IExportSetting> SettingColumns
        {
            get { return settingColumns; }
            set { settingColumns = value; }
        }
        //ここに追加するエクスポート設定を書いていく
        private Dictionary<int, IExportSetting> settingColumns = new Dictionary<int, IExportSetting>()
        {
            //TAP
            {0, new ExportBoolSetting(0, true, MainFormStrings.isOnSlideStart + "TAP" + MainFormStrings.NotGenerate, MainFormStrings.SlideStart + MainFormStrings.OverlapNote, true, "bool") },
            {1, new ExportBoolSetting(1, true, MainFormStrings.isOnSlideStep + "TAP" + MainFormStrings.NotGenerate, MainFormStrings.SlideStep + MainFormStrings.OverlapNote, true, "bool" ) },
            {2, new ExportBoolSetting(2, true, MainFormStrings.isOnSlideEnd + "TAP" + MainFormStrings.NotGenerate, MainFormStrings.SlideEnd + MainFormStrings.OverlapNote, true, "bool" ) },

            //TAP2
            {500, new ExportBoolSetting(500, true, MainFormStrings.isOnSlideStart + "TAP2" + MainFormStrings.NotGenerate, MainFormStrings.SlideStart + MainFormStrings.OverlapNote, true, "bool" ) },
            {501, new ExportBoolSetting(501, true, MainFormStrings.isOnSlideStep + "TAP2" + MainFormStrings.NotGenerate, MainFormStrings.SlideStep + MainFormStrings.OverlapNote, true, "bool" ) },
            {502, new ExportBoolSetting(502, true, MainFormStrings.isOnSlideEnd + "TAP2" + MainFormStrings.NotGenerate, MainFormStrings.SlideEnd + MainFormStrings.OverlapNote, true, "bool" ) },

            //SLIDE
            {4000, new ExportIntSetting(4000, 0, MainFormStrings.SlideStartTypes, MainFormStrings.NormalCondition + MainFormStrings.Generated + MainFormStrings.SlideStart + MainFormStrings.Type, 0, "int", new List<string> {MainFormStrings.SlideNormal, MainFormStrings.SlideTrace, MainFormStrings.None } ) },
        };


    }

    

}
