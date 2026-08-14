using Newtonsoft.Json.Linq;
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
    public class CameraChangeEvent : EventBase
    {
        [Newtonsoft.Json.JsonProperty]
        private float laneIndex;
        [Newtonsoft.Json.JsonProperty]
        private float width;
        [Newtonsoft.Json.JsonProperty]
        private float zoom;
        [Newtonsoft.Json.JsonProperty]
        private float zoomTargetLane;
        [Newtonsoft.Json.JsonProperty]
        private float zoomTargetY;
        [Newtonsoft.Json.JsonProperty]
        private int zoomVerticalAlign;
        [Newtonsoft.Json.JsonProperty]
        private float rotation;
        [Newtonsoft.Json.JsonProperty]
        private float tilt;
        [Newtonsoft.Json.JsonProperty]
        private int ease;



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
        public float Zoom
        {
            get { return zoom; }
            set { zoom = value; }
        }
        public float ZoomTargetLane
        {
            get { return zoomTargetLane; }
            set { zoomTargetLane = value; }
        }
        public float ZoomTartgetY
        {
            get { return zoomTargetY; }
            set { zoomTargetY = value; }
        }
        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }
        public float Tilt
        {
            get { return tilt; }
            set { tilt = value; }
        }
        public int ZoomVerticalAlign
        {
            get { return zoomVerticalAlign; }
            set { zoomVerticalAlign = value; }
        }
        public int Ease
        {
            get { return ease; }
            set { ease = value; }
        }


    }
}
