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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            var data = ((SlideNotePropertiesWindowViewModel)DataContext);
            StageComboBox.ItemsSource = data.Stages;
            StageComboBox.SelectedValue = data.Note.Stage;
        }
        private void StageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((SlideNotePropertiesWindowViewModel)DataContext).NoteStage = (int)StageComboBox.SelectedValue;
        }
    }

    public class SlideNotePropertiesWindowViewModel : ViewModel
    {

        public Slide Note { get; }
        public List<Stage> Stages { get; }

        private int noteTick;
        private float noteLaneIndex;
        private float noteWidth;
        private int noteChannel;
        private int noteStage;
        private int lanedecimalPlaces;
        private int widthdecimalPlaces;
        private float noteOpacity;

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
        public int NoteStage
        {
            get => noteStage;
            set
            {
                if (value == noteStage) return;
                noteStage = value;
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
        public float NoteOpacity
        {
            get => noteOpacity;
            set
            {
                if (value == noteOpacity) return;
                noteOpacity = value;
                NotifyPropertyChanged();
            }
        }

        public SlideNotePropertiesWindowViewModel()
        {
        }

        public SlideNotePropertiesWindowViewModel(Slide note, List<Stage> stages)
        {
            Note = note;
            Stages = stages;
        }

        public void BeginEdit()
        {
            NoteTick = Note.StartTick;
            NoteLaneIndex = Note.StartLaneIndex;
            NoteWidth = Note.StartWidth;
            NoteChannel = Note.StartNote.Channel;
            NoteStage = Note.Stage;
            LaneIndexDecimalPlaces = Note.StartLaneIndex.ToString().Length;
            WidthDecimalPlaces = Note.StartWidth.ToString().Length;
        }

        public void CommitEdit()
        {
            Note.StartTick = NoteTick;
            Note.StartLaneIndex = NoteLaneIndex;
            Note.StartWidth = NoteWidth;
            Note.StartNote.Channel = NoteChannel;
            Note.Stage = NoteStage;
        }
    }
}
