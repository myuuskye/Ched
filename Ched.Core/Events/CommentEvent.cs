using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ched.Core.Events
{
    /// <summary>
    /// Commentイベントを表すクラスです。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    [DebuggerDisplay("Tick = {Tick}, Value = {Comment}")]
    public class CommentEvent : EventBase
    {
        [Newtonsoft.Json.JsonProperty]
        private string comment;
        [Newtonsoft.Json.JsonProperty]
        private int color;
        [Newtonsoft.Json.JsonProperty]
        private float size;

        public string Comment
        {
            get { return comment; }
            set { comment = value; }
        }

        public int Color
        {
            get { return color; }
            set { color = value; }
        }

        public float Size
        {
            get { return size; }
            set { size = value; }
        }
        public int Type = -3;
    }
}
