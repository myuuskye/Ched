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
    public partial class NoteCollectionForm : Form
    {
        public double BeforeBpm
        {
            get => (double)bpmBox.Value;
            set
            {
                bpmBox.Value = (decimal)value;
                bpmBox.SelectAll();
            }
        }
        public double AfterBpm
        {
            get => (double)bpmBox2.Value;
            set
            {
                bpmBox2.Value = (decimal)value;
            }
        }

        public NoteCollectionForm()
        {
            InitializeComponent();
            AcceptButton = buttonOK;
            CancelButton = buttonCancel;
            buttonOK.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            bpmBox.DecimalPlaces = 1;
            bpmBox.Increment = 1;
            bpmBox.Maximum = 10000;
            bpmBox.Minimum = -10000;
            bpmBox.Value = 240;

            bpmBox2.DecimalPlaces = 1;
            bpmBox2.Increment = 1;
            bpmBox2.Maximum = 10000;
            bpmBox2.Minimum = -10000;
            bpmBox2.Value = 120;
        }
    }
}
