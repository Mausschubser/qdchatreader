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
        private QDChatReaderClass qdChatReader = new QDChatReaderClass();

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
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
