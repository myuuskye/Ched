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
    public partial class CameraEventPropertiesWindow : Window
    {
        public CameraEventPropertiesWindow()
        {
            InitializeComponent();
        }

    }

    public class CameraEventPropertiesWindowViewModel : ViewModel
    {
        static System.Data.DataTable _dt = new System.Data.DataTable();
        private CameraChangeEvent Event { get; }

        private int eventTick;
        private float laneIndex;
        private float width;
        private float zoom;
        private float zoomLane;
        private float zoomY;
        private bool zoomAlign;
        private float rotation;
        private float tilt;
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
        public float Zoom
        {
            get => zoom;
            set
            {
                if (value == zoom) return;
                zoom = value;
                NotifyPropertyChanged();
            }
        }
        public float ZoomLane
        {
            get => zoomLane;
            set
            {
                if (value == zoomLane) return;
                zoomLane = value;
                NotifyPropertyChanged();
            }
        }
        public float ZoomY
        {
            get => zoomY;
            set
            {
                if (value == zoomY) return;
                zoomY = value;
                NotifyPropertyChanged();
            }
        }
        public bool ZoomAlign
        {
            get => zoomAlign;
            set
            {
                if (value == zoomAlign) return;
                zoomAlign = value;
                NotifyPropertyChanged();
            }
        }
        public float Rotation
        {
            get => rotation;
            set
            {
                if (value == rotation) return;
                rotation = value;
                NotifyPropertyChanged();
            }
        }
        public float Tilt
        {
            get => tilt;
            set
            {
                if (value == tilt) return;
                tilt = value;
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

        public CameraEventPropertiesWindowViewModel()
        {
        }

        public CameraEventPropertiesWindowViewModel(CameraChangeEvent @event)
        {
            Event = @event;
        }


        public void BeginEdit()
        {
            EventTick = Event.Tick;
            LaneIndex = Event.LaneIndex;
            Width = Event.Width;
            Zoom = Event.Zoom;
            ZoomLane = Event.ZoomTargetLane;
            ZoomY = Event.ZoomTargetY;
            ZoomAlign = Event.ZoomVerticalAlign == 0 ? false : true;
            Rotation = Event.Rotation;
            Tilt = Event.Tilt;
            Ease = Event.Ease;
            
        }

        public void CommitEdit()
        {
            Event.Tick = EventTick;
            Event.LaneIndex = LaneIndex;
            Event.Width = Width;
            Event.Zoom = Zoom;
            Event.ZoomTargetLane = ZoomLane;
            Event.ZoomTargetY = ZoomY;
            Event.ZoomVerticalAlign = ZoomAlign ? 1 : 0;
            Event.Rotation = Rotation;
            Event.Tilt = Tilt;
            Event.Ease = Ease;
        }
    }
}
