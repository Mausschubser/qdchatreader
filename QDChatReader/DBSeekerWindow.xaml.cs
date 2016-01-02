using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
using System.Windows.Shapes;

namespace QDChatReader
{
    /// <summary>
    /// Interaction logic for DBSeekerWindow.xaml
    /// </summary>
    public partial class DBSeekerWindow : Window
    {
        public DBFiles DBFilesList = new DBFiles();
        private DataTable validFileTable = new DataTable();
        private BackgroundWorker bgSeeker = new BackgroundWorker();

        public DBSeekerWindow()
        {
            InitializeComponent();
            this.DataContext = ((App)Application.Current).QDChatReaderData;
            InitValidFileTable(validFileTable);
            gridViewFileList.ItemsSource = validFileTable.AsDataView();
            bgSeeker.WorkerReportsProgress = true;
            bgSeeker.WorkerSupportsCancellation = true;
            bgSeeker.DoWork += new DoWorkEventHandler(bgSeeker_DoWork);
            bgSeeker.ProgressChanged += new ProgressChangedEventHandler(bgSeeker_ProgressChanged);
            bgSeeker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgSeeker_RunWorkerCompleted);
        }

        public delegate void UpdateGridDelegate();
        public void UpdateGrid()
        {
            gridViewFileList.Items.Refresh();
        }

        public delegate void UpdateStatusLabelDelegate(string statustext);
        public void UpdateStatusLabel(string statustext)
        {
            toolStripStatusLabel.Text = statustext;
        }

        public delegate void UpdateProgressBarSeekDelegate(int value);
        public void UpdateProgressBarSeek(int value)
        {
            progressBarSeek.Value = value;
        }


        private void bgSeeker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            toolStripStatusLabel.Dispatcher.Invoke(new UpdateStatusLabelDelegate(UpdateStatusLabel), new object[] { "collecting files from the root folder..." });
            DBFilesList.SeekAll(((App)Application.Current).QDChatReaderData.RootFolder);
            //Console.WriteLine("start validating...");
            int totalfilescount = DBFilesList.FileList.Count;
            int filescounted = 0;
            int percentage = 0;
            string statustext = "";
            DateTime timestamp = DateTime.Now; ;
            foreach (DBFiles.DBFile file in DBFilesList.FileList)
            {
                if ((worker.CancellationPending == true))
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    if (DBFilesList.FileValidate(file.name))
                    {   //gültiges file gefunden.
                        DBFilesList.ValidFileList.Add(file);
                        AddValidFileTable(validFileTable, file);
                        //Aktualisierung der Anzeige über Thread-sicheres Delegate
                        //if (gridViewFileList.Dispatcher.)
                          gridViewFileList.Dispatcher.Invoke(new UpdateGridDelegate(UpdateGrid));
                       
                    }
                    filescounted++;
                    if ((DateTime.Now - timestamp).Milliseconds > 200)
                    {
                        percentage = filescounted * 100 / totalfilescount;
                        statustext = "" + filescounted + "/" + totalfilescount + " (" + percentage + "%)";
                        worker.ReportProgress(percentage, statustext);
                        timestamp = DateTime.Now;
                    }
                }
            }
            Console.WriteLine("end validating found: " + DBFilesList.ValidFileList.Count);
        }

        private void bgSeeker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int percent = e.ProgressPercentage;
            progressBarSeek.Dispatcher.Invoke(new UpdateProgressBarSeekDelegate(UpdateProgressBarSeek), new object[] { percent });
            toolStripStatusLabel.Dispatcher.Invoke(new UpdateStatusLabelDelegate(UpdateStatusLabel), new object[] { e.UserState.ToString() });
        }

        private void bgSeeker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            int percent = 100;
            progressBarSeek.Dispatcher.Invoke(new UpdateProgressBarSeekDelegate(UpdateProgressBarSeek), new object[] { percent });
            toolStripStatusLabel.Dispatcher.Invoke(new UpdateStatusLabelDelegate(UpdateStatusLabel), new object[] { "done."});
        }

        private void bgSeekerCancel()
        {
            if (bgSeeker.WorkerSupportsCancellation == true)
            {
                bgSeeker.CancelAsync();
            }
        }

        private void InitValidFileTable(DataTable fileTable)
        {
            fileTable.Columns.Add("File");
            fileTable.Columns.Add("Date");
            fileTable.Columns.Add("Size");
            fileTable.Locale = System.Globalization.CultureInfo.InvariantCulture;
        }

        private void FillValidFileTable(DataTable fileTable)
        {
            fileTable.Clear();
            foreach (DBFiles.DBFile file in DBFilesList.ValidFileList)
            {
                AddValidFileTable(fileTable, file);
            }
        }

        private void AddValidFileTable(DataTable fileTable, DBFiles.DBFile file)
        {
            DataRow nextRow = fileTable.NewRow();
            nextRow["File"] = file.name;
            nextRow["Date"] = file.timestamp;
            nextRow["Size"] = file.size;
            fileTable.Rows.Add(nextRow);
        }

        private void buttonRoot_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = ((App)Application.Current).QDChatReaderData.RootFolder;
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ((App)Application.Current).QDChatReaderData.RootFolder = folderBrowserDialog.SelectedPath;
            }
        }

        private void buttonSeek_Click(object sender, RoutedEventArgs e)
        {
            if (bgSeeker.IsBusy != true)
            {
                validFileTable.Clear();
                bgSeeker.RunWorkerAsync();
            }
        }

        private void buttonRootReset_Click(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).QDChatReaderData.SetDefaultRootFolder();
        }

        private void buttonSaveAs_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonCancelSeek_Click(object sender, RoutedEventArgs e)
        {
            bgSeekerCancel();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            string fileName = ((App)Application.Current).QDChatReaderData.SelectedDBFile;
            if (DBFilesList.GetValidFileByName(fileName) == 0)
            {
                bgSeekerCancel();
                ((App)Application.Current).QDChatReaderData.ActiveDBFile = DBFilesList.SelectedFile.name;
                Console.WriteLine("Datei benutzt: " + DBFilesList.SelectedFile.name);
                this.Close();
            }
        }

        private void gridViewFileList_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                int selectedRow = gridViewFileList.Items.IndexOf(gridViewFileList.CurrentItem);
                DataRowView row = (DataRowView)gridViewFileList.CurrentItem;
                string fileName = row["File"].ToString();
                if (DBFilesList.GetValidFileByName(fileName) == 0)
                {
                    ((App)Application.Current).QDChatReaderData.SelectedDBFile = DBFilesList.SelectedFile.name;
                    Console.WriteLine("Datei gewählt: " + DBFilesList.SelectedFile.name);
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

    }


}
