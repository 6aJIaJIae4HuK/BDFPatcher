using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BDFPatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void openFiles(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".bdf";
            dlg.Multiselect = true;
            dlg.Filter = "BDF Files (*.bdf) | *.bdf";

            bool? result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                if (openedFiles == null)
                    openedFiles = new List<BDFFile>();
                openedFiles.Clear();
                foreach (string fileName in dlg.FileNames)
                {
                    openedFiles.Add(new BDFFile());
                    openedFiles.Last().readFromFile(fileName);
                }
            }
        }

        private void patchAndSaveFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.DefaultExt = ".bdf";
            dlg.Filter = "BDF Files (*.bdf) | *.bdf";

            bool? result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                if (openedFiles == null) ;//Not selected files
                BDFFile newFile = new BDFFile();
                newFile.generateFromFiles(openedFiles);
                newFile.saveToFile(dlg.FileName);
            }
        }

        private List<BDFFile> openedFiles;
    }
}
