using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using Ched.Properties;
using Ched.Core;
using Ched.Localization;
using System.Windows.Controls;
using Ched.Configuration;

namespace Ched.UI
{
    public partial class ExportSettingsForm : Form
    {

        public Dictionary<int, bool> BoolResult = new Dictionary<int, bool>();
        public Dictionary<int, int> IntResult = new Dictionary<int, int>();
        public Dictionary<int, string> Result = new Dictionary<int, string>();


        public ScoreBook ScoreBook { get; } = new ScoreBook();


        public ExportSettingsForm(ScoreBook scoreBook)
        {
            InitializeComponent();
            AcceptButton = buttonOK;
            CancelButton = buttonClose;

            buttonClose.Click += (s, e) => Close();
            buttonOK.DialogResult = DialogResult.OK;
            buttonClose.DialogResult = DialogResult.Cancel;

            ScoreBook = scoreBook;

            tabPage11.Text = MainFormStrings.All;
            tabPage12.Text = MainFormStrings.Other;


            var defaultset = new ExportSetting();

            GridViewAll.ColumnCount = 6;

            GridViewAll.Columns[0].HeaderText = MainFormStrings.EP_name;
            GridViewAll.Columns[0].Width = 400;
            GridViewAll.Columns[0].ReadOnly = true;
            GridViewAll.Columns[1].HeaderText = MainFormStrings.EP_value;
            GridViewAll.Columns[1].Width = 40;
            GridViewAll.Columns[2].HeaderText = MainFormStrings.EP_value;
            GridViewAll.Columns[2].Width = 80;
            GridViewAll.Columns[2].ReadOnly = true;
            GridViewAll.Columns[3].HeaderText = MainFormStrings.EP_desc;
            GridViewAll.Columns[3].Width = 400;
            GridViewAll.Columns[3].ReadOnly = true;
            GridViewAll.Columns[4].HeaderText = MainFormStrings.Default;
            GridViewAll.Columns[4].Width = 60;
            GridViewAll.Columns[4].ReadOnly = true;
            GridViewAll.Columns[5].HeaderText = MainFormStrings.EP_id;
            GridViewAll.Columns[5].Width = 40;
            GridViewAll.Columns[5].ReadOnly = true;
            

            foreach (var s in defaultset.SettingColumns.OrderBy(p => p.Key))
            {
                ScoreBook.ExportSettings.TryGetValue(s.Key, out var value);
                switch (s.Value.Type)
                {
                    case "bool":
                        GridViewAll.Rows.Add(s.Value.Title, bool.Parse(value), value, s.Value.Description, ApplicationSettings.Default.DefaultExportSettings[s.Key], s.Value.ID);
                        break;
                    case "int":
                        GridViewAll.Rows.Add(s.Value.Title, true, value, s.Value.Description, ApplicationSettings.Default.DefaultExportSettings[s.Key], s.Value.ID);
                        GridViewAll.Rows[GridViewAll.Rows.Count - 1].Cells[1].ReadOnly = true;
                        break;
                    default:
                        break;
                }
            }

            if (defaultset.SettingColumns.TryGetValue((int)GridViewAll.CurrentRow.Cells[5].Value, out var colv))
            {
                listBox1.Items.Add("default");
                switch (colv.Type)
                {
                    case "bool":
                        listBox1.Items.Add(true);
                        listBox1.Items.Add(false);
                        break;
                    case "int":
                        var intsetting = (ExportIntSetting)colv;
                        foreach (var choice in intsetting.Choices)
                        {
                            listBox1.Items.Add(choice);
                        }
                        listBox1.SelectedIndex = int.Parse(intsetting.Value);
                        break;
                    default:
                        break;
                }
            }


            buttonOK.Click += (s, e) =>
            {
                for (int i = 0; i < GridViewAll.Rows.Count; i++)
                {
                    if(GridViewAll.Rows[i].Cells[2].Value != GridViewAll.Rows[i].Cells[4].Value)
                    {
                        Result.Add((int)GridViewAll.Rows[i].Cells[5].Value, GridViewAll.Rows[i].Cells[2].Value.ToString());
                        //Console.WriteLine(GridViewAll.Rows[i].Cells[2].Value + " : " + GridViewAll.Rows[i].Cells[4].Value + " count: " + Result.Count);
                        
                    }
                }

                ScoreBook.ExportSettings = Result;

                Close();
            };

            buttonConfirm.Click += (s, e) =>
            {
                if(listBox1.SelectedIndex == 0)
                {
                    switch (defaultset.SettingColumns[(int)GridViewAll.CurrentRow.Cells[5].Value].Type)
                    {
                        case "bool":
                            GridViewAll.CurrentRow.Cells[1].Value = defaultset.SettingColumns[(int)GridViewAll.CurrentRow.Cells[5].Value].Default;
                            GridViewAll.CurrentRow.Cells[2].Value = defaultset.SettingColumns[(int)GridViewAll.CurrentRow.Cells[5].Value].Default;
                            break;
                        case "int":
                            GridViewAll.CurrentRow.Cells[2].Value = defaultset.SettingColumns[(int)GridViewAll.CurrentRow.Cells[5].Value].Default;
                            break;
                        default:
                            GridViewAll.CurrentRow.Cells[2].Value = defaultset.SettingColumns[(int)GridViewAll.CurrentRow.Cells[5].Value].Default;
                            break;
                    }
                    
                }
                else
                {
                    Console.WriteLine(listBox1.SelectedItem);
                    switch (defaultset.SettingColumns[(int)GridViewAll.CurrentRow.Cells[5].Value].Type)
                    {
                        case "bool":
                            GridViewAll.CurrentRow.Cells[1].Value = listBox1.SelectedItem;
                            GridViewAll.CurrentRow.Cells[2].Value = listBox1.SelectedItem;
                            break;
                        case "int":
                            GridViewAll.CurrentRow.Cells[2].Value = listBox1.SelectedIndex - 1;
                            break;
                        default:
                            break;
                    }
                    
                }
            };
            GridViewAll.CurrentCellDirtyStateChanged += (s, e) =>
            {
                switch (defaultset.SettingColumns[(int)GridViewAll.CurrentRow.Cells[5].Value].Type)
                {
                    case "bool":
                        GridViewAll.CommitEdit(DataGridViewDataErrorContexts.Commit);
                        break;
                    default:
                        break;
                }
            };


            GridViewAll.CellValueChanged += (s, e) =>
            {
                switch (defaultset.SettingColumns[(int)GridViewAll.Rows[e.RowIndex].Cells[5].Value].Type)
                {
                    case "bool":
                        GridViewAll.Rows[e.RowIndex].Cells[2].Value = GridViewAll.Rows[e.RowIndex].Cells[1].Value;
                        break;
                    default:
                        break;
                }
            };

            GridViewAll.CellClick += (s, e) =>
            {

                if (e.RowIndex < 0) return;

                listBox1.Items.Clear();


                if (defaultset.SettingColumns.TryGetValue((int)GridViewAll.Rows[e.RowIndex].Cells[5].Value, out var value))
                {
                    listBox1.Items.Add("default");
                    switch (value.Type)
                    {
                        case "bool":
                            listBox1.Items.Add(true);
                            listBox1.Items.Add(false);
                            break;
                        case "int":
                            var intsetting = (ExportIntSetting)value;
                            foreach (var choice in intsetting.Choices)
                            {
                                listBox1.Items.Add(choice);
                            }
                            listBox1.SelectedIndex = int.Parse(intsetting.Value);
                            break;
                        default:
                            break;
                    }
                }

            };


        }

        private void ExportSettingsForm_Load(object sender, EventArgs e)
        {
            

        }


        private void tabControl_Selected(object sender, TabControlEventArgs e)
        {
            var defaultset = new ExportSetting();
            switch (e.TabPageIndex)
            {
                case 0: //ALL
                    for (int i = 0; i < GridViewAll.Rows.Count; i++)
                    {
                        GridViewAll.Rows[i].Visible = true;
                    }
                    break;
                case 1: //TAP
                    for (int i = 0; i < GridViewAll.Rows.Count; i++)
                    {
                        if (0 <= (int)GridViewAll.Rows[i].Cells[5].Value && (int)GridViewAll.Rows[i].Cells[5].Value < 500)
                        {
                            GridViewAll.Rows[i].Visible = true;
                        }
                        else
                        {
                            GridViewAll.Rows[i].Visible = false;
                        }
                        
                    }
                    break;
                case 2: //TAP2
                    for (int i = 0; i < GridViewAll.Rows.Count; i++)
                    {
                        if (500 <= (int)GridViewAll.Rows[i].Cells[5].Value && (int)GridViewAll.Rows[i].Cells[5].Value < 1000)
                        {
                            GridViewAll.Rows[i].Visible = true;
                        }
                        else
                        {
                            GridViewAll.Rows[i].Visible = false;
                        }

                    }
                    break;
                case 3: //ExTAP
                    for (int i = 0; i < GridViewAll.Rows.Count; i++)
                    {
                        if (1000 <= (int)GridViewAll.Rows[i].Cells[5].Value && (int)GridViewAll.Rows[i].Cells[5].Value < 1500)
                        {
                            GridViewAll.Rows[i].Visible = true;
                        }
                        else
                        {
                            GridViewAll.Rows[i].Visible = false;
                        }

                    }
                    break;
                case 4: //ExTAP2
                    for (int i = 0; i < GridViewAll.Rows.Count; i++)
                    {
                        if (1500 <= (int)GridViewAll.Rows[i].Cells[5].Value && (int)GridViewAll.Rows[i].Cells[5].Value < 2000)
                        {
                            GridViewAll.Rows[i].Visible = true;
                        }
                        else
                        {
                            GridViewAll.Rows[i].Visible = false;
                        }

                    }
                    break;
                case 5: //FLICK
                    for (int i = 0; i < GridViewAll.Rows.Count; i++)
                    {
                        if (2000 <= (int)GridViewAll.Rows[i].Cells[5].Value && (int)GridViewAll.Rows[i].Cells[5].Value < 2500)
                        {
                            GridViewAll.Rows[i].Visible = true;
                        }
                        else
                        {
                            GridViewAll.Rows[i].Visible = false;
                        }

                    }
                    break;
                case 6: //FLICK2
                    for (int i = 0; i < GridViewAll.Rows.Count; i++)
                    {
                        if (2500 <= (int)GridViewAll.Rows[i].Cells[5].Value && (int)GridViewAll.Rows[i].Cells[5].Value < 3000)
                        {
                            GridViewAll.Rows[i].Visible = true;
                        }
                        else
                        {
                            GridViewAll.Rows[i].Visible = false;
                        }

                    }
                    break;
                case 7: //DAMAGE
                    for (int i = 0; i < GridViewAll.Rows.Count; i++)
                    {
                        if (3000 <= (int)GridViewAll.Rows[i].Cells[5].Value && (int)GridViewAll.Rows[i].Cells[5].Value < 3500)
                        {
                            GridViewAll.Rows[i].Visible = true;
                        }
                        else
                        {
                            GridViewAll.Rows[i].Visible = false;
                        }

                    }
                    break;
                case 8: //DAMAGE2
                    for (int i = 0; i < GridViewAll.Rows.Count; i++)
                    {
                        if (3500 <= (int)GridViewAll.Rows[i].Cells[5].Value && (int)GridViewAll.Rows[i].Cells[5].Value < 4000)
                        {
                            GridViewAll.Rows[i].Visible = true;
                        }
                        else
                        {
                            GridViewAll.Rows[i].Visible = false;
                        }

                    }
                    break;
                case 9: //SLIDE
                    for (int i = 0; i < GridViewAll.Rows.Count; i++)
                    {
                        if (4000 <= (int)GridViewAll.Rows[i].Cells[5].Value && (int)GridViewAll.Rows[i].Cells[5].Value < 4500)
                        {
                            GridViewAll.Rows[i].Visible = true;
                        }
                        else
                        {
                            GridViewAll.Rows[i].Visible = false;
                        }

                    }
                    break;
                case 10: //GUIDE
                    for (int i = 0; i < GridViewAll.Rows.Count; i++)
                    {
                        if (4500 <= (int)GridViewAll.Rows[i].Cells[5].Value && (int)GridViewAll.Rows[i].Cells[5].Value < 5000)
                        {
                            GridViewAll.Rows[i].Visible = true;
                        }
                        else
                        {
                            GridViewAll.Rows[i].Visible = false;
                        }

                    }
                    break;
                case 11: //OTHER
                    for (int i = 0; i < GridViewAll.Rows.Count; i++)
                    {
                        if (5000 <= (int)GridViewAll.Rows[i].Cells[5].Value && (int)GridViewAll.Rows[i].Cells[5].Value < 5500)
                        {
                            GridViewAll.Rows[i].Visible = true;
                        }
                        else
                        {
                            GridViewAll.Rows[i].Visible = false;
                        }

                    }
                    break;
                default:
                    for (int i = 0; i < GridViewAll.Rows.Count; i++)
                    {
                        GridViewAll.Rows[i].Visible = true;
                    }
                    break;
            }
            
        }


    }
}
