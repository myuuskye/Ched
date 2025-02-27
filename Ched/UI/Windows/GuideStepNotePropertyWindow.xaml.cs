﻿using System;
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

namespace Ched.UI.Windows
{
    /// <summary>
    /// Interaction logic for NotePropertiesWindow.xaml
    /// </summary>
    public partial class GuideStepNotePropertiesWindow : Window
    {

        public GuideStepNotePropertiesWindow()
        {
            InitializeComponent();

        }

    }

    public class GuideStepNotePropertiesWindowViewModel : ViewModel
    {

        
        private Guide.StepTap Note { get; }
        private NoteView noteView { get; }


        private int noteTick;
        private float noteLaneIndex;
        private float noteWidth;
        private int noteChannel;
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



        public GuideStepNotePropertiesWindowViewModel()
        {
        }

        public GuideStepNotePropertiesWindowViewModel(Guide.StepTap note)
        {
            Note = note;
        }

        public void BeginEdit()
        {
            NoteTick = Note.TickOffset;
            NoteChannel = Note.Channel;
            NoteLaneIndex = Note.LaneIndexOffset;
            NoteWidth = Note.WidthChange;
            LaneIndexDecimalPlaces = Note.LaneIndexOffset.ToString().Length;
            WidthDecimalPlaces = Note.WidthChange.ToString().Length;

        }

        public void CommitEdit()
        {
            if (NoteWidth + Note.ParentNote.StartWidth  < 0.01)
            {
                NoteWidth = -Note.ParentNote.StartWidth + 0.1f;
            }
            Note.TickOffset = NoteTick;
            Note.Channel = NoteChannel;
            Note.LaneIndexOffset = NoteLaneIndex;
            Note.WidthChange = NoteWidth;

        }
    }
}
