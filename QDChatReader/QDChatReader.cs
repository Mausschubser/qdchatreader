using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDChatReader
{
    public class QDChatReaderClass : INotifyPropertyChanged
    {
        private string personSelected="";
        private string rootFolder="";
        private string selectedDBFile="";
        private string activeDBFile="";
        private string exportFolder="";
        private string myName="";
        private string culture = "";
        private double displaySizeInch = 4.0;

        #region Properties Getters and Setters
        public string PersonSelected
        {
            get{return personSelected;}
            set{personSelected = value; OnPropertyChanged("PersonSelected"); }
        }

        public string RootFolder
        {
            get { return rootFolder; }
            set { rootFolder = value; OnPropertyChanged("RootFolder"); }
        }

        public string ExportFolder
        {
            get { return exportFolder; }
            set { exportFolder = value; OnPropertyChanged("ExportFolder"); }
        }

        public string SelectedDBFile
        {
            get { return selectedDBFile; }
            set { selectedDBFile = value; OnPropertyChanged("SelectedDBFile"); }
        }

        public string ActiveDBFile
        {
            get { return activeDBFile; }
            set { activeDBFile = value; OnPropertyChanged("ActiveDBFile"); }
        }

        public string MyName
        {
            get { return myName; }
            set { myName = value; OnPropertyChanged("MyName"); }
        }

        public string Culture
        {
            get { return culture; }
            set { culture = value; OnPropertyChanged("Culture"); }
        }

        public double DisplaySizeInch
        {
            get { return displaySizeInch;}
            set
            {
                displaySizeInch = value>0.0?value:displaySizeInch;
                ChatLengthConverter.DisplaySizeInch = displaySizeInch;
            }
        }

        #endregion

        #region Eventhandler
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public void Init()
        {
            InitDefaultRootFolder();
            InitDefaulLinesToLength();
            SetDefaultExportFolder();
            SetDefaultMyName();
            SetDefaultSelectedDBFile();
        }

        public void SetDefaultRootFolder()
        {
            string fileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            fileName = fileName + "\\Apple Computer\\MobileSync";
            RootFolder = fileName;
        }

        public void InitDefaultRootFolder()
        {
            if (!Directory.Exists(RootFolder))
            {
                SetDefaultRootFolder();
            }
        }

        public void InitDefaulLinesToLength()
        {
            if (DisplaySizeInch<=0.0)
            { DisplaySizeInch = 4.0; }
        }

        public void SetDefaultExportFolder()
        {
            if (!Directory.Exists(ExportFolder))
            {
                ExportFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
        }

        public void SetDefaultMyName()
        {
            if(MyName=="")
            {
                MyName = "me";
            }
        }

        public void SetDefaultSelectedDBFile()
        {
            SelectedDBFile = "";
        }
    }
}
