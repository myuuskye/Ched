using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static Ched.Core.Notes.Guide;

namespace Ched.UI.Windows
{
    /// <summary>
    /// Interaction logic for NotePropertiesWindow.xaml
    /// </summary>
    public partial class GuideNotePropertiesWindow : Window
    {
        public GuideNotePropertiesWindow()
        {
            InitializeComponent();
        }

    }

    public class GuideNotePropertiesWindowViewModel : ViewModel
    {

        private Guide Note { get; }
        private NoteView noteView { get; }

        private int noteTick;
        private float noteLaneIndex;
        private float noteWidth;
        private int noteChannel;
        private int noteColor;
        private int lanedecimalPlaces;
        private int widthdecimalPlaces;

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
        public int NoteColor
        {
            get => noteColor;
            set
            {
                if (value == noteColor) return;
                noteColor = value;
                NotifyPropertyChanged();
            }
        }
        public int LaneIndexDecimalPlaces
        {
            get => lanedecimalPlaces;
            set
            {
                if (value == lanedecimalPlaces) return;
                lanedecimalPlaces = value;
                NotifyPropertyChanged();
            }
        }
        public int WidthDecimalPlaces
        {
            get => widthdecimalPlaces;
            set
            {
                if (value == widthdecimalPlaces) return;
                widthdecimalPlaces = value;
                NotifyPropertyChanged();
            }
        }


        public GuideNotePropertiesWindowViewModel()
        {
        }

        public GuideNotePropertiesWindowViewModel(Guide note)
        {
            Note = note;
        }


        public void BeginEdit()
        {
            NoteTick = Note.StartTick;
            NoteChannel = Note.Channel;
            NoteLaneIndex = Note.StartLaneIndex;
            NoteWidth = Note.StartWidth;
            NoteColor = (int)Note.GuideColor;
            LaneIndexDecimalPlaces = Note.StartLaneIndex.ToString().Length;
            WidthDecimalPlaces = Note.StartWidth.ToString().Length;
        }

        public void CommitEdit()
        {
            Note.StartTick = NoteTick;
            Note.Channel = NoteChannel;
            Note.StartLaneIndex = NoteLaneIndex;
            Note.StartWidth = NoteWidth;
            Note.GuideColor = (USCGuideColor)NoteColor;
        }
    }
}
