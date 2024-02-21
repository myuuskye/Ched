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
        public int SlideStartDefaultType
        {
            get => (int)this["SlideStartDefaultType"];
            set => this["SlideStartDefaultType"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("0")]
        public int SlideEndDefaultType
        {
            get => (int)this["SlideEndDefaultType"];
            set => this["SlideEndDefaultType"] = value;
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
        public bool IsFlick2HideOnSlide
        {
            get => (bool)this["IsFlick2HideOnSlide"];
            set => this["IsFlick2HideOnSlide"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsDamage2HideOnSlide
        {
            get => (bool)this["IsDamage2HideOnSlide"];
            set => this["IsDamage2HideOnSlide"] = value;
        }



        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool IsTapHideOnGuide
        {
            get => (bool)this["IsTapHideOnGuide"];
            set => this["IsTapHideOnGuide"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
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
        [DefaultSettingValue("True")]
        public bool IsFlick2HideOnGuide
        {
            get => (bool)this["IsFlick2HideOnGuide"];
            set => this["IsFlick2HideOnGuide"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsDamage2HideOnGuide
        {
            get => (bool)this["IsDamage2HideOnGuide"];
            set => this["IsDamage2HideOnGuide"] = value;
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
        public bool IsTapChangeFade
        {
            get => (bool)this["IsTapChangeFade"];
            set => this["IsTapChangeFade"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool IsExTapChangeFade
        {
            get => (bool)this["IsExTapChangeFade"];
            set => this["IsExTapChangeFade"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool IsTap2ChangeFade
        {
            get => (bool)this["IsTap2ChangeFade"];
            set => this["IsTap2ChangeFade"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool IsExTap2ChangeFade
        {
            get => (bool)this["IsExTap2ChangeFade"];
            set => this["IsExTap2ChangeFade"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool IsFlickChangeFade
        {
            get => (bool)this["IsFlickChangeFade"];
            set => this["IsFlickChangeFade"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsDamageChangeFade
        {
            get => (bool)this["IsDamageChangeFade"];
            set => this["IsDamageChangeFade"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsFlickSlideStartTrace
        {
            get => (bool)this["IsFlickSlideStartTrace"];
            set => this["IsFlickSlideStartTrace"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsFlickSlideEndTrace
        {
            get => (bool)this["IsFlickSlideEndTrace"];
            set => this["IsFlickSlideEndTrace"] = value;
        }

        //New
        //スライド始点に置いて

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsTapCritical // SlideStart スライドをクリティカル化するか
        {
            get => (bool)this["SSIsTapCritical"];
            set => this["SSIsTapCritical"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsTapChangeTraceS　// 始点をトレースにするか
        {
            get => (bool)this["SSIsTapChangeTraceS"];
            set => this["SSIsTapChangeTraceS"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsTapChangeTraceE //終点をトレースにするか
        {
            get => (bool)this["SSIsTapChangeTraceE"];
            set => this["SSIsTapChangeTraceE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsTapDeleteS // 始点を削除するか
        {
            get => (bool)this["SSIsTapDeleteS"];
            set => this["SSIsTapDeleteS"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsTapDeleteE // 終点を削除するか
        {
            get => (bool)this["SSIsTapDeleteE"];
            set => this["SSIsTapDeleteE"] = value;
        }

        // スライド終点に置いて

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsTapCritical // SlideEnd スライドをクリティカルにするか
        {
            get => (bool)this["SEIsTapCritical"];
            set => this["SEIsTapCritical"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsTapCriticalE //スライド終点をクリティカルにするか
        {
            get => (bool)this["SEIsTapCriticalE"];
            set => this["SEIsTapCriticalE"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsTapChangeTraceS // 始点をトレースにするか
        {
            get => (bool)this["SEIsTapChangeTraceS"];
            set => this["SEIsTapChangeTraceS"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsTapChangeTraceE //終点をトレースにするか
        {
            get => (bool)this["SEIsTapChangeTraceE"];
            set => this["SEIsTapChangeTraceE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsTapDeleteS //始点を削除するか
        {
            get => (bool)this["SEIsTapDeleteS"];
            set => this["SEIsTapDeleteS"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsTapDeleteE //終点を削除するか
        {
            get => (bool)this["SEIsTapDeleteE"];
            set => this["SEIsTapDeleteE"] = value;
        }
        //スライド中継点に置いて
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool STEIsTapCritical // SlidesTEp 中継点をクリティカルにするか
        {
            get => (bool)this["STEIsTapCritical"];
            set => this["STEIsTapCritical"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool STEIsTapAttach // 無視中継点にするか
        {
            get => (bool)this["STEIsTapAttach"];
            set => this["STEIsTapAttach"] = value;
        }

        //ガイド始点に置いて
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsTapFadeN // GuideStart  フェードを無にする
        {
            get => (bool)this["GSIsTapFadeN"];
            set => this["GSIsTapFadeN"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsTapFadeO // フェードをアウトにする
        {
            get => (bool)this["GSIsTapFadeO"];
            set => this["GSIsTapFadeO"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsTapFadeI // フェードをインにする
        {
            get => (bool)this["GSIsTapFadeI"];
            set => this["GSIsTapFadeI"] = value;
        }


        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool SSIsExTapCritical // SlideStart
        {
            get => (bool)this["SSIsExTapCritical"];
            set => this["SSIsExTapCritical"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsExTapChangeTraceS
        {
            get => (bool)this["SSIsExTapChangeTraceS"];
            set => this["SSIsExTapChangeTraceS"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsExTapChangeTraceE
        {
            get => (bool)this["SSIsExTapChangeTraceE"];
            set => this["SSIsExTapChangeTraceE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsExTapDeleteS
        {
            get => (bool)this["SSIsExTapDeleteS"];
            set => this["SSIsExTapDeleteS"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsExTapDeleteE
        {
            get => (bool)this["SSIsExTapDeleteE"];
            set => this["SSIsExTapDeleteE"] = value;
        }


        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsExTapCritical // SlideEnd
        {
            get => (bool)this["SEIsExTapCritical"];
            set => this["SEIsExTapCritical"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool SEIsExTapCriticalE
        {
            get => (bool)this["SEIsExTapCriticalE"];
            set => this["SEIsExTapCriticalE"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsExTapChangeTraceS
        {
            get => (bool)this["SEIsExTapChangeTraceS"];
            set => this["SEIsExTapChangeTraceS"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsExTapChangeTraceE
        {
            get => (bool)this["SEIsExTapChangeTraceE"];
            set => this["SEIsExTapChangeTraceE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsExTapDeleteS
        {
            get => (bool)this["SEIsExTapDeleteS"];
            set => this["SEIsExTapDeleteS"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsExTapDeleteE
        {
            get => (bool)this["SEIsExTapDeleteE"];
            set => this["SEIsExTapDeleteE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool STEIsExTapCritical // SlidesTEp 中継点をクリティカルにするか
        {
            get => (bool)this["STEIsExTapCritical"];
            set => this["STEIsExTapCritical"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool STEIsExTapAttach // 無視中継点にするか
        {
            get => (bool)this["STEIsExTapAttach"];
            set => this["STEIsExTapAttach"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsExTapFadeN // GuideStart
        {
            get => (bool)this["GSIsExTapFadeN"];
            set => this["GSIsExTapFadeN"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsExTapFadeO 
        {
            get => (bool)this["GSIsExTapFadeO"];
            set => this["GSIsExTapFadeO"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsExTapFadeI 
        {
            get => (bool)this["GSIsExTapFadeI"];
            set => this["GSIsExTapFadeI"] = value;
        }


        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsTap2Critical // SlideStart
        {
            get => (bool)this["SSIsTap2Critical"];
            set => this["SSIsTap2Critical"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsTap2ChangeTraceS
        {
            get => (bool)this["SSIsTap2ChangeTraceS"];
            set => this["SSIsTap2ChangeTraceS"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsTap2ChangeTraceE
        {
            get => (bool)this["SSIsTap2ChangeTraceE"];
            set => this["SSIsTap2ChangeTraceE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsTap2DeleteS
        {
            get => (bool)this["SSIsTap2DeleteS"];
            set => this["SSIsTap2DeleteS"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsTap2DeleteE
        {
            get => (bool)this["SSIsTap2DeleteE"];
            set => this["SSIsTap2DeleteE"] = value;
        }


        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsTap2Critical // SlideEnd
        {
            get => (bool)this["SEIsTap2Critical"];
            set => this["SEIsTap2Critical"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsTap2CriticalE
        {
            get => (bool)this["SEIsTap2CriticalE"];
            set => this["SEIsTap2CriticalE"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsTap2ChangeTraceS
        {
            get => (bool)this["SEIsTap2ChangeTraceS"];
            set => this["SEIsTap2ChangeTraceS"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsTap2ChangeTraceE
        {
            get => (bool)this["SEIsTap2ChangeTraceE"];
            set => this["SEIsTap2ChangeTraceE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsTap2DeleteS
        {
            get => (bool)this["SEIsTap2DeleteS"];
            set => this["SEIsTap2DeleteS"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsTap2DeleteE
        {
            get => (bool)this["SEIsTap2DeleteE"];
            set => this["SEIsTap2DeleteE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool STEIsTap2Critical
        {
            get => (bool)this["STEIsTap2Critical"];
            set => this["STEIsTap2Critical"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool STEIsTap2Attach // 無視中継点にするか
        {
            get => (bool)this["STEIsTap2Attach"];
            set => this["STEIsTap2Attach"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsTap2FadeN // GuideStart
        {
            get => (bool)this["GSIsTap2FadeN"];
            set => this["GSIsTap2FadeN"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsTap2FadeO
        {
            get => (bool)this["GSIsTap2FadeO"];
            set => this["GSIsTap2FadeO"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsTap2FadeI
        {
            get => (bool)this["GSIsTap2FadeI"];
            set => this["GSIsTap2FadeI"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool SSIsExTap2Critical // SlideStart
        {
            get => (bool)this["SSIsExTap2Critical"];
            set => this["SSIsExTap2Critical"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsExTap2ChangeTraceS
        {
            get => (bool)this["SSIsExTap2ChangeTraceS"];
            set => this["SSIsExTap2ChangeTraceS"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsExTap2ChangeTraceE
        {
            get => (bool)this["SSIsExTap2ChangeTraceE"];
            set => this["SSIsExTap2ChangeTraceE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsExTap2DeleteS
        {
            get => (bool)this["SSIsExTap2DeleteS"];
            set => this["SSIsExTap2DeleteS"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsExTap2DeleteE
        {
            get => (bool)this["SSIsExTap2DeleteE"];
            set => this["SSIsExTap2DeleteE"] = value;
        }


        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsExTap2Critical // SlideEnd
        {
            get => (bool)this["SEIsExTap2Critical"];
            set => this["SEIsExTap2Critical"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsExTap2CriticalE
        {
            get => (bool)this["SEIsExTap2CriticalE"];
            set => this["SEIsExTap2CriticalE"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsExTap2ChangeTraceS
        {
            get => (bool)this["SEIsExTap2ChangeTraceS"];
            set => this["SEIsExTap2ChangeTraceS"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsExTap2ChangeTraceE
        {
            get => (bool)this["SEIsExTap2ChangeTraceE"];
            set => this["SEIsExTap2ChangeTraceE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsExTap2DeleteS
        {
            get => (bool)this["SEIsExTap2DeleteS"];
            set => this["SEIsExTap2DeleteS"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsExTap2DeleteE
        {
            get => (bool)this["SEIsExTap2DeleteE"];
            set => this["SEIsExTap2DeleteE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool STEIsExTap2Critical
        {
            get => (bool)this["STEIsExTap2Critical"];
            set => this["STEIsExTap2Critical"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool STEIsExTap2Attach // 無視中継点にするか
        {
            get => (bool)this["STEIsExTap2Attach"];
            set => this["STEIsExTap2Attach"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsExTap2FadeN // GuideStart
        {
            get => (bool)this["GSIsExTap2FadeN"];
            set => this["GSIsExTap2FadeN"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsExTap2FadeO
        {
            get => (bool)this["GSIsExTap2FadeO"];
            set => this["GSIsExTap2FadeO"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsExTap2FadeI
        {
            get => (bool)this["GSIsExTap2FadeI"];
            set => this["GSIsExTap2FadeI"] = value;
        }



        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsFlickCritical // SlideStart
        {
            get => (bool)this["SSIsFlickCritical"];
            set => this["SSIsFlickCritical"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool SSIsFlickChangeTraceS
        {
            get => (bool)this["SSIsFlickChangeTraceS"];
            set => this["SSIsFlickChangeTraceS"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsFlickChangeTraceE
        {
            get => (bool)this["SSIsFlickChangeTraceE"];
            set => this["SSIsFlickChangeTraceE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsFlickDeleteS
        {
            get => (bool)this["SSIsFlickDeleteS"];
            set => this["SSIsFlickDeleteS"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsFlickDeleteE
        {
            get => (bool)this["SSIsFlickDeleteE"];
            set => this["SSIsFlickDeleteE"] = value;
        }


        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsFlickCritical // SlideEnd
        {
            get => (bool)this["SEIsFlickCritical"];
            set => this["SEIsFlickCritical"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsFlickCriticalE
        {
            get => (bool)this["SEIsFlickCriticalE"];
            set => this["SEIsFlickCriticalE"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsFlickChangeTraceS
        {
            get => (bool)this["SEIsFlickChangeTraceS"];
            set => this["SEIsFlickChangeTraceS"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool SEIsFlickChangeTraceE
        {
            get => (bool)this["SEIsFlickChangeTraceE"];
            set => this["SEIsFlickChangeTraceE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsFlickDeleteS
        {
            get => (bool)this["SEIsFlickDeleteS"];
            set => this["SEIsFlickDeleteS"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsFlickDeleteE
        {
            get => (bool)this["SEIsFlickDeleteE"];
            set => this["SEIsFlickDeleteE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool STEIsFlickCritical 
        {
            get => (bool)this["STEIsFlickCritical"];
            set => this["STEIsFlickCritical"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool STEIsFlickAttach // 無視中継点にするか
        {
            get => (bool)this["STEIsFlickAttach"];
            set => this["STEIsFlickAttach"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsFlickFadeN // GuideStart
        {
            get => (bool)this["GSIsFlickFadeN"];
            set => this["GSIsFlickFadeN"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsFlickFadeO
        {
            get => (bool)this["GSIsFlickFadeO"];
            set => this["GSIsFlickFadeO"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsFlickFadeI
        {
            get => (bool)this["GSIsFlickFadeI"];
            set => this["GSIsFlickFadeI"] = value;
        }


        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsDamageCritical // SlideStart
        {
            get => (bool)this["SSIsDamageCritical"];
            set => this["SSIsDamageCritical"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsDamageChangeTraceS
        {
            get => (bool)this["SSIsDamageChangeTraceS"];
            set => this["SSIsDamageChangeTraceS"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsDamageChangeTraceE
        {
            get => (bool)this["SSIsDamageChangeTraceE"];
            set => this["SSIsDamageChangeTraceE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool SSIsDamageDeleteS
        {
            get => (bool)this["SSIsDamageDeleteS"];
            set => this["SSIsDamageDeleteS"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsDamageDeleteE
        {
            get => (bool)this["SSIsDamageDeleteE"];
            set => this["SSIsDamageDeleteE"] = value;
        }


        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsDamageCritical // SlideEnd
        {
            get => (bool)this["SEIsDamageCritical"];
            set => this["SEIsDamageCritical"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsDamageCriticalE
        {
            get => (bool)this["SEIsDamageCriticalE"];
            set => this["SEIsDamageCriticalE"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsDamageChangeTraceS
        {
            get => (bool)this["SEIsDamageChangeTraceS"];
            set => this["SEIsDamageChangeTraceS"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsDamageChangeTraceE
        {
            get => (bool)this["SEIsDamageChangeTraceE"];
            set => this["SEIsDamageChangeTraceE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsDamageDeleteS
        {
            get => (bool)this["SEIsDamageDeleteS"];
            set => this["SEIsDamageDeleteS"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool SEIsDamageDeleteE
        {
            get => (bool)this["SEIsDamageDeleteE"];
            set => this["SEIsDamageDeleteE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool STEIsDamageCritical 
        {
            get => (bool)this["STEIsDamageCritical"];
            set => this["STEIsDamageCritical"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool STEIsDamageAttach // 無視中継点にするか
        {
            get => (bool)this["STEIsDamageAttach"];
            set => this["STEIsDamageAttach"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsDamageFadeN // GuideStart
        {
            get => (bool)this["GSIsDamageFadeN"];
            set => this["GSIsDamageFadeN"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsDamageFadeO
        {
            get => (bool)this["GSIsDamageFadeO"];
            set => this["GSIsDamageFadeO"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsDamageFadeI
        {
            get => (bool)this["GSIsDamageFadeI"];
            set => this["GSIsDamageFadeI"] = value;
        }


        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsFlick2Critical // SlideStart
        {
            get => (bool)this["SSIsFlick2Critical"];
            set => this["SSIsFlick2Critical"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool SSIsFlick2ChangeTraceS
        {
            get => (bool)this["SSIsFlick2ChangeTraceS"];
            set => this["SSIsFlick2ChangeTraceS"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsFlick2ChangeTraceE
        {
            get => (bool)this["SSIsFlick2ChangeTraceE"];
            set => this["SSIsFlick2ChangeTraceE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsFlick2DeleteS
        {
            get => (bool)this["SSIsFlick2DeleteS"];
            set => this["SSIsFlick2DeleteS"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsFlick2DeleteE
        {
            get => (bool)this["SSIsFlick2DeleteE"];
            set => this["SSIsFlick2DeleteE"] = value;
        }


        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsFlick2Critical // SlideEnd
        {
            get => (bool)this["SEIsFlick2Critical"];
            set => this["SEIsFlick2Critical"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsFlick2CriticalE
        {
            get => (bool)this["SEIsFlick2CriticalE"];
            set => this["SEIsFlick2CriticalE"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsFlick2ChangeTraceS
        {
            get => (bool)this["SEIsFlick2ChangeTraceS"];
            set => this["SEIsFlick2ChangeTraceS"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool SEIsFlick2ChangeTraceE
        {
            get => (bool)this["SEIsFlick2ChangeTraceE"];
            set => this["SEIsFlick2ChangeTraceE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsFlick2DeleteS
        {
            get => (bool)this["SEIsFlick2DeleteS"];
            set => this["SEIsFlick2DeleteS"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsFlick2DeleteE
        {
            get => (bool)this["SEIsFlick2DeleteE"];
            set => this["SEIsFlick2DeleteE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool STEIsFlick2Critical // SlidesTEp
        {
            get => (bool)this["STEIsFlick2Critical"];
            set => this["STEIsFlick2Critical"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool STEIsFlick2Attach // 無視中継点にするか
        {
            get => (bool)this["STEIsFlick2Attach"];
            set => this["STEIsFlick2Attach"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsFlick2FadeN // GuideStart
        {
            get => (bool)this["GSIsFlick2FadeN"];
            set => this["GSIsFlick2FadeN"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsFlick2FadeO
        {
            get => (bool)this["GSIsFlick2FadeO"];
            set => this["GSIsFlick2FadeO"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsFlick2FadeI
        {
            get => (bool)this["GSIsFlick2FadeI"];
            set => this["GSIsFlick2FadeI"] = value;
        }


        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsDamage2Critical // SlideStart
        {
            get => (bool)this["SSIsDamage2Critical"];
            set => this["SSIsDamage2Critical"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsDamage2ChangeTraceS
        {
            get => (bool)this["SSIsDamage2ChangeTraceS"];
            set => this["SSIsDamage2ChangeTraceS"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsDamage2ChangeTraceE
        {
            get => (bool)this["SSIsDamage2ChangeTraceE"];
            set => this["SSIsDamage2ChangeTraceE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool SSIsDamage2DeleteS
        {
            get => (bool)this["SSIsDamage2DeleteS"];
            set => this["SSIsDamage2DeleteS"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SSIsDamage2DeleteE
        {
            get => (bool)this["SSIsDamage2DeleteE"];
            set => this["SSIsDamage2DeleteE"] = value;
        }


        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsDamage2Critical // SlideEnd
        {
            get => (bool)this["SEIsDamage2Critical"];
            set => this["SEIsDamage2Critical"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsDamage2CriticalE
        {
            get => (bool)this["SEIsDamage2CriticalE"];
            set => this["SEIsDamage2CriticalE"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsDamage2ChangeTraceS
        {
            get => (bool)this["SEIsDamage2ChangeTraceS"];
            set => this["SEIsDamage2ChangeTraceS"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsDamage2ChangeTraceE
        {
            get => (bool)this["SEIsDamage2ChangeTraceE"];
            set => this["SEIsDamage2ChangeTraceE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool SEIsDamage2DeleteS
        {
            get => (bool)this["SEIsDamage2DeleteS"];
            set => this["SEIsDamage2DeleteS"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool SEIsDamage2DeleteE
        {
            get => (bool)this["SEIsDamage2DeleteE"];
            set => this["SEIsDamage2DeleteE"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool STEIsDamage2Critical // SlidesTEp 中継点をクリティカルにするか
        {
            get => (bool)this["STEIsDamage2Critical"];
            set => this["STEIsDamage2Critical"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool STEIsDamage2Attach // 無視中継点にするか
        {
            get => (bool)this["STEIsDamage2Attach"];
            set => this["STEIsDamage2Attach"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsDamage2FadeN // GuideStart
        {
            get => (bool)this["GSIsDamage2FadeN"];
            set => this["GSIsDamage2FadeN"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsDamage2FadeO
        {
            get => (bool)this["GSIsDamage2FadeO"];
            set => this["GSIsDamage2FadeO"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool GSIsDamage2FadeI
        {
            get => (bool)this["GSIsDamage2FadeI"];
            set => this["GSIsDamage2FadeI"] = value;
        }






        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool IsAllowStepChannel
        {
            get => (bool)this["IsAllowStepChannel"];
            set => this["IsAllowStepChannel"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("1")]
        public float ScrollAmount
        {
            get => (float)this["ScrollAmount"];
            set => this["ScrollAmount"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool IsPJsekaiSounds
        {
            get => (bool)this["IsPJsekaiSounds"];
            set => this["IsPJsekaiSounds"] = value;
        }
        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool IsAccurateOverlap
        {
            get => (bool)this["IsAccurateOverlap"];
            set => this["IsAccurateOverlap"] = value;
        }

    }
}
