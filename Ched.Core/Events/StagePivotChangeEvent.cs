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
    [DebuggerDisplay("Tick = {Tick}")]
    public class StagePivotChangeEvent : EventBase
    {
        [Newtonsoft.Json.JsonProperty]
        private int stage;
        [Newtonsoft.Json.JsonProperty]
        private float laneIndex;
        [Newtonsoft.Json.JsonProperty]
        private int divisionSize;
        [Newtonsoft.Json.JsonProperty]
        private int divisionParity;
        [Newtonsoft.Json.JsonProperty]
        private float yOffset;
        [Newtonsoft.Json.JsonProperty]
        private float yBeatOffset;
        [Newtonsoft.Json.JsonProperty]
        private int ease;


        /// <summary>
        /// ステージを設定します。
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
        public int DivisionSize
        {
            get { return divisionSize; }
            set { divisionSize = value; }
        }
        public int DivisionParity
        {
            get { return divisionParity; }
            set { divisionParity = value; }
        }
        public float YOffset
        {
            get { return yOffset; }
            set { yOffset = value; }
        }
        public float YBeatOffset
        {
            get { return yBeatOffset; }
            set { yBeatOffset = value; }
        }
        public int Ease
        {
            get { return ease; }
            set { ease = value; }
        }


    }
}
