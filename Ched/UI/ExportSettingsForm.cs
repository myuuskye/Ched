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
using Newtonsoft.Json.Linq;

namespace Ched.UI
{
    public partial class ExportSettingsForm : Form
    {

        public Dictionary<int, bool> BoolResult = new Dictionary<int, bool>();
        public Dictionary<int, int> IntResult = new Dictionary<int, int>();
        public Dictionary<int, IExportSetting> Result = new Dictionary<int, IExportSetting>();
        public Dictionary<int, string> Values = new Dictionary<int, string>();

        


        public ExportSettingsForm( Dictionary<int,IExportSetting> settings)
        {

            //はじめに ScoreBook.ExportSettings にデフォルトと違う部分だけを入れる、uscExporterでデフォルトのものと照らし合わせ、変えた部分だけデフォルトに上書きする感じで
            InitializeComponent();
            AcceptButton = buttonOK;
            CancelButton = buttonClose;

            buttonClose.Click += (s, e) => Close();
            buttonOK.DialogResult = DialogResult.OK;
            buttonClose.DialogResult = DialogResult.Cancel;


            tabPage11.Text = MainFormStrings.All;


            var defaultset = ApplicationSettings.Default.DefaultExportSettings;

            GridViewAll.ColumnCount = 7;

            GridViewAll.Columns[0].HeaderText = MainFormStrings.EP_name;
            GridViewAll.Columns[0].Width = 290;
            GridViewAll.Columns[0].ReadOnly = true;
            GridViewAll.Columns[0].Resizable = DataGridViewTriState.True;
            GridViewAll.Columns[1].HeaderText = MainFormStrings.EP_value;
            GridViewAll.Columns[1].Width = 35;
            GridViewAll.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            GridViewAll.Columns[1].Resizable = DataGridViewTriState.False;
            GridViewAll.Columns[2].HeaderText = MainFormStrings.EP_value;
            GridViewAll.Columns[2].Width = 60;
            GridViewAll.Columns[2].ReadOnly = true;
            GridViewAll.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
            GridViewAll.Columns[2].Resizable = DataGridViewTriState.True;
            GridViewAll.Columns[3].HeaderText = MainFormStrings.EP_desc;
            GridViewAll.Columns[3].Width = 400;
            GridViewAll.Columns[3].ReadOnly = true;
            GridViewAll.Columns[3].Resizable = DataGridViewTriState.True;
            GridViewAll.Columns[4].HeaderText = MainFormStrings.Default;
            GridViewAll.Columns[4].Width = 60;
            GridViewAll.Columns[4].ReadOnly = true;
            GridViewAll.Columns[4].Resizable = DataGridViewTriState.True;
            GridViewAll.Columns[5].HeaderText = MainFormStrings.EP_id;
            GridViewAll.Columns[5].Width = 40;
            GridViewAll.Columns[5].ReadOnly = true;
            GridViewAll.Columns[5].Resizable = DataGridViewTriState.True;
            GridViewAll.Columns[6].HeaderText = MainFormStrings.EP_Category;
            GridViewAll.Columns[6].Width = 60;
            GridViewAll.Columns[6].ReadOnly = true;
            GridViewAll.Columns[6].Resizable = DataGridViewTriState.True;


            foreach (KeyValuePair<int, IExportSetting> s in defaultset.OrderBy(p => p.Key))
            {
                switch (s.Value.Type)
                {
                    case SettingTypes.b:
                        if (settings.TryGetValue(s.Key, out var value))
                        {
                            DataGridViewRow row = new DataGridViewRow();
                            row.CreateCells(GridViewAll, new object[] { s.Value.Title, bool.Parse(value.Value[0]), value.Value[0], s.Value.Description, defaultset[s.Key].Value[0], s.Value.ID, s.Value.Category });
                            row.Tag = s.Value.ID;
                            GridViewAll.Rows.Add(row);
                            if (Values.ContainsKey(s.Key))
                            {
                                Values[s.Key] = value.Value[0];
                            }
                            else
                            {
                                Values.Add(s.Key, value.Value[0]);
                            }
                        }
                        else
                        {
                            DataGridViewRow row = new DataGridViewRow();
                            row.CreateCells(GridViewAll, new object[] { s.Value.Title, bool.Parse(s.Value.Value[0]), s.Value.Value[0], s.Value.Description, defaultset[s.Key].Value[0], s.Value.ID, s.Value.Category });
                            row.Tag = s.Value.ID;
                            GridViewAll.Rows.Add(row);
                        }
                        
                        break;
                    case SettingTypes.i:
                        if (settings.TryGetValue(s.Key, out var value2))
                        {
                            DataGridViewRow row = new DataGridViewRow();
                            row.CreateCells(GridViewAll, new object[] { s.Value.Title, true, value2.Value[0], s.Value.Description, defaultset[s.Key].Value[0], s.Value.ID, s.Value.Category });
                            row.Tag = s.Value.ID;
                            GridViewAll.Rows.Add(row);
                            GridViewAll.Rows[GridViewAll.Rows.Count - 1].Cells[1].ReadOnly = true;
                            if (Values.ContainsKey(s.Key))
                            {
                                Values[s.Key] = value2.Value[0];
                            }
                            else
                            {
                                Values.Add(s.Key, value2.Value[0]);
                            }
                        }
                        else
                        {
                            DataGridViewRow row = new DataGridViewRow();
                            row.CreateCells(GridViewAll, new object[] { s.Value.Title, true, s.Value.Value[0], s.Value.Description, defaultset[s.Key].Value[0], s.Value.ID, s.Value.Category });
                            row.Tag = s.Value.ID;
                            GridViewAll.Rows.Add(row);
                            GridViewAll.Rows[GridViewAll.Rows.Count - 1].Cells[1].ReadOnly = true;
                        }
                        
                        break;
                    default:
                        break;
                }
                var category = s.Value.Category;
                if (!tabControl1.TabPages.ContainsKey(category))
                {
                    tabControl1.TabPages.Add(category, category);
                }
            }
            
            
            
            if (defaultset.TryGetValue((int)GridViewAll.CurrentRow.Tag, out var colv))
            {
                listBox1.Items.Add("default");
                switch (colv.Type)
                {
                    case SettingTypes.b:
                        listBox1.Items.Add(true);
                        listBox1.Items.Add(false);
                        break;
                    case SettingTypes.i:
                        var intsetting = (ExportIntSetting)colv;
                        foreach (var choice in intsetting.Choices)
                        {
                            listBox1.Items.Add(choice);
                        }
                        listBox1.SelectedIndex = 0;
                        break;
                    default:
                        break;
                }
            }

            buttonReset.Click += (s, e) =>
            {
                //if(Values.Count  > 0)
                
                    Values.Clear();
                    GridViewAll.Rows.Clear();
                    defaultset = ApplicationSettings.Default.DefaultExportSettings;
                    foreach (KeyValuePair<int, IExportSetting> se in defaultset.OrderBy(p => p.Key))
                    {
                        switch (se.Value.Type)
                        {
                            case SettingTypes.b:

                            DataGridViewRow row = new DataGridViewRow();
                            row.CreateCells(GridViewAll, new object[] { se.Value.Title, bool.Parse(se.Value.Value[0]), se.Value.Value[0], se.Value.Description, defaultset[se.Key].Value[0], se.Value.ID, se.Value.Category });
                            row.Tag = se.Value.ID;
                            GridViewAll.Rows.Add(row);

                                
                                if (Values.ContainsKey(se.Key))
                                {
                                    
                                    Values[se.Key] = se.Value.Value[0];
                                }
                                else
                                {
                                    Values.Add(se.Key, se.Value.Value[0]);
                                }
                                break;
                            case SettingTypes.i:
                            DataGridViewRow rowi = new DataGridViewRow();
                            rowi.CreateCells(GridViewAll, new object[] { se.Value.Title, true, se.Value.Value[0], se.Value.Description, defaultset[se.Key].Value[0], se.Value.ID, se.Value.Category });
                            rowi.Tag = se.Value.ID;
                            GridViewAll.Rows.Add(rowi);
                                GridViewAll.Rows[GridViewAll.Rows.Count - 1].Cells[1].ReadOnly = true;

                                if (Values.ContainsKey(se.Key))
                                {
                                    Values[se.Key] = se.Value.Value[0];
                                }
                                else
                                {
                                    Values.Add(se.Key, se.Value.Value[0]);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                
                
            };


            buttonOK.Click += (s, e) =>
            {
                foreach(var value in Values)
                {
                    switch (defaultset[value.Key].Type)
                    {
                        case SettingTypes.b:
                            if (value.Value != defaultset[value.Key].Value[0])
                            {
                                var newsetting = new ExportBoolSetting((ExportBoolSetting)defaultset[value.Key]);
                                newsetting.Value[0] = value.Value;

                                Result.Add(value.Key, newsetting);

                            }
                            break;
                        case SettingTypes.i:
                            if (value.Value != defaultset[value.Key].Value[0])
                            {
                                var newsetting = new ExportIntSetting((ExportIntSetting)defaultset[value.Key]);
                                newsetting.Value[0] = value.Value;
                                Result.Add(value.Key, newsetting);
                            }
                            break;
                        case SettingTypes.list:
                            break;
                    }
                    
                }
                foreach(var setting in Result)
                {
                    Console.WriteLine(setting.Value.Category + " " + setting.Value.Title + " " + setting.Value.Value[0]);
                }

            };
            

            buttonConfirm.Click += (s, e) =>
            {
                if (GridViewAll.CurrentRow == null || listBox1.SelectedItem == null) return;
                if (listBox1.SelectedIndex == 0)
                {
                    switch (defaultset[(int)GridViewAll.CurrentRow.Tag].Type)
                    {
                        case SettingTypes.b:
                            Values.Remove(defaultset[(int)GridViewAll.CurrentRow.Tag].ID);
                            GridViewAll.CurrentRow.Cells[1].Value = defaultset[(int)GridViewAll.CurrentRow.Tag].Value[0];
                            GridViewAll.CurrentRow.Cells[2].Value = defaultset[(int)GridViewAll.CurrentRow.Tag].Value[0];
                            Console.WriteLine(defaultset[(int)GridViewAll.CurrentRow.Tag].Value[0]);
                            break;
                        case SettingTypes.i:
                            Values.Remove(defaultset[(int)GridViewAll.CurrentRow.Tag].ID);
                            GridViewAll.CurrentRow.Cells[2].Value = defaultset[(int)GridViewAll.CurrentRow.Tag].Value[0];
                            break;
                        default:
                            GridViewAll.CurrentRow.Cells[2].Value = defaultset[(int)GridViewAll.CurrentRow.Tag].Value[0];
                            break;
                    }
                    
                }
                else
                {
                    //Console.WriteLine(listBox1.SelectedItem);
                    
                    switch (defaultset[(int)GridViewAll.CurrentRow.Tag].Type)
                    {
                        case SettingTypes.b:
                            if(defaultset[(int)GridViewAll.CurrentRow.Tag].Value[0] != listBox1.SelectedItem.ToString())
                            {
                                var setting = listBox1.SelectedItem.ToString();
                                if (Values.ContainsKey(defaultset[(int)GridViewAll.CurrentRow.Tag].ID))
                                {
                                    Values[defaultset[(int)GridViewAll.CurrentRow.Tag].ID] = setting;
                                }
                                else
                                {
                                    Values.Add(defaultset[(int)GridViewAll.CurrentRow.Tag].ID, setting);
                                }
                                
                            }
                            
                            GridViewAll.CurrentRow.Cells[1].Value = (bool)listBox1.SelectedItem;
                            GridViewAll.CurrentRow.Cells[2].Value = (bool)listBox1.SelectedItem;
                            break;
                        case SettingTypes.i:
                            if (defaultset[(int)GridViewAll.CurrentRow.Tag].Value[0] != listBox1.SelectedItem.ToString())
                            {
                                /*
                                var setting = defaultset[(int)GridViewAll.CurrentRow.Cells[5].Value];
                                setting.Value[0] = (listBox1.SelectedIndex - 1).ToString();
                                setting.Default[0] = defaultset[(int)GridViewAll.CurrentRow.Cells[5].Value].Default[0];
                                
                                if (Result.ContainsKey(defaultset[(int)GridViewAll.CurrentRow.Cells[5].Value].ID))
                                {
                                    Result[defaultset[(int)GridViewAll.CurrentRow.Cells[5].Value].ID] = setting;
                                }
                                else
                                {
                                    Result.Add(defaultset[(int)GridViewAll.CurrentRow.Cells[5].Value].ID, setting);
                                }
                                */
                                var setting = (listBox1.SelectedIndex - 1).ToString();
                                if (Values.ContainsKey(defaultset[(int)GridViewAll.CurrentRow.Tag].ID))
                                {
                                    Values[defaultset[(int)GridViewAll.CurrentRow.Tag].ID] = setting;
                                }
                                else
                                {
                                    Values.Add(defaultset[(int)GridViewAll.CurrentRow.Tag].ID, setting);
                                }

                            }
                            GridViewAll.CurrentRow.Cells[2].Value = (int)listBox1.SelectedIndex - 1;
                            break;
                        default:
                            break;
                    }
                    
                }
            };
            
            GridViewAll.CurrentCellDirtyStateChanged += (s, e) =>
            {
                switch (defaultset[(int)GridViewAll.CurrentRow.Tag].Type)
                {
                    case SettingTypes.b:
                        GridViewAll.CommitEdit(DataGridViewDataErrorContexts.Commit);
                        break;
                    case SettingTypes.i:
                        GridViewAll.CommitEdit(DataGridViewDataErrorContexts.Commit);
                        break;
                    default:
                        break;
                }
            };


            GridViewAll.CellContentClick += (s, e) =>
            {

                if (e.ColumnIndex != 1 || e.RowIndex < 0) return;
                Console.WriteLine("content click");
                switch (defaultset[(int)GridViewAll.Rows[e.RowIndex].Tag].Type)
                {
                    case SettingTypes.b:
                        GridViewAll.Rows[e.RowIndex].Cells[2].Value = GridViewAll.Rows[e.RowIndex].Cells[1].Value;
                        if (Values.ContainsKey(defaultset[(int)GridViewAll.Rows[e.RowIndex].Tag].ID))
                        {
                            Values[defaultset[(int)GridViewAll.Rows[e.RowIndex].Tag].ID] = GridViewAll.Rows[e.RowIndex].Cells[1].Value.ToString();
                        }
                        else
                        {
                            Values.Add(defaultset[(int)GridViewAll.CurrentRow.Tag].ID, GridViewAll.Rows[e.RowIndex].Cells[1].Value.ToString());
                        }
                        break;
                    case SettingTypes.i:

                        if (int.Parse(GridViewAll.Rows[e.RowIndex].Cells[2].Value.ToString()) >= ((ExportIntSetting)defaultset[(int)GridViewAll.Rows[e.RowIndex].Cells[5].Value]).Max)
                        {
                            GridViewAll.Rows[e.RowIndex].Cells[2].Value = ((ExportIntSetting)defaultset[(int)GridViewAll.Rows[e.RowIndex].Tag]).Min;
                        }
                        else
                        {
                            GridViewAll.Rows[e.RowIndex].Cells[2].Value = int.Parse(GridViewAll.Rows[e.RowIndex].Cells[2].Value.ToString()) + 1;
                        }
                        listBox1.SelectedIndex = int.Parse(GridViewAll.Rows[e.RowIndex].Cells[2].Value.ToString()) + 1;
                        if (Values.ContainsKey(defaultset[(int)GridViewAll.Rows[e.RowIndex].Tag].ID))
                        {
                            Values[defaultset[(int)GridViewAll.Rows[e.RowIndex].Tag].ID] = (listBox1.SelectedIndex - 1).ToString();
                        }
                        else
                        {
                            Values.Add(defaultset[(int)GridViewAll.CurrentRow.Tag].ID, (listBox1.SelectedIndex - 1).ToString());
                        }
                        break;
                    default:
                        listBox1.SelectedIndex = 0;
                        break;
                }
            };
            



            GridViewAll.CellClick += (s, e) =>
            {

                if (e.RowIndex < 0) return;
                listBox1.Items.Clear();
                

                if (defaultset.TryGetValue((int)GridViewAll.Rows[e.RowIndex].Tag, out var value))
                {
                    listBox1.Items.Add("default");
                    listBox1.SelectedIndex = 0;
                    switch (value.Type)
                    {
                        case SettingTypes.b:
                            listBox1.Items.Add(true);
                            listBox1.Items.Add(false);
                            break;
                        case SettingTypes.i:
                            var intsetting = (ExportIntSetting)value;
                            foreach (var choice in intsetting.Choices)
                            {
                                listBox1.Items.Add(choice);
                            }
                            listBox1.SelectedIndex = int.Parse(GridViewAll.Rows[e.RowIndex].Cells[2].Value.ToString()) + 1;
                            /*
                            if (Result.TryGetValue((int)GridViewAll.Rows[e.RowIndex].Cells[5].Value, out var value2))
                            {
                                listBox1.SelectedIndex = int.Parse(value2.Value[0]) + 1;
                            }
                            else
                            {
                                listBox1.SelectedIndex = int.Parse(value.Value[0]) + 1;
                            }
                            */

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

            foreach(DataGridViewRow row in GridViewAll.Rows)
            {
                var category = defaultset.SettingColumns[(int)row.Tag].Category;
                if (e.TabPageIndex == 0 || e.TabPage.Text == category)
                {
                    row.Visible = true;
                }
                else
                {
                    row.Visible = false;
                }
            }
            if(GridViewAll.Rows.GetFirstRow(DataGridViewElementStates.Visible) != -1)
                GridViewAll.Rows[GridViewAll.Rows.GetFirstRow(DataGridViewElementStates.Visible)].Selected = true;
        }


    }
}
