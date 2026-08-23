using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

using Ched.Core.Notes;

namespace Ched.Core
{
    /// <summary>
    /// ノーツを格納するコレクションを表すクラスです。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class Stage
    {
        [Newtonsoft.Json.JsonProperty]
        private int id;
        [Newtonsoft.Json.JsonProperty]
        private int number;
        [Newtonsoft.Json.JsonProperty]
        private string name;
        [Newtonsoft.Json.JsonProperty]
        private bool start;
        [Newtonsoft.Json.JsonProperty]
        private bool end;
        [Newtonsoft.Json.JsonProperty]
        private bool simlines;
        [Newtonsoft.Json.JsonProperty]
        private NoteCollection notes = new NoteCollection();
        [Newtonsoft.Json.JsonProperty]
        private EventCollection events = new EventCollection();

        /// <summary>
        /// ステージID
        /// </summary>
        public int ID
        {
            get { return id; }
            set { id = value; }
        }
        /// <summary>
        /// ステージの数字
        /// </summary>
        public int Number
        {
            get { return number; }
            set { number = value; }
        }
        /// <summary>
        /// ステージの名前
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        /// <summary>
        /// 最初からか
        /// </summary>
        public bool FromStart
        {
            get { return start; }
            set { start = value; }
        }
        /// <summary>
        /// 最後までか
        /// </summary>
        public bool UntilEnd
        {
            get { return end; }
            set { end = value; }
        }
        /// <summary>
        /// 同時線を他のステージと共有しないか true しない false する
        /// </summary>
        public bool SimLines 
        {
            get { return simlines; }
            set { simlines = value; }
        }
        /// <summary>
        /// ノーツを格納するコレクションです。
        /// </summary>
        public NoteCollection Notes
        {
            get { return notes; }
            set { notes = value; }
        }

        /// <summary>
        /// イベントを格納するコレクションです。
        /// </summary>
        public EventCollection Events
        {
            get { return events; }
            set { events = value; }
        }

        public Stage()
        {
            Name = "stagename";
            Number = 0;
            Notes = new NoteCollection();
            Events = new EventCollection();
        }

        public Stage(Stage stage)
        {
            ID = stage.ID;
            Name = stage.Name;
            Number = stage.Number;
            Notes = stage.Notes;
            Events = stage.Events;
            FromStart = stage.FromStart;
            UntilEnd = stage.UntilEnd;
            SimLines = stage.SimLines;
        }

        public NoteCollection GetNotes()
        {
            return Notes;
        }
        public EventCollection GetEvents()
        {
            return Events;
        }
    }
}
