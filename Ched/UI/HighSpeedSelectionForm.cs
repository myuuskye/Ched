﻿using System;
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
        }
    }
}
