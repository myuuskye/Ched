using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        public void Undo()
        {
            Event.Tick = BeforeEvent.Tick;
            Event.SpeedRatio = BeforeEvent.SpeedRatio;
            Event.SpeedCh = BeforeEvent.SpeedCh;
        }

        public struct EventDetail
        {
            public int Tick { get; }
            public decimal SpeedRatio { get; }
            public int SpeedCh { get; }

            public EventDetail(int tick, decimal ratio, int ch)
            {
                Tick = tick;
                SpeedRatio = ratio;
                SpeedCh = ch;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is EventDetail)) return false;
                EventDetail other = (EventDetail)obj;
                return Tick == other.Tick && SpeedRatio == other.SpeedRatio && SpeedCh == other.SpeedCh;
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
