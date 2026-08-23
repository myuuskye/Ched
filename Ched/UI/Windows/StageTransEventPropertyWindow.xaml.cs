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
    public partial class StageTransEventPropertiesWindow : Window
    {
        public StageTransEventPropertiesWindow()
        {
            InitializeComponent();
        }

    }

    public class StageTransEventPropertiesWindowViewModel : ViewModel
    {
        static System.Data.DataTable _dt = new System.Data.DataTable();
        private StageTransformChangeEvent Event { get; }

        private int eventTick;
        private int stage;
        private float rotate;
        private float xTranslate;
        private float yTranslate;
        private bool anchor;
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

        public float XTranslate
        {
            get => xTranslate;
            set
            {
                if (value == xTranslate) return;
                xTranslate = value;
                NotifyPropertyChanged();
            }
        }
        public float YTranslate
        {
            get => yTranslate;
            set
            {
                if (value == yTranslate) return;
                yTranslate = value;
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
        public float Rotate
        {
            get => rotate;
            set
            {
                if (value == rotate) return;
                rotate = value;
                NotifyPropertyChanged();
            }
        }
        public bool Anchor
        {
            get => anchor;
            set
            {
                if (value == anchor) return;
                anchor = value;
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

        public StageTransEventPropertiesWindowViewModel()
        {
        }

        public StageTransEventPropertiesWindowViewModel(StageTransformChangeEvent @event)
        {
            Event = @event;
        }


        public void BeginEdit()
        {
            EventTick = Event.Tick;
            Stage = Event.Stage;
            XTranslate = Event.XTranslation;
            YTranslate = Event.YTranslation;
            Rotate = Event.Rotation;
            Anchor = Event.Anchor == 0 ? false : true;
            Ease = Event.Ease;
        }

        public void CommitEdit()
        {
            Event.Tick = EventTick;
            Event.Stage = Stage;
            Event.XTranslation = XTranslate;
            Event.YTranslation = YTranslate;
            Event.Rotation = Rotate;
            Event.Anchor = Anchor ? 1 : 0;
            Event.Ease = Ease;
        }
    }
}
