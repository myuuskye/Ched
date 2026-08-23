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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            var data = ((GuideNotePropertiesWindowViewModel)DataContext);
            StageComboBox.ItemsSource = data.Stages;
            StageComboBox.SelectedValue = data.Note.Stage;

            ColorComboBox.ItemsSource = data.Colors;
            ColorComboBox.SelectedValue = (int)data.Note.GuideColor;
        }
        private void StageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((GuideNotePropertiesWindowViewModel)DataContext).NoteStage = (int)StageComboBox.SelectedValue;
        }

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((GuideNotePropertiesWindowViewModel)DataContext).NoteColor = (int)ColorComboBox.SelectedValue;
        }
    }

    public class GuideNotePropertiesWindowViewModel : ViewModel
    {

        public Guide Note { get; }
        public List<Stage> Stages { get; }
        public List<Object> Colors { get; } = new List<Object>() { new { Name = MainFormStrings.ColorNeutral, ID = 0 }, new { Name = MainFormStrings.ColorRed, ID = 1 },
        new { Name = MainFormStrings.ColorGreen, ID = 2 }, new { Name = MainFormStrings.ColorBlue, ID = 3 }, new { Name = MainFormStrings.ColorYellow, ID = 4 },
        new { Name = MainFormStrings.ColorPurple, ID = 5 }, new { Name = MainFormStrings.ColorCyan, ID = 6 }, new { Name = MainFormStrings.ColorBlack, ID = 7 }};
        private int noteTick;
        private float noteLaneIndex;
        private float noteWidth;
        private int noteChannel;
        private int noteStage;
        private int noteColor;
        private int lanedecimalPlaces;
        private int widthdecimalPlaces;
        private float noteAlpha;

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
                noteWidth = Math.Max(-Note.StartWidth, value);
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
        public float NoteAlpha
        {
            get => noteAlpha;
            set
            {
                if (value == noteAlpha) return;
                noteAlpha = value;
                NotifyPropertyChanged();
            }
        }


        public GuideNotePropertiesWindowViewModel()
        {
        }

        public GuideNotePropertiesWindowViewModel(Guide note, List<Stage> stages)
        {
            Note = note;
            Stages = stages;
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
            NoteStage = Note.Stage;
            NoteAlpha = Note.StartAlpha;

        }

        public void CommitEdit()
        {
            Note.Stage = NoteStage;
            Note.StartTick = NoteTick;
            Note.Channel = NoteChannel;
            Note.StartLaneIndex = NoteLaneIndex;
            Note.StartWidth = NoteWidth;
            Note.GuideColor = (USCGuideColor)NoteColor;
            Note.StartAlpha = NoteAlpha;
        }
    }
}
