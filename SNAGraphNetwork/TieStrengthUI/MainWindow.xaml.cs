using GraphLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TieStrengthUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TieStrength _tieStrength;
        List<Tie> _report;
        SaveFileDialog saveDialog;
        bool _isManager;
        string _relType;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                _tieStrength = new TieStrength();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            saveDialog = new SaveFileDialog();
            saveDialog.FileOk += SaveDialog_FileOk;
            dgRelType.ItemsSource = new List<string> { RelType.TAKDIR, RelType.TESEKKUR, RelType.DOGUMGUNU, RelType.ALL };
            dgPersonelType.ItemsSource = new List<string> { PersonelType.MANAGER_TO_USER, PersonelType.USER_TO_USER};
            dgRelType.SelectedIndex = 0;
            dgPersonelType.SelectedIndex = 0;
        }
        private void dgPersonelType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex != -1)
            {
                string personelType = (string)((ComboBox)sender).SelectedItem;
                _isManager = personelType == PersonelType.MANAGER_TO_USER;
                _report = _tieStrength.GetReport(_relType, _isManager);
                dgList.ItemsSource = _report;
            }
        }
        private void dgRelType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex != -1)
            {
                _relType = (string)(sender as ComboBox).SelectedItem;
                _report = _tieStrength.GetReport(_relType, _isManager);
                dgList.ItemsSource = _report;
            }
        }
        private void SaveDialog_FileOk(object sender, CancelEventArgs e)
        {
            var csv = new StringBuilder();
            csv.AppendLine("sep=#");
            csv.AppendLine("UserId#DepartmentName#MaxTieStrength#OMaxTieUserId#MinTieUserId#MinTieUserId");
            foreach (var item in _report)
            {
                var newLine = string.Format("{0}#{1}#{2}#{3}#{4}#{5}", item.UserId, item.DepartmentName, item.MaxTieStrength,
                                                                       item.MaxTieUserId, item.MinTieStrength, item.MinTieUserId);
                csv.AppendLine(newLine);
            }
            string name = saveDialog.FileName + ".csv";
            File.WriteAllText(name, csv.ToString(), Encoding.GetEncoding(1252));
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            saveDialog.ShowDialog();
        }
    }
}
