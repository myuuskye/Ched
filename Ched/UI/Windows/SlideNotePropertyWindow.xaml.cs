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
    public partial class SlideNotePropertiesWindow : Window
    {
        public SlideNotePropertiesWindow()
        {
            InitializeComponent();
        }

    }

    public class SlideNotePropertiesWindowViewModel : ViewModel
    {

        private Slide Note { get; }
        private NoteView noteView { get; }

        private int noteTick;
        private float noteLaneIndex;
        private float noteWidth;
        private int noteChannel;

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


        public SlideNotePropertiesWindowViewModel()
        {
        }

        public SlideNotePropertiesWindowViewModel(Slide note)
        {
            Note = note;
        }

        public void BeginEdit()
        {
            NoteTick = Note.StartTick;
            NoteLaneIndex = Note.StartLaneIndex;
            NoteWidth = Note.StartWidth;
            NoteChannel = Note.StartNote.Channel;

        }

        public void CommitEdit()
        {
            Note.StartTick = NoteTick;
            Note.StartLaneIndex = NoteLaneIndex;
            Note.StartWidth = NoteWidth;
            Note.StartNote.Channel = NoteChannel;
        }
    }
}
