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
    public class Channel
    {
        [Newtonsoft.Json.JsonProperty]
        private int speedCh;
        [Newtonsoft.Json.JsonProperty]
        private string name;
        [Newtonsoft.Json.JsonProperty]
        private decimal forceNoteSpeed;
        [Newtonsoft.Json.JsonProperty]
        private NoteCollection notes = new NoteCollection();
        [Newtonsoft.Json.JsonProperty]
        private EventCollection events = new EventCollection();

        /// <summary>
        /// チャンネルID
        /// </summary>
        public int SpeedCh
        {
            get { return speedCh; }
            set { speedCh = value; }
        }
        /// <summary>
        /// チャンネルの名前
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        /// <summary>
        /// 最初のスピード
        /// </summary>
        public decimal ForceSpeed
        {
            get { return forceNoteSpeed; }
            set { forceNoteSpeed = value; }
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

        public Channel()
        {
            SpeedCh = 0;
            Name = "stagename";
            ForceSpeed = 1;
            Notes = new NoteCollection();
            Events = new EventCollection();
        }
        public Channel(Channel channel)
        {
            SpeedCh = channel.SpeedCh;
            Name = channel.Name;
            ForceSpeed = channel.ForceSpeed;
            Notes = channel.Notes;
            Events = channel.Events;
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
