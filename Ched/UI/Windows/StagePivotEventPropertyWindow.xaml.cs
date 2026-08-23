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
    public partial class StagePivotEventPropertiesWindow : Window
    {
        public StagePivotEventPropertiesWindow()
        {
            InitializeComponent();
        }

    }

    public class StagePivotEventPropertiesWindowViewModel : ViewModel
    {
        static System.Data.DataTable _dt = new System.Data.DataTable();
        private StagePivotChangeEvent Event { get; }

        private int eventTick;
        private int stage;
        private float laneIndex;
        private int divisionSize;
        private int divisionParity;
        private float yOffset;
        private float yBeatOffset;
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
        public int DivisionSize
        {
            get => divisionSize;
            set
            {
                if (value == divisionSize) return;
                divisionSize = value;
                NotifyPropertyChanged();
            }
        }
        public int DivisionParity
        {
            get => divisionParity;
            set
            {
                if (value == divisionParity) return;
                divisionParity = value;
                NotifyPropertyChanged();
            }
        }
        public float YOffset
        {
            get => yOffset;
            set
            {
                if (value == yOffset) return;
                yOffset = value;
                NotifyPropertyChanged();
            }
        }
        public float YBeatOffset
        {
            get => yBeatOffset;
            set
            {
                if (value == yBeatOffset) return;
                yBeatOffset = value;
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

        public StagePivotEventPropertiesWindowViewModel()
        {
        }

        public StagePivotEventPropertiesWindowViewModel(StagePivotChangeEvent @event)
        {
            Event = @event;
        }


        public void BeginEdit()
        {
            EventTick = Event.Tick;
            LaneIndex = Event.LaneIndex;
            Stage = Event.Stage;
            DivisionSize = Event.DivisionSize;
            DivisionParity = Event.DivisionParity;
            YOffset = Event.YOffset;
            YBeatOffset = Event.YBeatOffset;
            Ease = Event.Ease;
            
        }

        public void CommitEdit()
        {
            Event.Tick = EventTick;
            Event.LaneIndex = LaneIndex;
            Event.Stage = Stage;
            Event.DivisionSize = DivisionSize;
            Event.DivisionParity = DivisionParity;
            Event.YOffset = YOffset;
            Event.YBeatOffset = YBeatOffset;
            Event.Ease = Ease;
        }
    }
}
