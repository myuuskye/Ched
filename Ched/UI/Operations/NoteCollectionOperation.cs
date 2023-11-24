using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ched.Core.Notes;

namespace Ched.UI.Operations
{
    public abstract class NoteCollectionOperation<T> : IOperation
    {
        protected T Note { get; }
        protected NoteView.NoteCollection Collection { get; }
        public abstract string Description { get; }

        public NoteCollectionOperation(NoteView.NoteCollection collection, T note)
        {
            Collection = collection;
            Note = note;
        }

        public abstract void Undo();
        public abstract void Redo();
    }

    public class InsertTapOperation : NoteCollectionOperation<Tap>
    {
        public override string Description { get { return "TAPの追加"; } }

        public InsertTapOperation(NoteView.NoteCollection collection, Tap note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Add(Note);
        }

        public override void Undo()
        {
            Collection.Remove(Note);
        }
    }

    public class RemoveTapOperation : NoteCollectionOperation<Tap>
    {
        public override string Description { get { return "TAPの削除"; } }

        public RemoveTapOperation(NoteView.NoteCollection collection, Tap note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Remove(Note);
        }

        public override void Undo()
        {
            Collection.Add(Note);
        }
    }

    public class InsertExTapOperation : NoteCollectionOperation<ExTap>
    {
        public override string Description { get { return "ExTAPの追加"; } }

        public InsertExTapOperation(NoteView.NoteCollection collection, ExTap note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Add(Note);
        }

        public override void Undo()
        {
            Collection.Remove(Note);
        }
    }

    public class RemoveExTapOperation : NoteCollectionOperation<ExTap>
    {
        public override string Description { get { return "ExTAPの削除"; } }

        public RemoveExTapOperation(NoteView.NoteCollection collection, ExTap note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Remove(Note);
        }

        public override void Undo()
        {
            Collection.Add(Note);
        }
    }


    public class ChangeTapOperation : NoteCollectionOperation<TappableBase>
    {
        public override string Description { get { return "TAPをExTapと入れ替え"; } }
        protected ExTap AfterNote { get; }

        public ChangeTapOperation(NoteView.NoteCollection collection, Tap note, ExTap afternote) : base(collection, note)
        {
            AfterNote = afternote;
        }
        

        public override void Redo()
        {
            Collection.Remove((Tap)Note);
            Collection.Add(AfterNote);
        }

        public override void Undo()
        {
            Collection.Add((Tap)Note);
            Collection.Remove(AfterNote);
        }
    }
    public class ChangeExTapOperation : NoteCollectionOperation<TappableBase>
    {
        public override string Description { get { return "ExTAPをTapと入れ替え"; } }
        protected Tap AfterNote { get; }

        public ChangeExTapOperation(NoteView.NoteCollection collection, ExTap note, Tap afternote) : base(collection, note)
        {
            AfterNote = afternote;
        }


        public override void Redo()
        {
            Collection.Remove((ExTap)Note);
            Collection.Add(AfterNote);
        }

        public override void Undo()
        {
            Collection.Add((ExTap)Note);
            Collection.Remove(AfterNote);
        }
    }

    public class InsertHoldOperation : NoteCollectionOperation<Hold>
    {
        public override string Description { get { return "HOLDの追加"; } }

        public InsertHoldOperation(NoteView.NoteCollection collection, Hold note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Add(Note);
        }

        public override void Undo()
        {
            Collection.Remove(Note);
        }
    }

    public class RemoveHoldOperation : NoteCollectionOperation<Hold>
    {
        public override string Description { get { return "HOLDの削除"; } }

        public RemoveHoldOperation(NoteView.NoteCollection collection, Hold note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Remove(Note);
        }

        public override void Undo()
        {
            Collection.Add(Note);
        }
    }

    public class InsertSlideOperation : NoteCollectionOperation<Slide>
    {
        public override string Description { get { return "SLIDEの追加"; } }

        public InsertSlideOperation(NoteView.NoteCollection collection, Slide note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Add(Note);
        }

        public override void Undo()
        {
            Collection.Remove(Note);
        }
    }

    public class RemoveSlideOperation : NoteCollectionOperation<Slide>
    {
        public override string Description { get { return "SLIDEの削除"; } }

        public RemoveSlideOperation(NoteView.NoteCollection collection, Slide note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Remove(Note);
        }

        public override void Undo()
        {
            Collection.Add(Note);
        }
    }

    public class InsertFlickOperation : NoteCollectionOperation<Flick>
    {
        public override string Description { get { return "FLICKの追加"; } }

        public InsertFlickOperation(NoteView.NoteCollection collection, Flick note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Add(Note);
        }

        public override void Undo()
        {
            Collection.Remove(Note);
        }
    }

    public class RemoveFlickOperation : NoteCollectionOperation<Flick>
    {
        public override string Description { get { return "FLICKの削除"; } }

        public RemoveFlickOperation(NoteView.NoteCollection collection, Flick note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Remove(Note);
        }

        public override void Undo()
        {
            Collection.Add(Note);
        }
    }

    public class InsertAirOperation : NoteCollectionOperation<Air>
    {
        public override string Description { get { return "AIRの追加"; } }

        public InsertAirOperation(NoteView.NoteCollection collection, Air note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Add(Note);
        }

        public override void Undo()
        {
            Collection.Remove(Note);
        }
    }

    public class RemoveAirOperation : NoteCollectionOperation<Air>
    {
        public override string Description { get { return "AIRの削除"; } }

        public RemoveAirOperation(NoteView.NoteCollection collection, Air note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Remove(Note);
        }

        public override void Undo()
        {
            Collection.Add(Note);
        }
    }
    public class PaintAirOperation : NoteCollectionOperation<Air>
    {
        public override string Description { get { return "AIRの変更"; } }

        protected HorizontalAirDirection BeforeHDirection { get; }
        protected HorizontalAirDirection AfterHDirection { get; }

        protected VerticalAirDirection BeforeVDirection { get; }
        protected VerticalAirDirection AfterVDirection { get; }

        public PaintAirOperation(NoteView.NoteCollection collection, Air note, HorizontalAirDirection hbefore, HorizontalAirDirection hafter, VerticalAirDirection vbefore, VerticalAirDirection vafter) : base(collection, note)
        {
            BeforeHDirection = hbefore;
            AfterHDirection = hafter;
            BeforeVDirection = vbefore;
            AfterVDirection = vafter;
        }

        public override void Redo()
        {
            Collection.Paint(Note, AfterHDirection, AfterVDirection);
        }

        public override void Undo()
        {
            Collection.Paint(Note, BeforeHDirection, BeforeVDirection);
        }
    }

    public class InsertAirActionOperation : NoteCollectionOperation<AirAction>
    {
        public override string Description { get { return "AIR-ACTIONの追加"; } }

        public InsertAirActionOperation(NoteView.NoteCollection collection, AirAction note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Add(Note);
        }

        public override void Undo()
        {
            Collection.Remove(Note);
        }
    }

    public class RemoveAirActionOperation : NoteCollectionOperation<AirAction>
    {
        public override string Description { get { return "AIR-ACTIONの削除"; } }

        public RemoveAirActionOperation(NoteView.NoteCollection collection, AirAction note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Remove(Note);
        }

        public override void Undo()
        {
            Collection.Add(Note);
        }
    }


    public class InsertDamageOperation : NoteCollectionOperation<Damage>
    {
        public override string Description { get { return "ダメージノーツの追加"; } }

        public InsertDamageOperation(NoteView.NoteCollection collection, Damage note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Add(Note);
        }

        public override void Undo()
        {
            Collection.Remove(Note);
        }
    }

    public class RemoveDamageOperation : NoteCollectionOperation<Damage>
    {
        public override string Description { get { return "ダメージノーツの削除"; } }

        public RemoveDamageOperation(NoteView.NoteCollection collection, Damage note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Remove(Note);
        }

        public override void Undo()
        {
            Collection.Add(Note);
        }
    }

    public class InsertGuideOperation : NoteCollectionOperation<Guide>
    {
        public override string Description { get { return "GUIDEの追加"; } }

        public InsertGuideOperation(NoteView.NoteCollection collection, Guide note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Add(Note);
        }

        public override void Undo()
        {
            Collection.Remove(Note);
        }
    }

    public class RemoveGuideOperation : NoteCollectionOperation<Guide>
    {
        public override string Description { get { return "GUIDEの削除"; } }

        public RemoveGuideOperation(NoteView.NoteCollection collection, Guide note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Remove(Note);
        }

        public override void Undo()
        {
            Collection.Add(Note);
        }
    }
    public class PaintGuideOperation : NoteCollectionOperation<Guide>
    {
        public override string Description { get { return "GUIDEの変更"; } }

        Guide.USCGuideColor BeforeColor { get; }
        Guide.USCGuideColor AfterColor { get; }

        public PaintGuideOperation(NoteView.NoteCollection collection, Guide note, Guide.USCGuideColor beforecolor, Guide.USCGuideColor aftercolor) : base(collection, note)
        {
            BeforeColor = beforecolor;
            AfterColor = aftercolor;
        }

        public override void Redo()
        {
            Collection.Paint(Note, AfterColor);
        }

        public override void Undo()
        {
            Collection.Paint(Note, BeforeColor);
        }
    }


    public class InsertStepNoteTapOperation : NoteCollectionOperation<StepNoteTap>
    {
        public override string Description { get { return "StepNoteTAPの追加"; } }

        public InsertStepNoteTapOperation(NoteView.NoteCollection collection, StepNoteTap note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Add(Note);
        }

        public override void Undo()
        {
            Collection.Remove(Note);
        }
    }

    public class RemoveStepNoteTapOperation : NoteCollectionOperation<StepNoteTap>
    {
        public override string Description { get { return "StepNoteTAPの削除"; } }

        public RemoveStepNoteTapOperation(NoteView.NoteCollection collection, StepNoteTap note) : base(collection, note)
        {
        }

        public override void Redo()
        {
            Collection.Remove(Note);
        }

        public override void Undo()
        {
            Collection.Add(Note);
        }
    }



}
