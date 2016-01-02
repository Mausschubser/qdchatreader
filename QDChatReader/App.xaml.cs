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

        #region Getters and Setters
        public QDChatReaderClass QDChatReaderData
        {
            get { return qdChatReader; }
            set { qdChatReader = value; }
        }
        #endregion

        private void Application_Startup(object sender, StartupEventArgs e)
        {
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
