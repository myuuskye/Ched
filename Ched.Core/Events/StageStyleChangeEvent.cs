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
    public class StageStyleChangeEvent : EventBase
    {
        [Newtonsoft.Json.JsonProperty]
        private int stage;
        [Newtonsoft.Json.JsonProperty]
        private float laneIndex;
        [Newtonsoft.Json.JsonProperty]
        private int judgeLineColor;
        [Newtonsoft.Json.JsonProperty]
        private int judgeLineStyle;
        [Newtonsoft.Json.JsonProperty]
        private int leftBorderStyle;
        [Newtonsoft.Json.JsonProperty]
        private int rightBorderStyle;
        [Newtonsoft.Json.JsonProperty]
        private int fullWidth;
        [Newtonsoft.Json.JsonProperty]
        private float noteAlpha;
        [Newtonsoft.Json.JsonProperty]
        private float laneAlpha;
        [Newtonsoft.Json.JsonProperty]
        private float judgeLineAlpha;
        [Newtonsoft.Json.JsonProperty]
        private float divisionLineAlpha;
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
        public int JudgeLineColor
        {
            get { return judgeLineColor; }
            set { judgeLineColor= value; }
        }
        public int JudgeLineStyle
        {
            get { return judgeLineStyle; }
            set { judgeLineStyle = value; }
        }
        public int LeftBorderStyle
        {
            get { return leftBorderStyle; }
            set { leftBorderStyle = value; }
        }
        public int RightBorderStyle
        {
            get { return rightBorderStyle; }
            set { rightBorderStyle = value; }
        }
        public int FullWidth
        {
            get { return fullWidth; }
            set { fullWidth = value; }
        }
        public float NoteAlpha
        {
            get { return noteAlpha; }
            set { noteAlpha = value; }
        }
        public float LaneAlpha
        {
            get { return laneAlpha; }
            set { laneAlpha = value; }
        }
        public float JudgeLineAlpha
        {
            get { return judgeLineAlpha; }
            set { judgeLineAlpha = value; }
        }
        public float DivisonLineAlpha
        {
            get { return divisionLineAlpha; }
            set { divisionLineAlpha = value; }
        }
        public int Ease
        {
            get { return ease; }
            set { ease = value; }
        }


    }
}
