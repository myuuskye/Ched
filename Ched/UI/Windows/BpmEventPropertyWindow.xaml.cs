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
    public partial class BpmEventPropertiesWindow : Window
    {
        public BpmEventPropertiesWindow()
        {
            InitializeComponent();
        }

    }

    public class BpmEventPropertiesWindowViewModel : ViewModel
    {

        private BpmChangeEvent Event { get; }
        private NoteView noteView { get; }

        private int eventTick;
        private double eventBpm;

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

        public double EventBpm
        {
            get => eventBpm;
            set
            {
                if (value == eventBpm) return;
                eventBpm = value;
                NotifyPropertyChanged();
            }
        }


        public BpmEventPropertiesWindowViewModel()
        {
        }

        public BpmEventPropertiesWindowViewModel(BpmChangeEvent @event)
        {
            Event = @event;
        }


        public void BeginEdit()
        {
            EventTick = Event.Tick;
            EventBpm = Event.Bpm;
        }

        public void CommitEdit()
        {
            Event.Tick = EventTick;
            Event.Bpm = EventBpm;
            Event.Type = -1;
        }
    }
}
