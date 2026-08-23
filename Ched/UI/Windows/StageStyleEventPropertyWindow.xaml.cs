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
    public partial class StageStyleEventPropertiesWindow : Window
    {
        public StageStyleEventPropertiesWindow()
        {
            InitializeComponent();
        }

    }

    public class StageStyleEventPropertiesWindowViewModel : ViewModel
    {
        static System.Data.DataTable _dt = new System.Data.DataTable();
        private StageStyleChangeEvent Event { get; }

        private int eventTick;
        private int stage;
        private float laneIndex;
        private int judgeLineColor;
        private int judgeLineStyle;
        private int leftBorderStyle;
        private int rightBorderStyle;
        private bool fullWidth;
        private float noteAlpha;
        private float laneAlpha;
        private float judgeLineAlpha;
        private float divisionLineAlpha;
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
        public int JudgeLineColor
        {
            get => judgeLineColor;
            set
            {
                if (value == judgeLineColor) return;
                judgeLineColor = value;
                NotifyPropertyChanged();
            }
        }
        public int JudgeLineStyle
        {
            get => judgeLineStyle;
            set
            {
                if (value == judgeLineStyle) return;
                judgeLineStyle = value;
                NotifyPropertyChanged();
            }
        }
        public int LeftBorderStyle
        {
            get => leftBorderStyle;
            set
            {
                if (value == leftBorderStyle) return;
                leftBorderStyle = value;
                NotifyPropertyChanged();
            }
        }
        public int RightBorderStyle
        {
            get => rightBorderStyle;
            set
            {
                if (value == rightBorderStyle) return;
                rightBorderStyle = value;
                NotifyPropertyChanged();
            }
        }
        public bool FullWidth
        {
            get => fullWidth;
            set
            {
                if (value == fullWidth) return;
                fullWidth = value;
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
        public float LaneAlpha
        {
            get => laneAlpha;
            set
            {
                if (value == laneAlpha) return;
                laneAlpha = value;
                NotifyPropertyChanged();
            }
        }
        public float JudgeLineAlpha
        {
            get => judgeLineAlpha;
            set
            {
                if (value == judgeLineAlpha) return;
                judgeLineAlpha = value;
                NotifyPropertyChanged();
            }
        }
        public float DivisionLineAlpha
        {
            get => divisionLineAlpha;
            set
            {
                if (value == divisionLineAlpha) return;
                divisionLineAlpha = value;
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

        public StageStyleEventPropertiesWindowViewModel()
        {
        }

        public StageStyleEventPropertiesWindowViewModel(StageStyleChangeEvent @event)
        {
            Event = @event;
        }


        public void BeginEdit()
        {
            EventTick = Event.Tick;
            LaneIndex = Event.LaneIndex;
            Stage = Event.Stage;
            JudgeLineColor = Event.JudgeLineColor;
            JudgeLineStyle = Event.JudgeLineStyle;
            LeftBorderStyle = Event.LeftBorderStyle;
            RightBorderStyle = Event.RightBorderStyle;
            FullWidth = Event.FullWidth == 0 ? false : true ;
            NoteAlpha = Event.NoteAlpha;
            LaneAlpha = Event.LaneAlpha;
            JudgeLineAlpha = Event.JudgeLineAlpha;
            DivisionLineAlpha = Event.LaneAlpha;
            Ease = Event.Ease;
            
        }

        public void CommitEdit()
        {
            Event.Tick = EventTick;
            Event.LaneIndex = LaneIndex;
            Event.Stage = Stage;
            Event.JudgeLineColor = JudgeLineColor;
            Event.JudgeLineStyle = JudgeLineStyle;
            Event.LeftBorderStyle = LeftBorderStyle;
            Event.RightBorderStyle = RightBorderStyle;
            Event.FullWidth = FullWidth ? 1 : 0;
            Event.NoteAlpha = NoteAlpha;
            Event.LaneAlpha = LaneAlpha;
            Event.JudgeLineAlpha = JudgeLineAlpha;
            Event.DivisonLineAlpha = DivisionLineAlpha;
            Event.Ease = Ease;
        }
    }
}
