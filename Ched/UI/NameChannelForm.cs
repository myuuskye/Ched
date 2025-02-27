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
    public partial class NameChannelForm : Form
    {
        public string ChannelName
        {
            get => (string)textBox.Text;
            set
            {
                textBox.Text = (string)value;
                textBox.SelectAll();
            }
        }

        public NameChannelForm()
        {
            InitializeComponent();
            AcceptButton = buttonOK;
            CancelButton = buttonCancel;
            buttonOK.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            textBox.Text = "Ch ";


        }
    }
}
