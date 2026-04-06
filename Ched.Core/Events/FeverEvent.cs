using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ched.Core.Events
{
    /// <summary>
    /// BPMの変更イベントを表すクラスです。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    [DebuggerDisplay("Tick = {Tick}, Value = {Start}")]
    public class FeverEvent : EventBase
    {
        [Newtonsoft.Json.JsonProperty]
        private bool start;
        [Newtonsoft.Json.JsonProperty]
        private bool force;

        public bool Start
        {
            get { return start; }
            set { start = value; }
        }
        public bool Force
        {
            get { return force; }
            set { force = value; }
        }


        public int Type = -1;

    }
}
