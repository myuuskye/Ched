using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Ched.UI.Windows
{
    /// <summary>
    /// Interaction logic for BindableCheckBox .xaml
    /// </summary>
    public partial class BindableCheckBox : WindowsFormsHost, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(bool), typeof(BindableCheckBox),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnValueChanged)));
        public bool Value
        {
            get => (bool)GetValue(ValueProperty);
            set
            {
                SetValue(ValueProperty, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }



        public BindableCheckBox()
        {
            InitializeComponent();
            foreach (string prop in new[] { "Value" })
                InitializePropertyBinding(prop);
        }

        private void InitializePropertyBinding(string name)
        {
            var src = new BindingSource();
            var initializer = (ISupportInitialize)src;
            initializer.BeginInit();
            var child = (CheckBox)Child;
            
            child.DataBindings.Add(new System.Windows.Forms.Binding("Checked", src, name, true, DataSourceUpdateMode.OnPropertyChanged));
            src.DataSource = typeof(BindableCheckBox);
            initializer.EndInit();
            src.DataSource = this;
        }

        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as BindableCheckBox;
            if (control == null) return;
            if (e.Property == ValueProperty) control.Value = (bool)e.NewValue;
        }

        
    }
}
