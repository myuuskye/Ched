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
    public partial class HighSpeedEventPropertiesWindow : Window
    {
        public HighSpeedEventPropertiesWindow()
        {
            InitializeComponent();
        }

    }

    public class HighSpeedEventPropertiesWindowViewModel : ViewModel
    {

        private HighSpeedChangeEvent Event { get; }
        private NoteView noteView { get; }

        private int eventTick;
        private decimal eventSpeedRatio;
        private int eventSpeedCh;

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

        public decimal EventSpeedRatio
        {
            get => eventSpeedRatio;
            set
            {
                if (value == eventSpeedRatio) return;
                eventSpeedRatio = value;
                NotifyPropertyChanged();
            }
        }
        public int EventSpeedCh
        {
            get => eventSpeedCh;
            set
            {
                if (value == eventSpeedCh) return;
                eventSpeedCh = value;
                NotifyPropertyChanged();
            }
        }


        public HighSpeedEventPropertiesWindowViewModel()
        {
        }

        public HighSpeedEventPropertiesWindowViewModel(HighSpeedChangeEvent @event)
        {
            Event = @event;
        }


        public void BeginEdit()
        {
            EventTick = Event.Tick;
            EventSpeedRatio = Event.SpeedRatio; 
            EventSpeedCh = Event.SpeedCh;
        }

        public void CommitEdit()
        {
            Event.Tick = EventTick;
            Event.SpeedRatio = EventSpeedRatio;
            Event.SpeedCh = EventSpeedCh;
            Event.Type = EventSpeedCh;
        }
    }
}
