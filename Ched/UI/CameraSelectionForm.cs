using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ched.UI
{
    public partial class CameraSelectionForm : Form
    {
        public float LaneIndex
        {
            get => (float)laneBox.Value;
            set
            {
                laneBox.Value = (decimal)value;
                laneBox.SelectAll();
            }
        }
        public float Width
        {
            get => (float)widthBox.Value;
            set
            {
                widthBox.Value = (decimal)value;
            }
        }
        public float Zoom
        {
            get => (float)zoomBox.Value;
            set
            {
                zoomBox.Value = (decimal)value;
            }
        }
        public float ZoomLane
        {
            get => (float)zoomLaneBox.Value;
            set
            {
                zoomLaneBox.Value = (decimal)value;
            }
        }
        public float ZoomY
        {
            get => (float)zoomYBox.Value;
            set
            {
                zoomYBox.Value = (decimal)value;
            }
        }
        public int ZoomAlign
        {
            get => zoomAlignBox.SelectedIndex;
            set
            {
                zoomAlignBox.SelectedIndex = value;
            }
        }
        public float Rotation
        {
            get => (float)rotationBox.Value;
            set
            {
                rotationBox.Value = (decimal)value;
            }
        }
        public float Tilt
        {
            get => (float)stageTiltBox.Value;
            set
            {
                stageTiltBox.Value = (decimal)value;
            }
        }
        public int Ease
        {
            get => easeSelectBox.SelectedIndex;
            set
            {
                easeSelectBox.SelectedIndex = value;
            }
        }

        public CameraSelectionForm()
        {
            InitializeComponent();
            AcceptButton = buttonOK;
            CancelButton = buttonCancel;
            buttonOK.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            laneBox.DecimalPlaces = 1;
            laneBox.Increment = 1;
            laneBox.Maximum = 1000000000000;
            laneBox.Minimum = -1000000000000;
            laneBox.Value = 0;

            widthBox.DecimalPlaces = 1;
            widthBox.Increment = 1;
            widthBox.Maximum = 1000000000000;
            widthBox.Minimum = -1000000000000;
            widthBox.Value = 0;
        }
    }
}
