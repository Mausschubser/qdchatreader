using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace QDChatReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static QDChatReaderClass qdChatReader = new QDChatReaderClass();
        private QDSerializer QDChatReaderSerializer = new QDSerializer(qdChatReader, "QDChatReader.xml");
        private string version;

        #region Getters and Setters
        public QDChatReaderClass QDChatReaderData
        {
            get { return qdChatReader; }
            set { qdChatReader = value; }
        }

        public string Version
        {
            get { return version; }
            set { version = value; }
        }
        #endregion

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            System.Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Version=String.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            qdChatReader.Init();
            qdChatReader = QDChatReaderSerializer.DeserializeFromXML() as QDChatReaderClass;
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                QDChatReaderSerializer.SerializeToXML();
            }
            finally
            {
                base.OnExit(e);
            }
        }
    }
}
