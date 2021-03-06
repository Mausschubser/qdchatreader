﻿using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Windows;

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
            toolStripStatusLabel.Dispatcher.Invoke(new UpdateStatusLabelDelegate(UpdateStatusLabel), new object[] { QDChatReader.Properties.Resources.StatusBarCollectingFiles });
            DBFilesList.SeekAll(((App)Application.Current).QDChatReaderData.RootFolder);
            //Console.WriteLine("start validating...");
            int totalfilescount = DBFilesList.FileList.Count;
            int filescounted = 0;
            int percentage = 0;
            string statustext = "";
            DateTime timestamp = DateTime.Now; ;
            foreach (DBFile file in DBFilesList.FileList)
            {
                if ((worker.CancellationPending == true))
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    if (file.IsValidChatDB(file.name))
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
            toolStripStatusLabel.Dispatcher.Invoke(new UpdateStatusLabelDelegate(UpdateStatusLabel), new object[] { QDChatReader.Properties.Resources.StatusBarDone});
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
            foreach (DBFile file in DBFilesList.ValidFileList)
            {
                AddValidFileTable(fileTable, file);
            }
        }

        private void AddValidFileTable(DataTable fileTable, DBFile file)
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
            folderBrowserDialog.ShowNewFolderButton = false;
            folderBrowserDialog.Description = QDChatReader.Properties.Resources.DlgSearchITunes;
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
            string newfilename;
            string sourcefile = ((App)Application.Current).QDChatReaderData.SelectedDBFile;
            
            if (File.Exists(sourcefile))
            {
                FileInfo info = new FileInfo(sourcefile);
                DateTime timestamp = info.CreationTime;
                string timestring = timestamp.ToString("yyyy-MM-dd_HH-mm");
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = QDChatReader.Properties.Resources.saveDBDialogFilter;  //"database files (*.db)|*.db|all files (*.*)|*.*";
                saveFileDialog.DefaultExt = "*.db";
                saveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(((App)Application.Current).QDChatReaderData.ActiveDBFile);
                saveFileDialog.FileName = "QD-Chat_"+timestring+".db";
                saveFileDialog.ValidateNames = false;
                if (saveFileDialog.ShowDialog() == true)
                {
                    newfilename = saveFileDialog.FileName;
                    try
                    {
                        File.Copy(sourcefile, newfilename,true);
                        if (File.Exists(newfilename))
                        {
                            ((App)Application.Current).QDChatReaderData.ActiveDBFile = newfilename;
                            this.Close();
                        }
                    }
                    catch { }
                }
            }
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
                // this.Close();
            }
        }

        private void gridViewFileList_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                int selectedRow = gridViewFileList.Items.IndexOf(gridViewFileList.CurrentItem);
                if (gridViewFileList.CurrentItem!=null)
                {
                    DataRowView row = (DataRowView)gridViewFileList.CurrentItem;
                    string fileName = row["File"].ToString();
                    if (DBFilesList.GetValidFileByName(fileName) == 0)
                    {
                        ((App)Application.Current).QDChatReaderData.SelectedDBFile = DBFilesList.SelectedFile.name;
                        Console.WriteLine("Datei gewählt: " + DBFilesList.SelectedFile.name);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }


}
