using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Ched.Core;
using Ched.Components.Exporter;
using Ched.Localization;
using Ched.Plugins;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Collections.Specialized;

namespace Ched.UI.Windows
{
    /// <summary>
    /// Interaction logic for StageManagerWindow.xaml
    /// </summary>
    public partial class StageManagerWindow : Window
    {
        
        public StageManagerWindow()
        {
            InitializeComponent();
        }
        

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            int inputNumber = (int)InputNumberBox.Value;
            string inputText = InputTextBox.Text;
            var book = ((StageManagerWindowViewModel)DataContext).ScoreBook;
            var items = ((StageManagerWindowViewModel)DataContext).Items;

            if (!string.IsNullOrWhiteSpace(inputText))
            {
                while(items.Where(p => p.ID == book.StageCount).Count() > 0)
                {
                    book.StageCount++;
                }
                ((StageManagerWindowViewModel)DataContext).Items.Add(new Stage { ID = book.StageCount, Name = inputText, Number = inputNumber });
                InputTextBox.Clear();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemListBox.SelectedItem is Stage selectedItem)
            {
                if(ItemListBox.Items.Count > 1)
                ((StageManagerWindowViewModel)DataContext).Items.Remove(selectedItem);
            }
        }
        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemListBox.SelectedItem is Stage selectedItem)
            {
                int inputNumber = (int)InputNumberBox.Value;
                string inputText = InputTextBox.Text;
                int index = ItemListBox.SelectedIndex;
                if (!string.IsNullOrWhiteSpace(inputText))
                {
                    ((StageManagerWindowViewModel)DataContext).Items[index] = new Stage(selectedItem) { Name = inputText, Number = inputNumber };
                    ItemListBox.SelectedIndex = index;
                    InputTextBox.Clear();
                }
                
            }
        }
        private void ItemListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ItemListBox.SelectedItem is Stage selectedItem)
            {
                var book = ((StageManagerWindowViewModel)DataContext).ScoreBook;
                var taps = book.Score.Notes.Taps.Where(p => p.Stage == selectedItem.ID);
                var extaps = book.Score.Notes.ExTaps.Where(p => p.Stage == selectedItem.ID);
                var flicks = book.Score.Notes.Flicks.Where(p => p.Stage == selectedItem.ID);
                var damages = book.Score.Notes.Damages.Where(p => p.Stage == selectedItem.ID);
                MessageBox.Show($"{MainFormStrings.Stage}ID: {selectedItem.ID}\n{MainFormStrings.Name}: {selectedItem.Name}\n{MainFormStrings.Number}: {selectedItem.Number}\n" +
                    $"FromStart: {selectedItem.FromStart}\nUntilEnd: {selectedItem.UntilEnd}\nIsolatedSimLines: {selectedItem.SimLines}\n" +
                    $"TAP: {taps.Count()}  ExTAP: {extaps.Count()}\nFLICK: {flicks.Count()}  DAMAGE: {damages.Count()}",
                                $"ステージ {selectedItem.Name}");
            }
        }

    }

    public class StageManagerWindowViewModel : ViewModel
    {
        private List<Stage>  stages { get; set; }
        private ObservableCollection<Stage> items { get; set; }
        public ScoreBook ScoreBook { get; set; }

        public List<Stage> Stages 
        {
            get => stages;
            set
            {
                if (value == stages) return;
                stages = value;
                NotifyPropertyChanged();
            }
        }
        public ObservableCollection<Stage> Items
        {
            get => items;
            set
            {
                if (value == items) return;
                items = value;
                NotifyPropertyChanged();
            }
        }



        public StageManagerWindowViewModel()
        {
        }

        public StageManagerWindowViewModel( ScoreBook book)
        {
            Stages = book.Stages;
            ScoreBook = book;
        }

        public void BeginEdit()
        {
            Items = new ObservableCollection<Stage>(Stages);
        }

        public void CommitEdit()
        {
            Stages = Items.ToList();
            ScoreBook.Stages = Stages;
        }
    }
}
