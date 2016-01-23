using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QDChatReader
{
    /// <summary>
    /// Interaction logic for QDChatWindow.xaml
    /// </summary>
    public partial class QDChatWindow : Window
    {
        public QDChatDataTable ChatTable = new QDChatDataTable();

        #region Hide Window Buttons
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private void HideWindowButtons()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }
        #endregion

        public QDChatWindow()
        {
            InitializeComponent();
            this.DataContext = ((App)Application.Current).QDChatReaderData;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            chatGridView.ItemsSource = ChatTable.AsDataView();
            HideWindowButtons();
        }
    }
}
