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
    public partial class HighSpeedEventPropertiesWindow : Window
    {
        public HighSpeedEventPropertiesWindow()
        {
            InitializeComponent();
        }

    }

    public class HighSpeedEventPropertiesWindowViewModel : ViewModel
    {
        static System.Data.DataTable _dt = new System.Data.DataTable();
        private HighSpeedChangeEvent Event { get; }
        private NoteView NoteView { get; }
        private int Channel { get; }

        private int eventTick;
        private decimal eventSpeedRatio;
        private int eventSpeedCh;
        private string customArgs;

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

        public string EventCustomArgs
        {
            get => customArgs;
            set
            {
                if (value == customArgs) return;
                customArgs = value;
                NotifyPropertyChanged();
            }
        }

        public HighSpeedEventPropertiesWindowViewModel()
        {
        }

        public HighSpeedEventPropertiesWindowViewModel(HighSpeedChangeEvent @event, NoteView noteview)
        {
            Event = @event;
            NoteView = noteview;
        }


        public void BeginEdit()
        {
            EventTick = Event.Tick;
            EventSpeedRatio = Event.SpeedRatio; 
            EventSpeedCh = Event.SpeedCh;
            EventCustomArgs = Event.CustomArgs;
            
        }

        public void CommitEdit()
        {
            Event.Tick = EventTick;
            Event.SpeedRatio = EventSpeedRatio;
            Event.SpeedCh = EventSpeedCh;
            Event.Type = EventSpeedCh;
            Event.CustomArgs = EventCustomArgs;

            if(EventCustomArgs.Length > 0)
            {
                var bpm = NoteView.ScoreEvents.BpmChangeEvents.OrderBy(p => p.Tick).LastOrDefault(p => p.Tick <= NoteView.CurrentTick)?.Bpm ?? 120;
                var customArgs = EventCustomArgs.Replace("{channel}", NoteView.Channel.ToString()).Replace("{spchannel}", EventSpeedCh.ToString()).Replace("{bpm}", bpm.ToString());
                try
                {
                    
                    string s = _dt.Compute("1.0 *" + customArgs, "").ToString();
                    Event.SpeedRatio = decimal.Parse(s.ToString());
                }
                catch
                {
                    Event.SpeedRatio = EventSpeedRatio;
                    System.Windows.Forms.MessageBox.Show(ErrorStrings.ArgsException, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
