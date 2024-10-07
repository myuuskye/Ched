using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Ched.Core;
using Ched.Components.Exporter;
using Ched.Localization;
using Ched.Plugins;

namespace Ched.UI.Windows
{
    /// <summary>
    /// Interaction logic for UscImportWindow.xaml
    /// </summary>
    public partial class UscImportWindow : Window
    {
        public UscImportWindow()
        {
            InitializeComponent();
        }
    }

    public class UscImportWindowViewModel : ViewModel
    {
        private UscArgs UscArgs { get; }
        private decimal OldLaneOffset { get; }

        private decimal laneOffset;
        public decimal LaneOffset
        {
            get => laneOffset;
            set
            {
                if (value == laneOffset) return;
                laneOffset = value;
                NotifyPropertyChanged();
            }
        }

        private bool isOverlap;
        public bool IsOverlap
        {
            get => isOverlap;
            set
            {
                if (value == isOverlap) return;
                isOverlap = value;
                NotifyPropertyChanged();
            }
        }


        public UscImportWindowViewModel()
        {
        }

        public UscImportWindowViewModel(decimal laneoffset, UscArgs uscArgs)
        {
            OldLaneOffset = laneoffset;
            UscArgs = uscArgs;
        }

        public void BeginEdit()
        {
            LaneOffset = (decimal)OldLaneOffset;
            IsOverlap = false;
        }

        public void CommitEdit()
        {
            UscArgs.LaneOffset = (decimal)LaneOffset;
            UscArgs.IsOverlap = IsOverlap;
        }
    }
}
