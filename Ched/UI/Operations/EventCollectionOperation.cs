using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Ched.Core.Events;
using static Ched.UI.Operations.ChangeBpmEventOperation;

namespace Ched.UI.Operations
{
    public abstract class EventCollectionOperation<T> : IOperation where T : EventBase
    {
        protected T Event { get; }
        protected List<T> Collection { get; }
        public abstract string Description { get; }

        public EventCollectionOperation(List<T> collection, T item)
        {
            Collection = collection;
            Event = item;
        }

        public abstract void Redo();
        public abstract void Undo();
    }

    public class InsertEventOperation<T> : EventCollectionOperation<T> where T : EventBase
    {
        public override string Description { get { return "イベントの挿入"; } }

        public InsertEventOperation(List<T> collection, T item) : base(collection, item)
        {
        }

        public override void Redo()
        {
            Collection.Add(Event);
        }

        public override void Undo()
        {
            Collection.Remove(Event);
        }
    }

    public class ChangeBpmEventOperation : EventBase, IOperation
    { 
        public string Description { get { return "Bpmイベントの変更"; } }

        protected BpmChangeEvent Event { get; }
        protected List<BpmChangeEvent> Collection { get; }

        protected EventDetail BeforeEvent { get; }
        protected EventDetail AfterEvent { get; }

        public ChangeBpmEventOperation(List<BpmChangeEvent> collection, BpmChangeEvent item, EventDetail before, EventDetail after) 
        {
            Collection = collection;
            Event = item;
            BeforeEvent = before;
            AfterEvent = after;
        }

        public void Redo()
        {
            Event.Tick = AfterEvent.Tick;
            Event.Bpm = AfterEvent.Bpm;
        }

        public void Undo()
        {
            Event.Tick = BeforeEvent.Tick;
            Event.Bpm = BeforeEvent.Bpm;
        }

        public struct EventDetail
        {
            public int Tick { get; }
            public double Bpm { get; }

            public EventDetail(int tick, double bpm)
            {
                Tick = tick;
                Bpm = bpm;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is EventDetail)) return false;
                EventDetail other = (EventDetail)obj;
                return Tick == other.Tick && Bpm == other.Bpm;
            }

            public override int GetHashCode()
            {
                return Tick ^ (int)Bpm;
            }

            public static bool operator ==(EventDetail a, EventDetail b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(EventDetail a, EventDetail b)
            {
                return !a.Equals(b);
            }
        }
    }

    public class ChangeHighSpeedEventOperation : EventBase, IOperation
    {
        public string Description { get { return "HighSpeedイベントの変更"; } }

        protected HighSpeedChangeEvent Event { get; }
        protected List<HighSpeedChangeEvent> Collection { get; }

        protected EventDetail BeforeEvent { get; }
        protected EventDetail AfterEvent { get; }

        public ChangeHighSpeedEventOperation(List<HighSpeedChangeEvent> collection, HighSpeedChangeEvent item, EventDetail before, EventDetail after)
        {
            Collection = collection;
            Event = item;
            BeforeEvent = before;
            AfterEvent = after;
        }

        public void Redo()
        {
            Event.Tick = AfterEvent.Tick;
            Event.SpeedRatio = AfterEvent.SpeedRatio;
            Event.SpeedCh = AfterEvent.SpeedCh;
            Event.CustomArgs = AfterEvent.CustomArgs;
            Event.Skip = AfterEvent.SkipBeats;
            Event.Ease = AfterEvent.Ease;
            Event.HideNotes = AfterEvent.HideNotes;
            Event.EditLaneIndex = AfterEvent.EditLaneIndex;
        }

        public void Undo()
        {
            Event.Tick = BeforeEvent.Tick;
            Event.SpeedRatio = BeforeEvent.SpeedRatio;
            Event.SpeedCh = BeforeEvent.SpeedCh;
            Event.CustomArgs = BeforeEvent.CustomArgs;
            Event.Skip = BeforeEvent.SkipBeats;
            Event.Ease = BeforeEvent.Ease;
            Event.HideNotes = BeforeEvent.HideNotes;
            Event.EditLaneIndex = BeforeEvent.EditLaneIndex;
        }

        public struct EventDetail
        {
            public int Tick { get; }
            public decimal SpeedRatio { get; }
            public int SpeedCh { get; }
            public string CustomArgs { get; }
            public float SkipBeats { get; }
            public int Ease { get; }
            public int HideNotes { get; }
            public float EditLaneIndex { get; }

            public EventDetail(int tick, decimal ratio, int ch, string customargs, float skip, int ease, int hide, float laneindex )
            {
                Tick = tick;
                SpeedRatio = ratio;
                SpeedCh = ch;
                CustomArgs = customargs;
                SkipBeats = skip;
                Ease = ease;
                HideNotes = hide;
                EditLaneIndex = laneindex;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is EventDetail)) return false;
                EventDetail other = (EventDetail)obj;
                return Tick == other.Tick && SpeedRatio == other.SpeedRatio && SpeedCh == other.SpeedCh 
                    && CustomArgs == other.CustomArgs && SkipBeats == other.SkipBeats && Ease == other.Ease
                    && HideNotes == other.HideNotes && EditLaneIndex == other.EditLaneIndex;
            }

            public override int GetHashCode()
            {
                return Tick ^ SpeedCh;
            }

            public static bool operator ==(EventDetail a, EventDetail b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(EventDetail a, EventDetail b)
            {
                return !a.Equals(b);
            }
        }
    }


    public class ChangeTimeSignatureEventOperation : EventBase, IOperation
    {
        public string Description { get { return "TimeSignatureイベントの変更"; } }

        protected TimeSignatureChangeEvent Event { get; }
        protected List<TimeSignatureChangeEvent> Collection { get; }

        protected EventDetail BeforeEvent { get; }
        protected EventDetail AfterEvent { get; }

        public ChangeTimeSignatureEventOperation(List<TimeSignatureChangeEvent> collection, TimeSignatureChangeEvent item, EventDetail before, EventDetail after)
        {
            Collection = collection;
            Event = item;
            BeforeEvent = before;
            AfterEvent = after;
        }

        public void Redo()
        {
            Event.Tick = AfterEvent.Tick;
            Event.Numerator = AfterEvent.Numrator;
            Event.DenominatorExponent = AfterEvent.DenominatorExponent;
        }

        public void Undo()
        {
            Event.Tick = BeforeEvent.Tick;
            Event.Numerator = BeforeEvent.Numrator;
            Event.DenominatorExponent = BeforeEvent.DenominatorExponent;
        }

        public struct EventDetail
        {
            public int Tick { get; }
            public int Numrator { get; }
            public int DenominatorExponent { get; }

            public EventDetail(int tick, int numrator, int denominatorex)
            {
                Tick = tick;
                Numrator = numrator;
                DenominatorExponent = denominatorex;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is EventDetail)) return false;
                EventDetail other = (EventDetail)obj;
                return Tick == other.Tick && Numrator == other.Numrator && DenominatorExponent == other.DenominatorExponent;
            }

            public override int GetHashCode()
            {
                return Tick ^ (int)Numrator ^ DenominatorExponent;
            }

            public static bool operator ==(EventDetail a, EventDetail b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(EventDetail a, EventDetail b)
            {
                return !a.Equals(b);
            }
        }
    }

    public class ChangeCommentEventOperation : EventBase, IOperation
    {
        public string Description { get { return "Commentイベントの変更"; } }

        protected CommentEvent Event { get; }
        protected List<CommentEvent> Collection { get; }

        protected EventDetail BeforeEvent { get; }
        protected EventDetail AfterEvent { get; }

        public ChangeCommentEventOperation(List<CommentEvent> collection, CommentEvent item, EventDetail before, EventDetail after)
        {
            Collection = collection;
            Event = item;
            BeforeEvent = before;
            AfterEvent = after;
        }

        public void Redo()
        {
            Event.Tick = AfterEvent.Tick;
            Event.Comment = AfterEvent.Comment;
            Event.Color = AfterEvent.Color;
            Event.Size = AfterEvent.Size;
            Event.LaneIndex = AfterEvent.LaneIndex;

        }

        public void Undo()
        {
            Event.Tick = BeforeEvent.Tick;
            Event.Comment = BeforeEvent.Comment;
            Event.Color = BeforeEvent.Color;
            Event.Size = BeforeEvent.Size;
            Event.LaneIndex = BeforeEvent.LaneIndex;
        }

        public struct EventDetail
        {
            public int Tick { get; }
            public string Comment { get; }
            public int Color { get; }
            public float Size { get; }
            public float LaneIndex { get; }

            public EventDetail(int tick, string comment, int color, float size, float laneIndex)
            {
                Tick = tick;
                Comment = comment;
                Color = color;
                Size = size;
                LaneIndex = laneIndex;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is EventDetail)) return false;
                EventDetail other = (EventDetail)obj;
                return Tick == other.Tick && Comment == other.Comment && Color == other.Color && Size == other.Size && LaneIndex == other.LaneIndex;
            }

            public override int GetHashCode()
            {
                return Tick ^ (int)Color;
            }

            public static bool operator ==(EventDetail a, EventDetail b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(EventDetail a, EventDetail b)
            {
                return !a.Equals(b);
            }
        }
    }
    public class ChangeCameraEventOperation : EventBase, IOperation
    {
        public string Description { get { return "Cameraイベントの変更"; } }

        protected CameraChangeEvent Event { get; }
        protected List<CameraChangeEvent> Collection { get; }

        protected EventDetail BeforeEvent { get; }
        protected EventDetail AfterEvent { get; }

        public ChangeCameraEventOperation(List<CameraChangeEvent> collection, CameraChangeEvent item, EventDetail before, EventDetail after)
        {
            Collection = collection;
            Event = item;
            BeforeEvent = before;
            AfterEvent = after;
        }

        public void Redo()
        {
            Event.Tick = AfterEvent.Tick;
            Event.LaneIndex = AfterEvent.LaneIndex;
            Event.Width = AfterEvent.Width;
            Event.Zoom = AfterEvent.Zoom;
            Event.ZoomTargetLane = AfterEvent.ZoomTargetLane;
            Event.ZoomTargetY = AfterEvent.ZoomTargetY;
            Event.ZoomVerticalAlign = AfterEvent.ZoomVerticalAlign;
            Event.Rotation = AfterEvent.Rotation;
            Event.Tilt = AfterEvent.StageTilt;
            Event.Ease = AfterEvent.Ease;


        }

        public void Undo()
        {
            Event.Tick = BeforeEvent.Tick;
            Event.LaneIndex = BeforeEvent.LaneIndex;
            Event.Width = BeforeEvent.Width;
            Event.Zoom = BeforeEvent.Zoom;
            Event.ZoomTargetLane = BeforeEvent.ZoomTargetLane;
            Event.ZoomTargetY = BeforeEvent.ZoomTargetY;
            Event.ZoomVerticalAlign = BeforeEvent.ZoomVerticalAlign;
            Event.Rotation = BeforeEvent.Rotation;
            Event.Tilt = BeforeEvent.StageTilt;
            Event.Ease = BeforeEvent.Ease;
        }

        public struct EventDetail
        {
            public int Tick { get; }
            public float LaneIndex { get; }
            public float Width { get; }
            public float Zoom { get; }
            public float ZoomTargetLane { get; }
            public float ZoomTargetY { get; }
            public int ZoomVerticalAlign { get; }
            public float Rotation { get; }
            public float StageTilt { get; }
            public int Ease { get; }


            public EventDetail(int tick, float laneIndex, float width, float zoom, float zoomLane, float zoomY, int zoomAlign, float rotation, float tilt, int ease )
            {
                Tick = tick;
                LaneIndex = laneIndex;
                Width = width;
                Zoom = zoom;
                ZoomTargetLane = zoomLane;
                ZoomTargetY = zoomY;
                ZoomVerticalAlign = zoomAlign;
                Rotation = rotation;
                StageTilt = tilt;
                Ease = ease;
            }
            public EventDetail(CameraChangeEvent @event)
            {
                Tick = @event.Tick;
                LaneIndex = @event.LaneIndex;
                Width = @event.Width;
                Zoom = @event.Zoom;
                ZoomTargetLane = @event.ZoomTargetLane;
                ZoomTargetY = @event.ZoomTargetY;
                ZoomVerticalAlign = @event.ZoomVerticalAlign;
                Rotation = @event.Rotation;
                StageTilt = @event.Tilt;
                Ease = @event.Ease;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is EventDetail)) return false;
                EventDetail other = (EventDetail)obj;
                return Tick == other.Tick && LaneIndex == other.LaneIndex && Width == other.Width && Zoom == other.Zoom &&
                    ZoomTargetLane == other.ZoomTargetLane && ZoomTargetY == other.ZoomTargetY && ZoomVerticalAlign == other.ZoomVerticalAlign &&
                    Rotation == other.Rotation && StageTilt == other.StageTilt && Ease == other.Ease;
            }

            public override int GetHashCode()
            {
                return Tick ^ (int)LaneIndex;
            }

            public static bool operator ==(EventDetail a, EventDetail b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(EventDetail a, EventDetail b)
            {
                return !a.Equals(b);
            }
        }
    }

    public class ChangeStageMaskEventOperation : EventBase, IOperation
    {
        public string Description { get { return "StageMaskイベントの変更"; } }

        protected StageMaskChangeEvent Event { get; }
        protected List<StageMaskChangeEvent> Collection { get; }

        protected EventDetail BeforeEvent { get; }
        protected EventDetail AfterEvent { get; }

        public ChangeStageMaskEventOperation(List<StageMaskChangeEvent> collection, StageMaskChangeEvent item, EventDetail before, EventDetail after)
        {
            Collection = collection;
            Event = item;
            BeforeEvent = before;
            AfterEvent = after;
        }

        public void Redo()
        {
            Event.Tick = AfterEvent.Tick;
            Event.Stage = AfterEvent.Stage;
            Event.LaneIndex = AfterEvent.LaneIndex;
            Event.Width = AfterEvent.Width;
            Event.Ease = AfterEvent.Ease;


        }

        public void Undo()
        {
            Event.Tick = BeforeEvent.Tick;
            Event.LaneIndex = BeforeEvent.LaneIndex;
            Event.Width = BeforeEvent.Width;
            Event.Stage = BeforeEvent.Stage;
            Event.Ease = BeforeEvent.Ease;
        }

        public struct EventDetail
        {
            public int Tick { get; }
            public int Stage { get; }
            public float LaneIndex { get; }
            public float Width { get; }
            public int Ease { get; }


            public EventDetail(int tick, float laneIndex, float width, int stage, int ease)
            {
                Tick = tick;
                LaneIndex = laneIndex;
                Width = width;
                Stage = stage;
                Ease = ease;
            }
            public EventDetail(StageMaskChangeEvent @event)
            {
                Tick = @event.Tick;
                LaneIndex = @event.LaneIndex;
                Width = @event.Width;
                Stage= @event.Stage;
                Ease = @event.Ease;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is EventDetail)) return false;
                EventDetail other = (EventDetail)obj;
                return Tick == other.Tick && LaneIndex == other.LaneIndex && Width == other.Width  &&  Stage == other.Stage && Ease == other.Ease;
            }

            public override int GetHashCode()
            {
                return Tick ^ (int)LaneIndex;
            }

            public static bool operator ==(EventDetail a, EventDetail b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(EventDetail a, EventDetail b)
            {
                return !a.Equals(b);
            }
        }
    }
    public class ChangeStagePivotEventOperation : EventBase, IOperation
    {
        public string Description { get { return "StagePivotイベントの変更"; } }

        protected StagePivotChangeEvent Event { get; }
        protected List<StagePivotChangeEvent> Collection { get; }

        protected EventDetail BeforeEvent { get; }
        protected EventDetail AfterEvent { get; }

        public ChangeStagePivotEventOperation(List<StagePivotChangeEvent> collection, StagePivotChangeEvent item, EventDetail before, EventDetail after)
        {
            Collection = collection;
            Event = item;
            BeforeEvent = before;
            AfterEvent = after;
        }

        public void Redo()
        {
            Event.Tick = AfterEvent.Tick;
            Event.Stage = AfterEvent.Stage;
            Event.LaneIndex = AfterEvent.LaneIndex;
            Event.DivisionSize = AfterEvent.DivisionSize;
            Event.DivisionParity = AfterEvent.DivisionParity;
            Event.YOffset = AfterEvent.YOffset;
            Event.YBeatOffset = AfterEvent.YBeatOffset;
            Event.Ease = AfterEvent.Ease;


        }

        public void Undo()
        {
            Event.Tick = BeforeEvent.Tick;
            Event.LaneIndex = BeforeEvent.LaneIndex;
            Event.Stage = BeforeEvent.Stage;
            Event.DivisionSize = BeforeEvent.DivisionSize;
            Event.DivisionParity = BeforeEvent.DivisionParity;
            Event.YOffset = BeforeEvent.YOffset;
            Event.YBeatOffset = BeforeEvent.YBeatOffset;
            Event.Ease = BeforeEvent.Ease;
        }

        public struct EventDetail
        {
            public int Tick { get; }
            public int Stage { get; }
            public float LaneIndex { get; }
            public int DivisionSize { get; }
            public int DivisionParity { get; }
            public float YOffset { get; }
            public float YBeatOffset { get; }
            public int Ease { get; }


            public EventDetail(int tick, float laneIndex, int stage, int divsize, int divparity, float yoffset, float yboffset, int ease)
            {
                Tick = tick;
                LaneIndex = laneIndex;
                Stage = stage;
                DivisionSize = divsize;
                DivisionParity = divparity;
                YOffset = yoffset;
                YBeatOffset = yboffset;
                Ease = ease;
            }
            public EventDetail(StagePivotChangeEvent @event)
            {
                Tick = @event.Tick;
                LaneIndex = @event.LaneIndex;
                Stage = @event.Stage;
                DivisionSize = @event.DivisionSize;
                DivisionParity = @event.DivisionParity;
                YOffset = @event.YOffset;
                YBeatOffset = @event.YBeatOffset;
                Ease = @event.Ease;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is EventDetail)) return false;
                EventDetail other = (EventDetail)obj;
                return Tick == other.Tick && LaneIndex == other.LaneIndex && Stage == other.Stage && DivisionSize == other.DivisionSize && DivisionParity == other.DivisionParity
                    && YOffset == other.YOffset && YBeatOffset == other.YBeatOffset && Ease == other.Ease;
            }

            public override int GetHashCode()
            {
                return Tick ^ (int)LaneIndex;
            }

            public static bool operator ==(EventDetail a, EventDetail b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(EventDetail a, EventDetail b)
            {
                return !a.Equals(b);
            }
        }
    }
    public class ChangeStageStyleEventOperation : EventBase, IOperation
    {
        public string Description { get { return "StageStyleイベントの変更"; } }

        protected StageStyleChangeEvent Event { get; }
        protected List<StageStyleChangeEvent> Collection { get; }

        protected EventDetail BeforeEvent { get; }
        protected EventDetail AfterEvent { get; }

        public ChangeStageStyleEventOperation(List<StageStyleChangeEvent> collection, StageStyleChangeEvent item, EventDetail before, EventDetail after)
        {
            Collection = collection;
            Event = item;
            BeforeEvent = before;
            AfterEvent = after;
        }

        public void Redo()
        {
            Event.Tick = AfterEvent.Tick;
            Event.Stage = AfterEvent.Stage;
            Event.LaneIndex = AfterEvent.LaneIndex;
            Event.JudgeLineColor = AfterEvent.JudgeLineColor;
            Event.JudgeLineStyle = AfterEvent.JudgeLineStyle;
            Event.LeftBorderStyle = AfterEvent.LeftBorderStyle;
            Event.RightBorderStyle = AfterEvent.RightBorderStyle;
            Event.FullWidth = AfterEvent.FullWidth;
            Event.NoteAlpha = AfterEvent.NoteAlpha;
            Event.LaneAlpha = AfterEvent.LaneAlpha;
            Event.JudgeLineAlpha = AfterEvent.JudgeLineAlpha;
            Event.DivisonLineAlpha = AfterEvent.DivisionLineAlpha;
            Event.Ease = AfterEvent.Ease;


        }

        public void Undo()
        {
            Event.Tick = BeforeEvent.Tick;
            Event.LaneIndex = BeforeEvent.LaneIndex;
            Event.Stage = BeforeEvent.Stage;
            Event.JudgeLineColor = BeforeEvent.JudgeLineColor;
            Event.JudgeLineStyle = BeforeEvent.JudgeLineStyle;
            Event.LeftBorderStyle = BeforeEvent.LeftBorderStyle;
            Event.RightBorderStyle = BeforeEvent.RightBorderStyle;
            Event.FullWidth = BeforeEvent.FullWidth;
            Event.NoteAlpha = BeforeEvent.NoteAlpha;
            Event.LaneAlpha = BeforeEvent.LaneAlpha;
            Event.JudgeLineAlpha = BeforeEvent.JudgeLineAlpha;
            Event.DivisonLineAlpha = BeforeEvent.DivisionLineAlpha;
            Event.Ease = BeforeEvent.Ease;
        }

        public struct EventDetail
        {
            public int Tick { get; }
            public int Stage { get; }
            public float LaneIndex { get; }
            public int JudgeLineColor { get; }
            public int JudgeLineStyle { get; }
            public int LeftBorderStyle { get; }
            public int RightBorderStyle { get; }
            public int FullWidth { get; }
            public float NoteAlpha { get; }
            public float LaneAlpha { get; }
            public float JudgeLineAlpha { get; }
            public float DivisionLineAlpha { get; }
            public int Ease { get; }


            public EventDetail(int tick, float laneIndex, int stage, int judgeLineC, int judgeLineS, int left, int right, int fullWidth, float noteAlpha, float laneAlpha, float judgeAlpha, float divAlpha, int ease)
            {
                Tick = tick;
                LaneIndex = laneIndex;
                Stage = stage;
                JudgeLineColor = judgeLineC;
                JudgeLineStyle = judgeLineS;
                LeftBorderStyle = left;
                RightBorderStyle = right;
                FullWidth = fullWidth;
                NoteAlpha = noteAlpha;
                LaneAlpha = laneAlpha;
                JudgeLineAlpha = judgeAlpha;
                DivisionLineAlpha = divAlpha;
                Ease = ease;
            }
            public EventDetail(StageStyleChangeEvent @event)
            {
                Tick = @event.Tick;
                LaneIndex = @event.LaneIndex;
                Stage = @event.Stage;
                JudgeLineColor = @event.JudgeLineColor;
                JudgeLineStyle = @event.JudgeLineStyle;
                LeftBorderStyle = @event.LeftBorderStyle;
                RightBorderStyle = @event.RightBorderStyle;
                FullWidth = @event.FullWidth;
                NoteAlpha = @event.NoteAlpha;
                LaneAlpha = @event.LaneAlpha;
                JudgeLineAlpha = @event.JudgeLineAlpha;
                DivisionLineAlpha = @event.DivisonLineAlpha;
                Ease = @event.Ease;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is EventDetail)) return false;
                EventDetail other = (EventDetail)obj;
                return Tick == other.Tick && LaneIndex == other.LaneIndex && Stage == other.Stage && JudgeLineColor == other.JudgeLineColor && JudgeLineStyle == other.JudgeLineStyle
                    && LeftBorderStyle == other.LeftBorderStyle && RightBorderStyle == other.RightBorderStyle
                    && NoteAlpha == other.NoteAlpha && LaneAlpha == other.LaneAlpha && JudgeLineAlpha == other.JudgeLineAlpha && DivisionLineAlpha == other.DivisionLineAlpha
                    && Ease == other.Ease;
            }

            public override int GetHashCode()
            {
                return Tick ^ (int)LaneIndex;
            }

            public static bool operator ==(EventDetail a, EventDetail b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(EventDetail a, EventDetail b)
            {
                return !a.Equals(b);
            }
        }
    }
    public class ChangeStageTransformEventOperation : EventBase, IOperation
    {
        public string Description { get { return "StageTransformイベントの変更"; } }

        protected StageTransformChangeEvent Event { get; }
        protected List<StageTransformChangeEvent> Collection { get; }

        protected EventDetail BeforeEvent { get; }
        protected EventDetail AfterEvent { get; }

        public ChangeStageTransformEventOperation(List<StageTransformChangeEvent> collection, StageTransformChangeEvent item, EventDetail before, EventDetail after)
        {
            Collection = collection;
            Event = item;
            BeforeEvent = before;
            AfterEvent = after;
        }

        public void Redo()
        {
            Event.Tick = AfterEvent.Tick;
            Event.Stage = AfterEvent.Stage;
            Event.XTranslation = AfterEvent.Xtrans;
            Event.YTranslation = AfterEvent.YXtrans;
            Event.Rotation = AfterEvent.Rotation;
            Event.Anchor = AfterEvent.Anchor;
            Event.Ease = AfterEvent.Ease;


        }

        public void Undo()
        {
            Event.Tick = BeforeEvent.Tick;
            Event.Stage = BeforeEvent.Stage;
            Event.XTranslation = BeforeEvent.Xtrans;
            Event.YTranslation = BeforeEvent.YXtrans;
            Event.Rotation = BeforeEvent.Rotation;
            Event.Anchor = BeforeEvent.Anchor;
            Event.Ease = BeforeEvent.Ease;
        }

        public struct EventDetail
        {
            public int Tick { get; }
            public int Stage { get; }
            public float Xtrans { get; }
            public float YXtrans { get; }
            public float Rotation { get; }
            public int Anchor { get; }
            public int Ease { get; }


            public EventDetail(int tick, int stage, float x, float y, float rotation,  int anchor, int ease)
            {
                Tick = tick;
                Stage = stage;
                Xtrans = x;
                YXtrans = y;
                Rotation = rotation;
                Anchor = anchor;
                Ease = ease;
            }
            public EventDetail(StageTransformChangeEvent @event)
            {
                Tick = @event.Tick;
                Stage = @event.Stage;
                Xtrans = @event.XTranslation;
                YXtrans = @event.YTranslation;
                Rotation = @event.Rotation;
                Anchor = @event.Anchor;
                Ease = @event.Ease;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is EventDetail)) return false;
                EventDetail other = (EventDetail)obj;
                return Tick == other.Tick  && Stage == other.Stage && Xtrans == other.Xtrans && YXtrans == other.YXtrans && Rotation == other.Rotation
                    && Anchor == other.Anchor && Ease == other.Ease;
            }

            public override int GetHashCode()
            {
                return Tick ^ (int)Xtrans;
            }

            public static bool operator ==(EventDetail a, EventDetail b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(EventDetail a, EventDetail b)
            {
                return !a.Equals(b);
            }
        }
    }



    public class RemoveEventOperation<T> : EventCollectionOperation<T> where T : EventBase
    {
        public override string Description { get { return "イベントの削除"; } }

        public RemoveEventOperation(List<T> collection, T item) : base(collection, item)
        {
        }

        public override void Redo()
        {
            Collection.Remove(Event);
        }

        public override void Undo()
        {
            Collection.Add(Event);
        }
    }
}
