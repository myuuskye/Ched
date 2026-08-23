using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
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
    public partial class StageMaskEventPropertiesWindow : Window
    {
        public StageMaskEventPropertiesWindow()
        {
            InitializeComponent();
        }

    }

    public class StageMaskEventPropertiesWindowViewModel : ViewModel
    {
        static System.Data.DataTable _dt = new System.Data.DataTable();
        private StageMaskChangeEvent Event { get; }

        private int eventTick;
        private int stage;
        private float laneIndex;
        private float width;
        private int ease;

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

        public float LaneIndex
        {
            get => laneIndex;
            set
            {
                if (value == laneIndex) return;
                laneIndex = value;
                NotifyPropertyChanged();
            }
        }
        public float Width
        {
            get => width;
            set
            {
                if (value == width) return;
                width = value;
                NotifyPropertyChanged();
            }
        }
        public int Stage
        {
            get => stage;
            set
            {
                if (value == stage) return;
                stage = value;
                NotifyPropertyChanged();
            }
        }
        public int Ease
        {
            get => ease;
            set
            {
                if (value == ease) return;
                ease = value;
                NotifyPropertyChanged();
            }
        }

        public StageMaskEventPropertiesWindowViewModel()
        {
        }

        public StageMaskEventPropertiesWindowViewModel(StageMaskChangeEvent @event)
        {
            Event = @event;
        }


        public void BeginEdit()
        {
            EventTick = Event.Tick;
            LaneIndex = Event.LaneIndex;
            Width = Event.Width;
            Stage = Event.Stage;
            Ease = Event.Ease;
            
        }

        public void CommitEdit()
        {
            Event.Tick = EventTick;
            Event.LaneIndex = LaneIndex;
            Event.Width = Width;
            Event.Stage = Stage;
            Event.Ease = Ease;
        }
    }
}
