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
    public partial class SkillEventForm : Form
    {
        public SkillTypes Skill
        {
            get => (SkillTypes)bpmBox.Value;
            set
            {
                bpmBox.Value = (int)value;
                bpmBox.SelectAll();
            }
        }
        public int Level
        {
            get => (int)numericUpDown1.Value;
            set
            {
                numericUpDown1.Value = (int)value;

            }
        }

        public SkillEventForm()
        {
            InitializeComponent();
            AcceptButton = buttonOK;
            CancelButton = buttonCancel;
            buttonOK.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            bpmBox.DecimalPlaces = 0;
            bpmBox.Increment = 1;
            bpmBox.Maximum = 3;
            bpmBox.Minimum = 0;
            bpmBox.Value = 1;

            numericUpDown1.Maximum  = 4;
            numericUpDown1.Minimum = 1;
            numericUpDown1.Value = 1;
        }
    }
}
