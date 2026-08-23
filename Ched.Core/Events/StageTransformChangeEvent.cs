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
    public class StageTransformChangeEvent : EventBase
    {
        [Newtonsoft.Json.JsonProperty]
        private int stage;
        [Newtonsoft.Json.JsonProperty]
        private float rotation;
        [Newtonsoft.Json.JsonProperty]
        private float xTrans;
        [Newtonsoft.Json.JsonProperty]
        private float yTrans;
        [Newtonsoft.Json.JsonProperty]
        private int anchor;
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


        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }
        public float XTranslation
        {
            get { return xTrans; }
            set { xTrans = value; }
        }
        public float YTranslation
        {
            get { return yTrans; }
            set { yTrans = value; }
        }
        public int Anchor
        {
            get { return anchor; }
            set { anchor= value; }
        }
        public int Ease
        {
            get { return ease; }
            set { ease = value; }
        }


    }
}
