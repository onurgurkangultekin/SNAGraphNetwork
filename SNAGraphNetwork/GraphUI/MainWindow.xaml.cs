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

namespace GraphUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Centrality _centrality;
        List<User> _report;
        SaveFileDialog saveDialog;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                _centrality = new Centrality();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Error",MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }
            saveDialog = new SaveFileDialog();
            saveDialog.FileOk += SaveDialog_FileOk;
            dgRelType.ItemsSource = new List<string> { RelType.TAKDIR, RelType.TESEKKUR, RelType.DOGUMGUNU };
            dgRelType.SelectedIndex = 0;
        }
        private void dgDept_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedIndex != -1)
            {
                string deptName = (string)((ComboBox)sender).SelectedItem;
                dgUser.ItemsSource = _report.Where(x => x.DepartmentName == deptName);
            }
        }
        private void dgRelType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string relType = (string)(sender as ComboBox).SelectedItem;
            _report = _centrality.GetReport(relType);
            dgUser.ItemsSource = _report;
            dgDept.ItemsSource = _report.GroupBy(x => x.DepartmentName).Select(group => group.First().DepartmentName);
            dgDept.SelectedIndex = -1;
        }
        private void SaveDialog_FileOk(object sender, CancelEventArgs e)
        {
            var csv = new StringBuilder();
            csv.AppendLine("sep=#");
            csv.AppendLine("UserId#DepartmentName#InDegreeCentrality#OutDegreeCentrality#ClosenessCentrality#BetweennessCentrality#EigenvectorCentrality#ComponentNo#ComponentSize");
            foreach (var item in _report)
            {
                var newLine = string.Format("{0}#{1}#{2}#{3}#{4}#{5}#{6}#{7}#{8}", item.UserId, item.DepartmentName, item.InDegreeCentrality,
                                                                           item.OutDegreeCentrality, item.ClosenessCentrality,
                                                                           item.BetweennessCentrality, item.EigenvectorCentrality,
                                                                           item.ComponentNo, item.ComponentSize);
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
