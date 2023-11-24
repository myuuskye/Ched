using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

using Ched.UI;

namespace Ched.Configuration
{
    internal sealed class SoundSettings : SettingsBase
    {
        public static SoundSettings Default { get; } = (SoundSettings)Synchronized(new SoundSettings());

        private SoundSettings()
        {
        }

        // ref: https://stackoverflow.com/a/12807699
        [UserScopedSetting]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        [DefaultSettingValue("")] // empty dictionary
        public Dictionary<string, SoundSource> ScoreSound
        {
            get { return (Dictionary<string, SoundSource>)this["ScoreSound"]; }
            set { this["ScoreSound"] = value; }
        }

        [UserScopedSetting]
        public SoundSource GuideSound
        {
            
            get => (SoundSource)this["GuideSound"] ?? new SoundSource("Sounds/guide.mp3", 0.036);
            set => this["GuideSound"] = value;
        }
        [UserScopedSetting]
        public SoundSource TapSound
        {

            get => (SoundSource)this["TapSound"] ?? new SoundSource("Sounds/tap.mp3", 0.036);
            set => this["TapSound"] = value;
        }
        [UserScopedSetting]
        public SoundSource ExTapSound
        {

            get => (SoundSource)this["ExTapSound"] ?? new SoundSource("Sounds/extap.mp3", 0.036);
            set => this["ExTapSound"] = value;
        }
        [UserScopedSetting]
        public SoundSource AirSound
        {

            get => (SoundSource)this["AirSound"] ?? new SoundSource("Sounds/flick.mp3", 0.036);
            set => this["AirSound"] = value;
        }
        [UserScopedSetting]
        public SoundSource ExAirSound
        {

            get => (SoundSource)this["ExAirSound"] ?? new SoundSource("Sounds/exflick.mp3", 0.036);
            set => this["ExAirSound"] = value;
        }
        [UserScopedSetting]
        public SoundSource TraceSound
        {

            get => (SoundSource)this["TraceSound"] ?? new SoundSource("Sounds/trace.mp3", 0.036);
            set => this["TraceSound"] = value;
        }
        [UserScopedSetting]
        public SoundSource ExTraceSound
        {

            get => (SoundSource)this["ExTraceSound"] ?? new SoundSource("Sounds/extrace.mp3", 0.036);
            set => this["ExTraceSound"] = value;
        }
        [UserScopedSetting]
        public SoundSource StepSound
        {

            get => (SoundSource)this["StepSound"] ?? new SoundSource("Sounds/step.mp3", 0.036);
            set => this["StepSound"] = value;
        }
        [UserScopedSetting]
        public SoundSource ExStepSound
        {

            get => (SoundSource)this["ExStepSound"] ?? new SoundSource("Sounds/exstep.mp3", 0.036);
            set => this["ExStepSound"] = value;
        }
    }
}
