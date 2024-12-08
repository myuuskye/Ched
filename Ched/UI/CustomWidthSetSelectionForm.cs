using Ched.Core.Notes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Ched.UI
{
    public partial class CustomWidthSetSelectionForm : Form
    {
        List<IAirable> notes = new List<IAirable>();

        public float Width
        {
            get { return (float)widthSetBox.Value; }
            set
            {
                widthSetBox.Value = (decimal)value;

            }
        }
       
        public IAirable SelectNote
        {
            get {
                if (notesBox.SelectedIndex < 0) return null;
                else return notes[notesBox.SelectedIndex]; }
        }


        public CustomWidthSetSelectionForm(NoteView.NoteCollection Notes, int CurrentTick, int HeadTick, int TailTick)
        {
            InitializeComponent();
            
            AcceptButton = buttonOK;
            CancelButton = buttonCancel;
            buttonOK.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            
            widthSetBox.Minimum = 0;
            widthSetBox.Increment = 1;
            widthSetBox.DecimalPlaces = 1;
            widthSetBox.Value = 1;

           if(Notes  != null)
            {
                Console.WriteLine("notes is exist");
                List<IAirable> steps = new List<IAirable>();
                foreach (var slide in Notes.Slides)
                {
                    steps.AddRange(slide.StepNotes);
                }
                foreach (var guide in Notes.Guides)
                {
                    steps.AddRange(guide.StepNotes);
                }
                foreach (var note in Notes.Taps.Cast<IAirable>().Concat(Notes.ExTaps).Concat(Notes.Damages).Concat(Notes.Flicks).Concat(steps).Where(p => p.Tick >= HeadTick && p.Tick <= TailTick).OrderBy(q => Math.Abs( q.Tick - CurrentTick)))
                {
                    notesBox.Items.Add("Tick: " + note.Tick + " LaneIndex: " + note.LaneIndex + " Width: " + note.Width + " Channel: " + note.Channel);
                    notes.Add(note);
                }
            }
            

        }
    }
}
