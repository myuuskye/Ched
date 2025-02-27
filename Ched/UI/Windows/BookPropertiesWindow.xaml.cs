﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Ched.Configuration;
using Ched.Core;
using Ched.Localization;

namespace Ched.UI.Windows
{
    /// <summary>
    /// Interaction logic for BookPropertiesWindow.xaml
    /// </summary>
    public partial class BookPropertiesWindow : Window
    {
        public BookPropertiesWindow()
        {
            InitializeComponent();
        }

    }

    public class BookPropertiesWindowViewModel : ViewModel
    {
        public ScoreBook ScoreBook { get; } = new ScoreBook();
        private SoundSource MusicSource { get; set; } = new SoundSource();
        private SoundSource GuideSource { get; set; } = Configuration.SoundSettings.Default.GuideSound;

        public string SoundSourceFilter { get; } = Helpers.GetFilterString(FileFilterStrings.AudioFilter, SoundSource.SupportedExtensions);
        public Action<string> SetMusicSourceFileAction => path => MusicSourcePath = path;



        private string title;
        public string Title
        {
            get => title;
            set
            {
                if (value == title) return;
                title = value;
                NotifyPropertyChanged();
            }
        }

        private string artist;
        public string Artist
        {
            get => artist;
            set
            {
                if (value == artist) return;
                artist = value;
                NotifyPropertyChanged();
            }
        }

        private string notesDesigner;
        public string NotesDesigner
        {
            get => notesDesigner;
            set
            {
                if (value == notesDesigner) return;
                notesDesigner = value;
                NotifyPropertyChanged();
            }
        }

        private string musicSourcePath;
        public string MusicSourcePath
        {
            get => musicSourcePath;
            set
            {
                if (value == musicSourcePath) return;
                musicSourcePath = value;
                NotifyPropertyChanged();
            }
        }

        private double musicSourceLatency;
        public double MusicSourceLatency
        {
            get => musicSourceLatency;
            set
            {
                if (value == musicSourceLatency) return;
                musicSourceLatency = value;
                NotifyPropertyChanged();
            }
        }

        private decimal laneoffset;
        public decimal LaneOffset
        {
            get => laneoffset;
            set
            {
                laneoffset = value;
                NotifyPropertyChanged();
            }
        }
        private decimal defaultlaneoffset;
        public decimal DefaultLaneOffset
        {
            get => defaultlaneoffset;
            set
            {
                defaultlaneoffset = value;
                NotifyPropertyChanged();
            }
        }

        public bool siken;

        public bool isSiken
        {
            get => siken;
            set
            {
                siken = value;
                NotifyPropertyChanged();
            }
        }

        private double musicVolume;
        public double MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = value;
                NotifyPropertyChanged();
            }
        }

        private double guideVolume;
        public double GuideVolume
        {
            get => guideVolume;
            set
            {
                guideVolume = value;
                NotifyPropertyChanged();
            }
        }

        private double offset;
        public double Offset
        {
            get => offset;
            set
            {
                offset = value;
                NotifyPropertyChanged();
            }
        }

        private Dictionary<int, bool> exportSettings;
        public Dictionary<int, bool> ExportSettings
        {
            get => exportSettings;
            set
            {
                if (value == exportSettings) return;
                exportSettings = value;
                NotifyPropertyChanged();
            }
        }



        public BookPropertiesWindowViewModel()
        {
        }

        public BookPropertiesWindowViewModel(ScoreBook scoreBook, SoundSource musicSource)
        {
            ScoreBook = scoreBook;
            MusicSource = musicSource;
        }

        public void BeginEdit()
        {
            Title = ScoreBook.Title;
            Artist = ScoreBook.ArtistName;
            NotesDesigner = ScoreBook.NotesDesignerName;
            LaneOffset = ScoreBook.LaneOffset;
            DefaultLaneOffset = ApplicationSettings.Default.LaneOffset;

            MusicSourcePath = MusicSource.FilePath;
            MusicSourceLatency = ScoreBook.Offset;
            MusicVolume = MusicSource.Volume;
            GuideVolume = Configuration.SoundSettings.Default.GuideSound.Volume;

        }

        public void CommitEdit()
        {
            ScoreBook.Title = Title;
            ScoreBook.ArtistName = Artist;
            ScoreBook.NotesDesignerName = NotesDesigner;
            ScoreBook.LaneOffset = LaneOffset;
            ScoreBook.Offset = MusicSourceLatency;
            NoteView.laneOffset = LaneOffset;
            ApplicationSettings.Default.LaneOffset = DefaultLaneOffset;

            MusicSource.FilePath = MusicSourcePath;
            MusicSource.Latency = MusicSourceLatency;
            
            MusicSource.Volume = MusicVolume;
            var guide = Configuration.SoundSettings.Default.GuideSound;
            var tap = Configuration.SoundSettings.Default.TapSound;
            guide.Volume = GuideVolume;
            tap.Volume = GuideVolume;
            Configuration.SoundSettings.Default.GuideSound = guide;
            Configuration.SoundSettings.Default.TapSound = tap;
            Configuration.SoundSettings.Default.Save();
        }
    }
}
