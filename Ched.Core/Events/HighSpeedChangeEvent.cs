using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ched.Core.Events
{
    /// <summary>
    /// ハイスピードの変更を表すクラスです。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    [DebuggerDisplay("Tick = {Tick}, Value = {SpeedRatio}, Ch = {Channel}")]
    public class HighSpeedChangeEvent : EventBase
    {
        [Newtonsoft.Json.JsonProperty]
        private decimal speedRatio;
        [Newtonsoft.Json.JsonProperty]
        private int speedCh;
        [Newtonsoft.Json.JsonProperty]
        private string customArgs;
        [Newtonsoft.Json.JsonProperty]
        private float skip;
        [Newtonsoft.Json.JsonProperty]
        private int ease;
        [Newtonsoft.Json.JsonProperty]
        private int hideNotes;
        [Newtonsoft.Json.JsonProperty]
        private float editLaneIndex = 8;



        /// <summary>
        /// 1を基準とする速度比を設定します。
        /// </summary>
        public decimal SpeedRatio
        {
            get { return speedRatio; }
            set { speedRatio = value; }
        }
        /// <summary>
        /// ハイスピードのチャンネルを設定します。
        /// </summary>
        public int SpeedCh
        {
            get { return speedCh; }
            set { speedCh = value; }
        }

        public string CustomArgs
        {
            get { return customArgs; }
            set { customArgs = value; }
        }

        public float Skip
        {
            get { return skip; }
            set { skip = value; }
        }
        public int Ease
        {
            get { return ease; }
            set { ease = value; }
        }
        public int HideNotes
        {
            get { return hideNotes; }
            set { hideNotes = value; }
        }
        public float EditLaneIndex
        {
            get { return editLaneIndex; }
            set { editLaneIndex = value; }
        }

    }
}
