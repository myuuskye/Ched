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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            var data = ((SlideStepNotePropertiesWindowViewModel)DataContext);
            StageComboBox.ItemsSource = data.Stages;
            StageComboBox.SelectedValue = data.Note.Stage;
        }
        private void StageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((SlideStepNotePropertiesWindowViewModel)DataContext).NoteStage = (int)StageComboBox.SelectedValue;
        }

    }

    public class SlideStepNotePropertiesWindowViewModel : ViewModel
    {

        public Slide.StepTap Note { get; }
        private bool IsEnd { get; }
        public List<Stage> Stages { get; }

        private int noteTick;
        private float noteLaneIndex;
        private float noteWidth;
        private int noteChannel;
        private int noteStage;
        private bool noteVisible;
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
                noteWidth = Math.Max(-Note.ParentNote.StartWidth, value);
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


        public SlideStepNotePropertiesWindowViewModel()
        {
        }

        public SlideStepNotePropertiesWindowViewModel(Slide.StepTap note, bool isend, List<Stage> stages)
        {
            Note = note;
            IsEnd = isend;
            Stages = stages;
        }

        public void BeginEdit()
        {
            NoteTick = Note.TickOffset;
            NoteChannel = Note.Channel;
            NoteStage = Note.Stage;
            NoteLaneIndex = Note.LaneIndexOffset;
            NoteWidth = Note.WidthChange;
            LaneIndexDecimalPlaces = Note.LaneIndexOffset.ToString().Length;
            WidthDecimalPlaces = Note.WidthChange.ToString().Length;
            if (IsEnd)
            NoteVisible = true;
            else
            NoteVisible = Note.IsVisible;

        }

        public void CommitEdit()
        {

            Note.TickOffset = NoteTick;
            Note.Channel = NoteChannel;
            Note.Stage = NoteStage;
            Note.LaneIndexOffset = NoteLaneIndex;
            Note.WidthChange = NoteWidth;
            if (IsEnd)
                Note.IsVisible = true;
            else
                Note.IsVisible = NoteVisible;
        }
    }
}
