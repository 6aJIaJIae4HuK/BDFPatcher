using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
            workerList = new List<BackgroundWorker>();
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

                workerList.Clear();

                disableButtons();


                foreach (string fileName in dlg.FileNames)
                {
                    BDFFile newFile = new BDFFile();
                    openedFiles.Add(newFile);
                    ProgressBar progressBar = new ProgressBar();

                    progressBar.Width = Double.NaN;
                    //progressBar.Height = 25.0;

                    progressBar.Minimum = 0;

                    Binding maxBinding = new Binding();
                    maxBinding.Source = newFile;
                    maxBinding.Path = new PropertyPath("Size");
                    maxBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    maxBinding.Mode = BindingMode.OneWay;
                    BindingOperations.SetBinding(progressBar, ProgressBar.MaximumProperty, maxBinding);

                    Binding curBinding = new Binding();
                    curBinding.Source = newFile;
                    curBinding.Path = new PropertyPath("Read");
                    curBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    curBinding.Mode = BindingMode.OneWay;
                    BindingOperations.SetBinding(progressBar, ProgressBar.ValueProperty, curBinding);

                    BackgroundWorker bw = new BackgroundWorker();
                    workerList.Add(bw);
                    bw.DoWork += (obj, args) => 
                    {
                        App.Current.Dispatcher.Invoke(new ThreadStart(delegate { this.stackPanel.Children.Add(progressBar); }));
                        newFile.readFromFile(fileName);
                        App.Current.Dispatcher.Invoke(new ThreadStart(delegate { this.stackPanel.Children.Remove(progressBar); }));
                    };

                    bw.RunWorkerCompleted += (obj, args) => 
                    {
                        tryEnableButtons();
                    };
                    
                    bw.RunWorkerAsync();
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
                if (openedFiles == null || !openedFiles.Any()) ;//Not selected files

                workerList.Clear();

                disableButtons();

                BDFFile newFile = new BDFFile();

                BackgroundWorker bw = new BackgroundWorker();
                workerList.Add(bw);

                ProgressBar progressBar = new ProgressBar();


                progressBar.Width = Double.NaN;
                progressBar.Height = 25.0;

                progressBar.Minimum = 0;

                Binding maxBinding = new Binding();
                maxBinding.Source = newFile;
                maxBinding.Path = new PropertyPath("Size");
                maxBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                maxBinding.Mode = BindingMode.OneWay;
                BindingOperations.SetBinding(progressBar, ProgressBar.MaximumProperty, maxBinding);

                Binding curBinding = new Binding();
                curBinding.Source = newFile;
                curBinding.Path = new PropertyPath("Wrote");
                curBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                curBinding.Mode = BindingMode.OneWay;
                BindingOperations.SetBinding(progressBar, ProgressBar.ValueProperty, curBinding);

                bw.DoWork += (obj, args) => 
                {
                    newFile.generateFromFiles(openedFiles);
                    App.Current.Dispatcher.Invoke(new ThreadStart(delegate { this.stackPanel.Children.Add(progressBar); }));
                    newFile.saveToFile(dlg.FileName);
                    App.Current.Dispatcher.Invoke(new ThreadStart(delegate { this.stackPanel.Children.Remove(progressBar); }));
                };

                bw.RunWorkerCompleted += (obj, args) =>
                {
                    tryEnableButtons();
                };

                bw.RunWorkerAsync();

            }
        }

        private bool allWorkersAreDone()
        {
            bool res = true;
            foreach (BackgroundWorker worker in workerList)
            {
                if (worker.IsBusy)
                {
                    res = false;
                    break;
                }
            }
            return res;
        }

        private void disableButtons()
        {
            openFilesButton.IsEnabled = false;
            patchAndSaveFileButton.IsEnabled = false;
        }

        private void tryEnableButtons()
        {
            if (allWorkersAreDone())
            {
                openFilesButton.IsEnabled = true;
                patchAndSaveFileButton.IsEnabled = true;
            }
        }

        private List<BackgroundWorker> workerList;
        private List<BDFFile> openedFiles;
    }
}
