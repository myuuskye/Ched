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
using Ched.Core.Events;
using Ched.Core.Notes;
using Ched.Localization;
using Ched.UI.Operations;

namespace Ched.UI.Windows
{
    /// <summary>
    /// Interaction logic for NotePropertiesWindow.xaml
    /// </summary>
    public partial class TimeSignatureEventPropertiesWindow : Window
    {
        public TimeSignatureEventPropertiesWindow()
        {
            InitializeComponent();
        }

    }

    public class TimeSignatureEventPropertiesWindowViewModel : ViewModel
    {

        private TimeSignatureChangeEvent Event { get; }
        private NoteView noteView { get; }

        private int eventTick;
        private int eventNumrator;
        private int eventDenominatorExponent;

        public int EventTick
        {
            get => eventTick;
            set
            {
                if (value == eventTick) return;
                eventTick = value;
                NotifyPropertyChanged();
            }
        }

        public int EventNumrator
        {
            get => eventNumrator;
            set
            {
                if (value == eventNumrator) return;
                eventNumrator = value;
                NotifyPropertyChanged();
            }
        }

        public int EventDenominatorExponent
        {
            get => eventDenominatorExponent;
            set
            {
                if (value == eventDenominatorExponent) return;
                eventDenominatorExponent = value;
                NotifyPropertyChanged();
            }
        }


        public TimeSignatureEventPropertiesWindowViewModel()
        {
        }

        public TimeSignatureEventPropertiesWindowViewModel(TimeSignatureChangeEvent @event)
        {
            Event = @event;
        }


        public void BeginEdit()
        {
            EventTick = Event.Tick;
            EventNumrator = Event.Numerator;
            EventDenominatorExponent = Event.DenominatorExponent;
        }

        public void CommitEdit()
        {
            Event.Tick = EventTick;
            Event.Numerator = EventNumrator;
            Event.DenominatorExponent = EventDenominatorExponent;
            Event.Type = -2;
        }
    }
}
