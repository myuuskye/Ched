using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Ched.Core;
using Microsoft.Xaml.Behaviors;

namespace Ched.UI.Windows.Behaviors
{
    public class OpenSettingBehavior : Behavior<System.Windows.Controls.Button>
    {

        public static readonly DependencyProperty CallbackActionProperty = DependencyProperty.RegisterAttached("CallbackAction", typeof(Action<Dictionary<int, IExportSetting>>), typeof(OpenSettingBehavior), new FrameworkPropertyMetadata(null));
        public Action<Dictionary<int, IExportSetting>> CallbackAction
        {
            get => (Action<Dictionary<int, IExportSetting>>)GetValue(CallbackActionProperty);
            set => SetValue(CallbackActionProperty, value);
        }
        public static readonly DependencyProperty ScoreBookProperty = DependencyProperty.RegisterAttached("scoreBook", typeof(ScoreBook), typeof(OpenSettingBehavior), new FrameworkPropertyMetadata(null));
        public ScoreBook scoreBook
        {
            get => (ScoreBook)GetValue(ScoreBookProperty);
            set => SetValue(ScoreBookProperty, value);
        }
        public static readonly DependencyProperty ExportSettingsProperty = DependencyProperty.RegisterAttached("Settings", typeof(Dictionary<int, IExportSetting>), typeof(OpenSettingBehavior), new FrameworkPropertyMetadata(null));
        public Dictionary<int, IExportSetting> Settings
        {
            get => (Dictionary<int, IExportSetting>)GetValue(ExportSettingsProperty);
            set => SetValue(ExportSettingsProperty, value);
        }


        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Click += OnClick;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Click -= OnClick;
        }
        private void OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new ExportSettingsForm(Settings)
            {
                
            };
            var result = dialog.ShowDialog();
            if(result == DialogResult.OK)
            {
                Console.WriteLine("result " + dialog.Result.Count);
                
                CallbackAction?.Invoke(dialog.Result);
            }
            
        }
    }
}
