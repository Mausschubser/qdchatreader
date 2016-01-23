using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
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
            qdChatReader = QDChatReaderSerializer.DeserializeFromXML() as QDChatReaderClass;
            qdChatReader.Init();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        #region Localization
        //Chage language (culture) of the Application during runtime
        //needs closing all windows and re-opening the main window
        //found in: WPF Loclisation Guidance
        //http://wpflocalization.codeplex.com/releases/view/29389

        public static void SetCulture(string culture, bool closeAllWindowsReloadMain)
        {
            if (culture == null)
                return;
            bool cultureChanged = culture != Thread.CurrentThread.CurrentUICulture.IetfLanguageTag;
            if (!cultureChanged)
                return;
            if (!string.IsNullOrEmpty(culture))
            {
                CultureInfo ci = new CultureInfo(culture);
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
                qdChatReader.Culture = ci.ToString();
            }

            if (closeAllWindowsReloadMain)
            {
                //                Type mainWinType = App.Current.Windows[0].GetType();
                //                Window mainForm = Assembly.GetExecutingAssembly().CreateInstance(mainWinType.FullName) as Window;
                Window mainForm = Assembly.GetExecutingAssembly().CreateInstance("QDChatReader.MainWindow") as Window; 
                // close all other windows
                foreach (Window win in App.Current.Windows)
                {
                    if (mainForm == win) continue;
                    win.Hide();
                }
                mainForm.Show();
            }
        }

        #endregion

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
