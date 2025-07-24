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

namespace Ched.UI
{
    public partial class VersionInfoForm : Form
    {
        public VersionInfoForm()
        {
            InitializeComponent();

            var asm = Assembly.GetEntryAssembly();

            labelTitle.Text = string.Format("{0} - {1}", asm.GetCustomAttribute<AssemblyTitleAttribute>().Title, asm.GetCustomAttribute<AssemblyDescriptionAttribute>().Description);
            labelVersion.Text = string.Format("Version {0}", asm.GetName().Version.ToString());
            labelProduct.Text = asm.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;

            ComponentResourceManager resources = new ComponentResourceManager(typeof(VersionInfoForm));
            var creditTexts = new List<string>();

            Console.WriteLine(resources.GetString(string.Format("creditText_{0}", 0)));
            Console.WriteLine(resources.GetString(string.Format("creditText_{0}", 0)) == null);
            for (int i = 0; resources.GetString(string.Format("creditText_{0}", i )) != null; i++){
                
                creditTexts.Add(resources.GetString(string.Format("creditText_{0}", i)));
            }
            creditTexts.Add(resources.GetString(string.Format("creditText_{0}", "last")));

            credits.Text = string.Join(Environment.NewLine, creditTexts);

            pictureBox1.Image = Bitmap.FromHicon(Resources.MainIcon.Handle);

            buttonClose.Click += (s, e) => Close();
        }
    }
}
