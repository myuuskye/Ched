using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ched.Core.Notes.Guide;

namespace Ched.Core.Notes
{
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class Marker : MovableLongNoteBase
    {
        [Newtonsoft.Json.JsonProperty]
        private float startWidth = 1;
        [Newtonsoft.Json.JsonProperty]
        private float startLaneIndex;
        [Newtonsoft.Json.JsonProperty]
        private int channel = 1;
        [Newtonsoft.Json.JsonProperty]
        private int duration = 1;
        [Newtonsoft.Json.JsonProperty]
        private List<string> tags = new List<string>();
        [Newtonsoft.Json.JsonProperty]
        private string name = "New Line";
        [Newtonsoft.Json.JsonProperty]
        private int colorR;
        [Newtonsoft.Json.JsonProperty]
        private int colorG;
        [Newtonsoft.Json.JsonProperty]
        private int colorB;

        [Newtonsoft.Json.JsonProperty]
        private List<StepTap> stepNotes = new List<StepTap>();


        /// <summary>
        /// 開始ノートの配置されるレーン番号を設定します。。
        /// </summary>
        public float StartLaneIndex
        {
            get { return startLaneIndex; }
            set
            {
                CheckPosition(value, startWidth);
                startLaneIndex = value;
            }
        }

        /// <summary>
        /// 開始ノートのレーン幅を設定します。
        /// </summary>
        public float StartWidth
        {
            get { return startWidth; }
            set
            {
                CheckPosition(startLaneIndex, value);
                startWidth = value;
            }
        }
        public List<string> Tags
        {
            get { return tags; }
            set
            {
                tags = value;
            }
        }
        /// <summary>
        /// ノートのレーン幅を設定します。
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// ノートのチャンネルを設定します。
        /// </summary>
        public int Channel
        {
            get { return channel; }
            set
            {
                channel = value;
            }
        }

        /// <summary>
        /// ノートの長さを設定します。
        /// </summary>
        public int Duration
        {
            get { return duration; }
            set
            {
                if (duration == value) return;
                if (duration <= 0) throw new ArgumentOutOfRangeException("value", "value must be positive.");
                duration = value;
            }
        }

        /// <summary>
        /// カラーを設定します。
        /// </summary>
        public int MarkerColorR
        {
            get { return colorR; }
            set { colorR = value; }
        }
        /// <summary>
        /// カラーを設定します。
        /// </summary>
        public int MarkerColorG
        {
            get { return colorG; }
            set { colorG = value; }
        }
        /// <summary>
        /// カラーを設定します。
        /// </summary>
        public int MarkerColorB
        {
            get { return colorB; }
            set { colorB = value; }
        }

        protected void CheckPosition(float startLaneIndex, float startWidthth)
        {
            float maxRightOffset = Math.Max(0, StepNotes.Count == 0 ? 0 : StepNotes.Max(p => p.LaneIndexOffset + p.WidthChange));
            /*
            if (startWidth < Math.Abs(Math.Min(0, StepNotes.Count == 0 ? 0 : StepNotes.Min(p => p.WidthChange))) + 0.1 || startLaneIndex + startWidth + maxRightOffset > Constants.LanesCount)
                throw new ArgumentOutOfRangeException("startWidth", "Invalid note width.");
                
            if (StepNotes.Any(p =>
            {
                float laneIndex = startLaneIndex + p.LaneIndexOffset;
                return laneIndex < constants.MinusLaneCount -8 || laneIndex + (startWidth + p.WidthChange) > constants.LaneCount + 8;
            })) throw new ArgumentOutOfRangeException("startLaneIndex", "Invalid lane index.");
            if (startLaneIndex < constants.MinusLaneCount || startLaneIndex + startWidth > constants.LaneCount)
                throw new ArgumentOutOfRangeException("startLaneIndex", "Invalid lane index.");
            */
        }

        public void SetPosition(float lane, float width)
        {
            CheckPosition(lane, width);
            this.startLaneIndex = lane;
            this.startWidth = width;
        }

        public List<StepTap> StepNotes { get { return stepNotes; } }
        public StartTap StartNote { get; }

        public Marker()
        {
            StartNote = new StartTap(this);
        }


        public override int GetDuration()
        {
            return Duration;
        }

        [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
        public abstract class TapBase : LongNoteTapBase
        {
            [Newtonsoft.Json.JsonProperty]
            protected Marker parent;

            public Marker ParentNote { get { return parent; } }

            public TapBase(Marker parent)
            {
                this.parent = parent;
            }
        }

        public class StartTap : TapBase, IAirable
        {

            public override bool IsTap { get { return false; } }

            public override int Tick { get { return ParentNote.StartTick; } }

            public override float LaneIndex { get { return ParentNote.StartLaneIndex; } }

            public override float Width { get { return ParentNote.StartWidth; } }

            public override int Channel { get { return ParentNote.Channel; } set { ParentNote.Channel = value; } }

            public StartTap(Marker parent) : base(parent)
            {
                
            }
        }

        [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
        public class StepTap : TapBase, IAirable
        {
            [Newtonsoft.Json.JsonProperty]
            private float laneIndexOffset;
            [Newtonsoft.Json.JsonProperty]
            private float widthChange;
            [Newtonsoft.Json.JsonProperty]
            private int tickOffset = 1;
            [Newtonsoft.Json.JsonProperty]
            private int channel;



            public int TickOffset
            {
                get { return tickOffset; }
                set
                {
                    //if (value <= 0) throw new ArgumentOutOfRangeException("value", "value must be positive.");
                    tickOffset = value;
                }
            }

            public override int Tick { get { return parent.StartTick + TickOffset; } }
            public override bool IsTap { get { return false; } }

            public override float LaneIndex { get { return parent.StartLaneIndex + LaneIndexOffset; } }

            public override int Channel
            {
                get { return channel; }
                set { channel = value; }
            }

            public float LaneIndexOffset
            {
                get { return laneIndexOffset; }
                set
                {
                    CheckPosition(value, widthChange);
                    laneIndexOffset = value;
                }
            }

            public float WidthChange
            {
                get { return widthChange; }
                set
                {
                    CheckPosition(laneIndexOffset, value);
                    widthChange = value;
                }
            }

            public override float Width { get { return ParentNote.StartWidth + WidthChange; } }

            public StepTap(Marker parent) : base(parent)
            {
            }

            public void SetPosition(float laneIndexOffset, float widthChange)
            {
                CheckPosition(laneIndexOffset, widthChange);
                this.laneIndexOffset = laneIndexOffset;
                this.widthChange = widthChange;
            }


            protected void CheckPosition(float laneIndexOffset, float widthChange)
            {
                float laneIndex = ParentNote.StartNote.LaneIndex + laneIndexOffset;
                //if (laneIndex < constants.MinusLaneCount || laneIndex + (ParentNote.StartWidth + widthChange) > constants.LaneCount)
                //throw new ArgumentOutOfRangeException("laneIndexOffset", "Invalid lane index offset.");

                float actualWidth = widthChange + ParentNote.StartWidth;
                //if (actualWidth < 0.01 )
                //throw new ArgumentOutOfRangeException("widthChange", "Invalid width change value.");
            }
        }

        public enum MarkerColor
        {
            pink,
            lime,
            blue,
            orange,
            custom
        }
    }
}
