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
    public partial class ShortNotePropertiesWindow : Window
    {
        public ShortNotePropertiesWindow()
        {
            InitializeComponent();
        }

    }

    public class ShortNotePropertiesWindowViewModel : ViewModel
    {

        private TappableBase Note { get; }
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


        public ShortNotePropertiesWindowViewModel()
        {
        }

        public ShortNotePropertiesWindowViewModel(Tap note)
        {
            Note = note;
        }

        public ShortNotePropertiesWindowViewModel(TappableBase note)
        {
            Note = note;
        }

        public void BeginEdit()
        {
            NoteTick = Note.Tick;
            NoteChannel = Note.Channel;
            NoteLaneIndex = Note.LaneIndex;
            NoteWidth = Note.Width;
        }

        public void CommitEdit()
        {
            Note.Tick = NoteTick;
            Note.Channel = NoteChannel;
            Note.LaneIndex = NoteLaneIndex;
            Note.Width = NoteWidth;
        }
    }
}
