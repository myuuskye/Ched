using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Ched.Configuration
{
    internal sealed class ApplicationSettings : SettingsBase
    {
        public static ApplicationSettings Default { get; } = (ApplicationSettings)Synchronized(new ApplicationSettings());

        private ApplicationSettings()
        {
        }

        [UserScopedSetting]
        [DefaultSettingValue("12")]
        public int UnitLaneWidth
        {
            get { return ((int)(this["UnitLaneWidth"])); }
            set { this["UnitLaneWidth"] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("120")]
        public int UnitBeatHeight
        {
            get { return ((int)(this["UnitBeatHeight"])); }
            set { this["UnitBeatHeight"] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool InsertAirWithAirAction
        {
            get { return ((bool)(this["InsertAirWithAirAction"])); }
            set { this["InsertAirWithAirAction"] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool IsPreviewAbortAtLastNote
        {
            get { return ((bool)(this["IsPreviewAbortAtLastNote"])); }
            set { this["IsPreviewAbortAtLastNote"] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool IsSlowDownPreviewEnabled
        {
            get => (bool)this["IsSlowDownPreviewEnabled"];
            set => this["IsSlowDownPreviewEnabled"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsAnotherChannelEditable
        {
            get => (bool)this["IsAnotherChannelEditable"];
            set => this["IsAnotherChannelEditable"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("1")]
        public int NoteVisualMode
        {
            get => (int)this["NoteVisualMode"];
            set => this["NoteVisualMode"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsAnotherChannelSounds
        {
            get => (bool)this["IsAnotherChannelSounds"];
            set => this["IsAnotherChannelSounds"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsAnotherChannelFormSpeeds
        {
            get => (bool)this["IsAnotherChannelFormSpeeds"];
            set => this["IsAnotherChannelFormSpeeds"] = value;
        }


        [UserScopedSetting]
        [DefaultSettingValue("0")]
        public int GuideDefaultFade
        {
            get => (int)this["GuideDefaultFade"];
            set => this["GuideDefaultFade"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("0")]
        public decimal LaneOffset
        {
            get => (decimal)this["LaneOffset"];
            set => this["LaneOffset"] = value;
        }



        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsTapHideOnSlide
        {
            get => (bool)this["IsTapHideOnSlide"];
            set => this["IsTapHideOnSlide"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsExTapHideOnSlide
        {
            get => (bool)this["IsExTapHideOnSlide"];
            set => this["IsExTapHideOnSlide"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsTap2HideOnSlide
        {
            get => (bool)this["IsTap2HideOnSlide"];
            set => this["IsTap2HideOnSlide"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsExTap2HideOnSlide
        {
            get => (bool)this["IsExTap2HideOnSlide"];
            set => this["IsExTap2HideOnSlide"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsFlickHideOnSlide
        {
            get => (bool)this["IsFlickHideOnSlide"];
            set => this["IsFlickHideOnSlide"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsDamageHideOnSlide
        {
            get => (bool)this["IsDamageHideOnSlide"];
            set => this["IsDamageHideOnSlide"] = value;
        }


        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsTapHideOnGuide
        {
            get => (bool)this["IsTapHideOnGuide"];
            set => this["IsTapHideOnGuide"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsExTapHideOnGuide
        {
            get => (bool)this["IsExTapHideOnGuide"];
            set => this["IsExTapHideOnGuide"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsFlickHideOnGuide
        {
            get => (bool)this["IsFlickHideOnGuide"];
            set => this["IsFlickHideOnGuide"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsDamageHideOnGuide
        {
            get => (bool)this["IsDamageHideOnGuide"];
            set => this["IsDamageHideOnGuide"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool IsTapEraseStart
        {
            get => (bool)this["IsTapEraseStart"];
            set => this["IsTapEraseStart"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool IsExTapEraseStart
        {
            get => (bool)this["IsExTapEraseStart"];
            set => this["IsExTapEraseStart"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool IsTap2EraseStart
        {
            get => (bool)this["IsTap2EraseStart"];
            set => this["IsTap2EraseStart"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool IsExTap2EraseStart
        {
            get => (bool)this["IsExTap2EraseStart"];
            set => this["IsExTap2EraseStart"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsDamageEraseStart
        {
            get => (bool)this["IsDamageEraseStart"];
            set => this["IsDamageEraseStart"] = value;
        }


        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool IsTapEraseEnd
        {
            get => (bool)this["IsTapEraseEnd"];
            set => this["IsTapEraseEnd"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool IsExTapEraseEnd
        {
            get => (bool)this["IsExTapEraseEnd"];
            set => this["IsExTapEraseEnd"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool IsTap2EraseEnd
        {
            get => (bool)this["IsTap2EraseEnd"];
            set => this["IsTap2EraseEnd"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool IsExTap2EraseEnd
        {
            get => (bool)this["IsExTap2EraseEnd"];
            set => this["IsExTap2EraseEnd"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsDamageEraseEnd
        {
            get => (bool)this["IsDamageEraseEnd"];
            set => this["IsDamageEraseEnd"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool IsAllowStepChannel
        {
            get => (bool)this["IsAllowStepChannel"];
            set => this["IsAllowStepChannel"] = value;
        }

    }
}
