using Ched.Core.Events;
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
    public partial class FeverEventForm : Form
    {
        public bool Start
        {
            get => (bool)bpmBox.Checked;
            set
            {
                bpmBox.Checked = (bool)value;
            }
        }
        public bool Force
        {
            get => (bool)checkBox1.Checked;
            set
            {
                checkBox1.Checked = (bool)value;
            }
        }

        public FeverEventForm()
        {
            InitializeComponent();
            AcceptButton = buttonOK;
            CancelButton = buttonCancel;
            buttonOK.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            bpmBox.Checked = false;
            checkBox1.Checked  = false;
        }

    }
}
