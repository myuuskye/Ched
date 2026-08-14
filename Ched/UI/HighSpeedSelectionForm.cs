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
    public partial class HighSpeedSelectionForm : Form
    {
        public decimal SpeedRatio
        {
            get { return speedRatioBox.Value; }
            set
            {
                speedRatioBox.Value = value;
                speedRatioBox.SelectAll();
            }
        }

        public int SpeedCh
        {
            get { return (int)speedChBox.Value; }
            set
            {
                speedChBox.Value = value;

            }
        }

        public string CustomArgs
        {
            get { return (string)customBox.Text; }
            set
            {
                customBox.Text = value;
            }
        }
        public float SkipBeats
        {
            get { return (float)skipNumber.Value; }
            set
            {
                skipNumber.Value = (decimal)value;
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
        public bool HideNotes
        {
            get => hideNotesCheck.Checked;
            set
            {
                hideNotesCheck.Checked = value;
            }
        }
        public float EditorLane
        {
            get { return (float)editorLane.Value; }
            set
            {
                editorLane.Value = (decimal)value;

            }
        }

        public HighSpeedSelectionForm()
        {
            InitializeComponent();
            AcceptButton = buttonOK;
            CancelButton = buttonCancel;
            buttonOK.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            speedRatioBox.Minimum = -10000000000m;
            speedRatioBox.Maximum = 10000000000m;
            speedRatioBox.Increment = 0.01m;
            speedRatioBox.DecimalPlaces = 2;
            speedRatioBox.Value = 1;

            speedChBox.Minimum = 0;
            speedChBox.Maximum = 100000;
            speedChBox.Increment = 1;
            speedChBox.DecimalPlaces = 0;
            speedChBox.Value = 1;

            skipNumber.Minimum = -100000000000;
            skipNumber.Maximum =  100000000000;
            skipNumber.Increment = 1;
            skipNumber.DecimalPlaces = 0;
            skipNumber.Value = 0;

            editorLane.Minimum = -100000000000;
            editorLane.Maximum = 100000000000;
            editorLane.Increment = 1;
            editorLane.DecimalPlaces = 0;
            editorLane.Value = 8;




            ActiveControl = speedRatioBox;


        }
    }
}
