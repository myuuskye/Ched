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
    public partial class CommentInsertForm : Form
    {
        public string Comment
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
        public decimal TextSize
        {
            get => (decimal)sizeBox.Value;
            set
            {
                sizeBox.Value = (decimal)value;
            }
        }
        public float LaneIndex
        {
            get => (float)laneBox.Value;
            set
            {
                laneBox.Value = (decimal)value;
            }
        }

        public CommentInsertForm()
        {
            InitializeComponent();
            AcceptButton = buttonOK;
            CancelButton = buttonCancel;
            buttonOK.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            textBox.Text = "Comment";

            colorBox.SelectedIndex = 0;

            sizeBox.DecimalPlaces = 1;
            sizeBox.Increment = 1;
            sizeBox.Maximum = 10000;
            sizeBox.Minimum = 1;
            sizeBox.Value = 9;

            laneBox.DecimalPlaces = 1;
            laneBox.Increment = 1;
            laneBox.Maximum = 100000000;
            laneBox.Minimum = -100000000;
            laneBox.Value = 12;

        }
    }
}
