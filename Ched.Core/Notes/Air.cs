﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ched.Core.Notes
{
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class Air
    {
        [Newtonsoft.Json.JsonProperty]
        private IAirable parentNote;
        [Newtonsoft.Json.JsonProperty]
        private VerticalAirDirection verticalDirection;
        [Newtonsoft.Json.JsonProperty]
        private HorizontalAirDirection horizontalDirection;

        public IAirable ParentNote { get { return parentNote; } }

        public VerticalAirDirection VerticalDirection
        {
            get { return verticalDirection; }
            set { verticalDirection = value; }
        }

        public HorizontalAirDirection HorizontalDirection
        {
            get { return horizontalDirection; }
            set { horizontalDirection = value; }
        }

        public int Tick { get { return ParentNote.Tick; } }

        public float LaneIndex { get { return ParentNote.LaneIndex; } }

        public float Width { get { return ParentNote.Width; } }
        public int Channel { get { return ParentNote.Channel; } }

        public Air(IAirable parent)
        {
            parentNote = parent;
        }

        public void Flip()
        {
            if (HorizontalDirection == HorizontalAirDirection.Center) return;
            HorizontalDirection = HorizontalDirection == HorizontalAirDirection.Left ? HorizontalAirDirection.Right : HorizontalAirDirection.Left;
        }

    }

    public interface IAirable
    {
        /// <summary>
        /// ノートの位置を表すTickを設定します。
        /// </summary>
        int Tick { get; }

        /// <summary>
        /// ノートの配置されるレーン番号を取得します。。
        /// </summary>
        float LaneIndex { get; }

        /// <summary>
        /// ノートのレーン幅を取得します。
        /// </summary>
        float Width { get; }

        /// <summary>
        /// ノートのチャンネルを取得します。
        /// </summary>
        int Channel { get; }

    }


    public enum VerticalAirDirection
    {
        Up,
        Down,
        Other
    }

    public enum HorizontalAirDirection
    {
        Center,
        Left,
        Right
    }
}
