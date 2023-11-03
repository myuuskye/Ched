using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Ched.Configuration;
using Ched.Core;
using Ched.Core.Notes;
using Ched.Localization;
using Ched.UI.Operations;

namespace Ched.UI.Windows
{
    /// <summary>
    /// Interaction logic for NotePropertiesWindow.xaml
    /// </summary>
    public partial class SlideStepNotePropertiesWindow : Window
    {
        public SlideStepNotePropertiesWindow()
        {
            InitializeComponent();
        }

    }

    public class SlideStepNotePropertiesWindowViewModel : ViewModel
    {

        private Slide.StepTap Note { get; }
        private bool IsEnd { get; }
        private NoteView noteView { get; }

        private int noteTick;
        private float noteLaneIndex;
        private float noteWidth;
        private int noteChannel;
        private bool noteVisible;

        public int NoteTick
        {
            get => noteTick;
            set
            {
                if (value == noteTick) return;
                noteTick = value;
                NotifyPropertyChanged();
            }
        }

        public int NoteChannel
        {
            get => noteChannel;
            set
            {
                if (value == noteChannel) return;
                noteChannel = value;
                NotifyPropertyChanged();
            }
        }
        public float NoteLaneIndex
        {
            get => noteLaneIndex;
            set
            {
                if (value == noteLaneIndex) return;
                noteLaneIndex = value;
                NotifyPropertyChanged();
            }
        }
        public float NoteWidth
        {
            get => noteWidth;
            set
            {
                if (value == noteWidth) return;
                noteWidth = value;
                NotifyPropertyChanged();
            }
        }
        public bool NoteVisible
        {
            get => noteVisible;
            set
            {
                if (value == noteVisible) return;
                noteVisible = value;
                NotifyPropertyChanged();
            }
        }


        public SlideStepNotePropertiesWindowViewModel()
        {
        }

        public SlideStepNotePropertiesWindowViewModel(Slide.StepTap note, bool isend)
        {
            Note = note;
            IsEnd = isend;
        }

        public void BeginEdit()
        {
            NoteTick = Note.TickOffset;
            NoteChannel = Note.Channel;
            NoteLaneIndex = Note.LaneIndexOffset;
            NoteWidth = Note.WidthChange;
            if (IsEnd)
            NoteVisible = true;
            else
            NoteVisible = Note.IsVisible;

        }

        public void CommitEdit()
        {
            Note.TickOffset = NoteTick;
            Note.Channel = NoteChannel;
            Note.LaneIndexOffset = NoteLaneIndex;
            Note.WidthChange = NoteWidth;
            if (IsEnd)
                Note.IsVisible = true;
            else
                Note.IsVisible = NoteVisible;
        }
    }
}
