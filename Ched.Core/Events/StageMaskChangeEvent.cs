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
    public class StageMaskChangeEvent : EventBase
    {
        [Newtonsoft.Json.JsonProperty]
        private int stage;
        [Newtonsoft.Json.JsonProperty]
        private float laneIndex;
        [Newtonsoft.Json.JsonProperty]
        private float width;
        [Newtonsoft.Json.JsonProperty]
        private int ease;


        /// <summary>
        /// ハイスピードのチャンネルを設定します。
        /// </summary>
        public int Stage
        {
            get { return stage; }
            set { stage = value; }
        }


        public float LaneIndex
        {
            get { return laneIndex; }
            set { laneIndex = value; }
        }
        public float Width
        {
            get { return width; }
            set { width = value; }
        }
        public int Ease
        {
            get { return ease; }
            set { ease = value; }
        }


    }
}
