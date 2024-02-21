﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ched.Core.Notes
{
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class Hold : MovableLongNoteBase
    {
        [Newtonsoft.Json.JsonProperty]
        private float laneIndex;
        [Newtonsoft.Json.JsonProperty]
        private float width = 1;
        [Newtonsoft.Json.JsonProperty]
        private int channel = 1;
        [Newtonsoft.Json.JsonProperty]
        private int duration = 1;

        [Newtonsoft.Json.JsonProperty]
        private StartTap startNote;
        [Newtonsoft.Json.JsonProperty]
        private EndTap endNote;

        /// <summary>
        /// ノートの配置されるレーン番号を設定します。。
        /// </summary>
        public float LaneIndex
        {
            get { return laneIndex; }
            set
            {
                CheckPosition(value, Width);
                laneIndex = (float)value;
            }
        }

        /// <summary>
        /// ノートのレーン幅を設定します。
        /// </summary>
        public float Width
        {
            get { return width; }
            set
            {
                CheckPosition(LaneIndex, value);
                width = (float)value;
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

        protected void CheckPosition(float laneIndex, float width)
        {
            if (width < 0.1)
                throw new ArgumentOutOfRangeException("width", "Invalid width.");
            if (laneIndex < -16)
                throw new ArgumentOutOfRangeException("laneIndex", "Invalid lane index.");
        }

        public void SetPosition(float laneIndex, float width)
        {
            CheckPosition(laneIndex, width);
            this.laneIndex = laneIndex;
            this.width = width;
        }
        public void SetPosition(int laneIndex, int width)
        {
            CheckPosition(laneIndex, width);
            this.laneIndex = laneIndex;
            this.width = width;
        }

        public StartTap StartNote { get { return startNote; } }
        public EndTap EndNote { get { return endNote; } }

        public Hold()
        {
            startNote = new StartTap(this);
            endNote = new EndTap(this);
        }

        public override int GetDuration()
        {
            return Duration;
        }

        [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
        public abstract class TapBase : LongNoteTapBase
        {
            protected Hold parent;

            public override float LaneIndex { get { return parent.LaneIndex; } }

            public override float Width { get { return parent.Width; } }

            public TapBase(Hold parent)
            {
                this.parent = parent;
            }
        }

        public class StartTap : TapBase
        {
            public override int Tick { get { return parent.StartTick; } }

            public override int Channel { get { return parent.Channel; } set { parent.Channel = value; } }

            public override bool IsTap { get { return true; } }
            public override float LaneIndex { get { return parent.LaneIndex; } }
            public override float Width { get { return parent.Width; } }

            public StartTap(Hold parent) : base(parent)
            {
            }
        }

        public class EndTap : TapBase
        {
            public override bool IsTap { get { return false; } }

            public override int Channel { get { return parent.Channel; } set { parent.Channel = value; } }

            public override int Tick { get { return parent.StartTick + parent.Duration; } }
            public override float LaneIndex { get { return parent.LaneIndex; } }
            public override float Width { get { return parent.Width; } }

            public EndTap(Hold parent) : base(parent)
            {
            }
        }
    }
}
