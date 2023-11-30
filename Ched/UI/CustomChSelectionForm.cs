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
    public partial class CustomChSelectionForm : Form
    {

        public int SpeedCh
        {
            get { return (int)speedChBox.Value; }
            set
            {
                speedChBox.Value = value;

            }
        }

        public CustomChSelectionForm()
        {
            InitializeComponent();
            AcceptButton = buttonOK;
            CancelButton = buttonCancel;
            buttonOK.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;


            speedChBox.Minimum = 0;
            speedChBox.Maximum = 100000;
            speedChBox.Increment = 1;
            speedChBox.DecimalPlaces = 0;
            speedChBox.Value = 1;
        }
    }
}
