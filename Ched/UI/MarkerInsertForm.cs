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
    public partial class MarkerInsertForm : Form
    {
        public string Name
        {
            get => (string)textBox.Text;
            set
            {
                textBox.Text = (string)value;
                textBox.SelectAll();
            }
        }
        public int Color
        {
            get => colorBox.SelectedIndex;
            set
            {
                colorBox.SelectedIndex = value;
            }
        }
        public decimal MarkerWidth
        {
            get => (decimal)sizeBox.Value;
            set
            {
                sizeBox.Value = (decimal)value;
            }
        }

        public MarkerInsertForm()
        {
            InitializeComponent();
            AcceptButton = buttonOK;
            CancelButton = buttonCancel;
            buttonOK.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            textBox.Text = "New Line";

            colorBox.SelectedIndex = 0;

            sizeBox.DecimalPlaces = 1;
            sizeBox.Increment = 1;
            sizeBox.Maximum = 10000;
            sizeBox.Minimum = 1;
            sizeBox.Value = 1;

        }
    }
}
