using Ched.Configuration;
using Ched.Localization;
using Ched.Properties;
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
        }
        //ここに追加するエクスポート設定を書いていく 大体500刻み
        private Dictionary<int, IExportSetting> settingColumns = new Dictionary<int, IExportSetting>()
        {
            //TAP
            {0, new ExportIntSetting(0, 0,  MainFormStrings.Generated + MainFormStrings.Notes, DescriptionStrings.GenerateByThisNote , 0, SettingTypes.i, new List<string> {MainFormStrings.TAP, MainFormStrings.ExTAP, MainFormStrings.TRACE, MainFormStrings.DAMAGE }, 0, 3, "TAP", 0 ) },
            {1, new ExportBoolSetting(1, false, MainFormStrings.Accuratejudge + "(" + MainFormStrings.Width + ")" , DescriptionStrings.AccurateOverlapDetection, false, SettingTypes.b, "TAP", 0) },
            {2, new ExportBoolSetting(2, false, MainFormStrings.Accuratejudge + "(" + MainFormStrings.Channel + ")" , DescriptionStrings.AccurateOverlapDetection2, false, SettingTypes.b, "TAP", 0) },
            {3, new ExportBoolSetting(3, true, DescriptionStrings.isOnSlideStart + "TAP" + MainFormStrings.NotGenerate, MainFormStrings.SlideStart + " " + MainFormStrings.OverlapNote, true, SettingTypes.b, "TAP", 1, MainFormStrings.isOnSlide, Resources.SlideIcon) },
            {4, new ExportBoolSetting(4, true, DescriptionStrings.isOnSlideStep + "TAP" + MainFormStrings.NotGenerate, MainFormStrings.SlideStep + " " + MainFormStrings.OverlapNote, true, SettingTypes.b, "TAP", 1 ) },
            {5, new ExportBoolSetting(5, true, DescriptionStrings.isOnSlideEnd + "TAP" + MainFormStrings.NotGenerate, MainFormStrings.SlideEnd + " " + MainFormStrings.OverlapNote, true, SettingTypes.b, "TAP", 1 ) },
            {6, new ExportBoolSetting(6, false, DescriptionStrings.isOnGuideStart + "TAP" + MainFormStrings.NotGenerate, MainFormStrings.GuideStart + " " + MainFormStrings.OverlapNote, false, SettingTypes.b, "TAP", 2, MainFormStrings.isOnGuide, Resources.GuideGreen) },
            {7, new ExportBoolSetting(7, false, DescriptionStrings.isOnGuideStep + "TAP" + MainFormStrings.NotGenerate, MainFormStrings.GuideStep + " " + MainFormStrings.OverlapNote, false, SettingTypes.b, "TAP", 2 ) },
            {8, new ExportBoolSetting(8, false, DescriptionStrings.isOnGuideEnd + "TAP" + MainFormStrings.NotGenerate, MainFormStrings.GuideEnd + " " + MainFormStrings.OverlapNote, false, SettingTypes.b, "TAP", 2 ) },

            {9, new ExportBoolSetting(9, false, DescriptionStrings.isOnTap + DescriptionStrings.Thisnote + MainFormStrings.NotGenerate,  "TAP " + MainFormStrings.OverlapNote, false, SettingTypes.b, "TAP", 3, DescriptionStrings.isOnTap, Resources.TapIcon) },
            {10, new ExportIntSetting(10, 0, DescriptionStrings.isOnTap + DescriptionStrings.Thisnote + DescriptionStrings.ToFlick,  "TAP " + DescriptionStrings.isOverlap + DescriptionStrings.Thisnote + DescriptionStrings.ToFlick, 0, SettingTypes.i, new List<string> {MainFormStrings.None, MainFormStrings.AirUp, MainFormStrings.AirLeftUp, MainFormStrings.AirRightUp, MainFormStrings.AirDown, MainFormStrings.AirLeftDown, MainFormStrings.AirRightDown }, 0, 6, "TAP", 3) },

            {11, new ExportBoolSetting(11, false, DescriptionStrings.isOnTap2 + "TAP" + MainFormStrings.NotGenerate,  "TAP2 " + MainFormStrings.OverlapNote, false, SettingTypes.b, "TAP", 4, DescriptionStrings.isOnTap2, Resources.TapIcon) },
            {12, new ExportBoolSetting(12, false, DescriptionStrings.isOnExTap + "TAP" + MainFormStrings.NotGenerate,  "ExTAP " + MainFormStrings.OverlapNote, false, SettingTypes.b, "TAP", 5, DescriptionStrings.isOnExTap, Resources.TapIcon) },
            {13, new ExportBoolSetting(13, false, DescriptionStrings.isOnExTap2 + "TAP" + MainFormStrings.NotGenerate,  "ExTAP2 " + MainFormStrings.OverlapNote, false, SettingTypes.b, "TAP", 6, DescriptionStrings.isOnExTap2, Resources.TapIcon) },
            {14, new ExportBoolSetting(14, false, DescriptionStrings.isOnFlick + "TAP" + MainFormStrings.NotGenerate,  "FLICK " + MainFormStrings.OverlapNote, false, SettingTypes.b, "TAP", 7, DescriptionStrings.isOnExTap, Resources.TapIcon) },
            {15, new ExportBoolSetting(15, false, DescriptionStrings.isOnFlick2 + "TAP" + MainFormStrings.NotGenerate,  "FLICK2 " + MainFormStrings.OverlapNote, false, SettingTypes.b, "TAP", 8, DescriptionStrings.isOnExTap2, Resources.TapIcon) },

            //TAP2
            {502, new ExportBoolSetting(502, true, MainFormStrings.isOnSlideStart + "TAP2" + MainFormStrings.NotGenerate, MainFormStrings.SlideStart + " " + MainFormStrings.OverlapNote, true, SettingTypes.b, "TAP2", 1, MainFormStrings.isOnSlide ) },
            {503, new ExportBoolSetting(503, true, MainFormStrings.isOnSlideStep + "TAP2" + MainFormStrings.NotGenerate, MainFormStrings.SlideStep + " " + MainFormStrings.OverlapNote, true, SettingTypes.b, "TAP2", 1 ) },
            {504, new ExportBoolSetting(504, true, MainFormStrings.isOnSlideEnd + "TAP2" + MainFormStrings.NotGenerate, MainFormStrings.SlideEnd + " " + MainFormStrings.OverlapNote, true, SettingTypes.b, "TAP2" , 1) },

            //ExTAP
            {1000, new ExportIntSetting(1000, 1, MainFormStrings.Generated + MainFormStrings.Notes, DescriptionStrings.GenerateByThisNote , 1, SettingTypes.i, new List<string> {MainFormStrings.TAP, MainFormStrings.ExTAP, MainFormStrings.TRACE, MainFormStrings.DAMAGE }, 0, 3, "ExTAP", 0 ) },

            //SLIDE
            {4000, new ExportIntSetting(4000, 0, MainFormStrings.SlideStartTypes, MainFormStrings.NormalCondition + MainFormStrings.Generated + MainFormStrings.SlideStart + MainFormStrings.Type, 0, SettingTypes.i, new List<string> {MainFormStrings.SlideNormal, MainFormStrings.SlideTrace, MainFormStrings.None }, 0, 2, "SLIDE", 0 ) },
        };

        


    }

    

}
