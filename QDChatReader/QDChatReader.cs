using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDChatReader
{
    public class QDChatReaderClass : INotifyPropertyChanged
    {
        private string personSelected;
        private string rootFolder;
        private string selectedDBFile;
        private string activeDBFile;

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
            SetDefaultRootFolder();
        }

        public void SetDefaultRootFolder()
        {
            string fileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            fileName = fileName + "\\Apple Computer\\MobileSync";
            fileName = "E:\\Temp\\Test";
            fileName = "E:\\testQDchat";
            RootFolder=fileName;
        }
    }
}
