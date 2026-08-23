using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ched.Core.Events;

namespace Ched.Core
{
    /// <summary>
    /// イベントを格納するコレクションを表すクラスです。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class EventCollection
    {
        [Newtonsoft.Json.JsonProperty]
        private List<BpmChangeEvent> bpmChangeEvents = new List<BpmChangeEvent>();
        [Newtonsoft.Json.JsonProperty]
        private List<TimeSignatureChangeEvent> timeSignatureChangeEvents = new List<TimeSignatureChangeEvent>();
        [Newtonsoft.Json.JsonProperty]
        private List<HighSpeedChangeEvent> highSpeedChangeEvents = new List<HighSpeedChangeEvent>();
        [Newtonsoft.Json.JsonProperty]
        private List<CommentEvent> commentEvents = new List<CommentEvent>();
        [Newtonsoft.Json.JsonProperty]
        private List<SkillEvent> skillEvents = new List<SkillEvent>();
        [Newtonsoft.Json.JsonProperty]
        private List<FeverEvent> feverEvents = new List<FeverEvent>();
        [Newtonsoft.Json.JsonProperty]
        private List<CameraChangeEvent> cameraChangeEvents = new List<CameraChangeEvent>();
        [Newtonsoft.Json.JsonProperty]
        private List<StageMaskChangeEvent> stageMaskChangeEvents = new List<StageMaskChangeEvent>();
        [Newtonsoft.Json.JsonProperty]
        private List<StagePivotChangeEvent> stagePivotChangeEvents = new List<StagePivotChangeEvent>();
        [Newtonsoft.Json.JsonProperty]
        private List<StageStyleChangeEvent> stageStyleChangeEvents = new List<StageStyleChangeEvent>();
        [Newtonsoft.Json.JsonProperty]
        private List<StageTransformChangeEvent> stageTransformChangeEvents = new List<StageTransformChangeEvent>();

        public List<BpmChangeEvent> BpmChangeEvents
        {
            get { return bpmChangeEvents; }
            set { bpmChangeEvents = value; }
        }

        public List<TimeSignatureChangeEvent> TimeSignatureChangeEvents
        {
            get { return timeSignatureChangeEvents; }
            set { timeSignatureChangeEvents = value; }
        }

        public List<HighSpeedChangeEvent> HighSpeedChangeEvents
        {
            get { return highSpeedChangeEvents; }
            set { highSpeedChangeEvents = value; }
        }
        public List<CommentEvent> CommentEvents
        {
            get { return commentEvents; }
            set { commentEvents = value; }
        }
        public List<SkillEvent> SkillEvents
        {
            get { return skillEvents; }
            set { skillEvents = value; }
        }
        public List<FeverEvent> FeverEvents
        {
            get { return feverEvents; }
            set { feverEvents = value; }
        }
        public List<CameraChangeEvent> CameraChangeEvents
        {
            get { return cameraChangeEvents; }
            set { cameraChangeEvents = value; }
        }
        public List<StageMaskChangeEvent> StageMaskChangeEvents
        {
            get { return stageMaskChangeEvents; }
            set { stageMaskChangeEvents = value; }
        }
        public List<StagePivotChangeEvent> StagePivotChangeEvents
        {
            get { return stagePivotChangeEvents; }
            set { stagePivotChangeEvents = value; }
        }
        public List<StageStyleChangeEvent> StageStyleChangeEvents
        {
            get { return stageStyleChangeEvents; }
            set { stageStyleChangeEvents = value; }
        }
        public List<StageTransformChangeEvent> StageTransformChangeEvents
        {
            get { return stageTransformChangeEvents; }
            set { stageTransformChangeEvents = value; }
        }

        public IEnumerable<EventBase> AllEvents =>
            BpmChangeEvents.Cast<EventBase>()
            .Concat(TimeSignatureChangeEvents)
            .Concat(HighSpeedChangeEvents)
            .Concat(CommentEvents)
            .Concat(SkillEvents)
            .Concat(FeverEvents)
            .Concat(CameraChangeEvents)
            .Concat(StageMaskChangeEvents)
            .Concat(StagePivotChangeEvents)
            .Concat(StageStyleChangeEvents)
            .Concat(StageTransformChangeEvents)
            ;

        public void UpdateTicksPerBeat(double factor)
        {
            var events = BpmChangeEvents.Cast<EventBase>()
                 .Concat(TimeSignatureChangeEvents)
                 .Concat(HighSpeedChangeEvents)
                 .Concat(CommentEvents)
                 .Concat(skillEvents)
                 .Concat(feverEvents)
                 .Concat(CameraChangeEvents)
                 .Concat(StageMaskChangeEvents)
                .Concat(StagePivotChangeEvents)
                .Concat(StageStyleChangeEvents)
                .Concat(StageTransformChangeEvents);
            foreach (var e in events)
                e.Tick = (int)(e.Tick * factor);
        }
    }
}
