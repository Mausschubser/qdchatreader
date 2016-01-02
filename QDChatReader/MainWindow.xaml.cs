using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QDChatReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        QDChatDB qddb = new QDChatDB();
        List<QDChatLine> QDChat = new List<QDChatLine>();
        static QDChatPersons QDPersons = new QDChatPersons();
        DataTable PersonTable = new DataTable();
        DataTable ChatTable = new DataTable();
        QDSerializer personSerializer = new QDSerializer(QDPersons, "persons2.xml");
        private string selectedNameID= "";

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = ((App)Application.Current).QDChatReaderData;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitPersonTable(PersonTable);
            QDPersons= personSerializer.DeserializeFromXML() as QDChatPersons;
            FillPersonTable(PersonTable);
            personGridView.ItemsSource = PersonTable.AsDataView();
            InitChatTable(ChatTable);
            chatGridView.ItemsSource = ChatTable.AsDataView();
        }

        private void UpdateChatTableFromNewRow(string newID)
        {
            if (newID != selectedNameID)
            {
                int personindex = QDPersons.List.FindIndex(x => x.id == newID);
                if (personindex >= 0)
                {
                    QDPersons.Selected = QDPersons.List[personindex];
                    ((App)Application.Current).QDChatReaderData.PersonSelected = QDPersons.Selected.name;
                    FillChatTable(ChatTable);
                }
                selectedNameID = newID;
            }
        }


        private void InitChatTable(DataTable chatTable)
        {
            chatTable.Columns.Add("Date");
            chatTable.Columns.Add("Direction");
            chatTable.Columns.Add("Chat"); 
            chatTable.Locale = System.Globalization.CultureInfo.InvariantCulture;
        }

        private void FillChatTable(DataTable chatTable)
        {
            chatTable.Clear();
            foreach (QDChatLine chatline in QDChat)
            {
                if (chatline.receiverid == QDPersons.Selected.id || chatline.senderid == QDPersons.Selected.id)
                {
                    DataRow workRow = chatTable.NewRow();
                    workRow["Date"] = chatline.timestamp.ToString();
                    workRow["Direction"] = ((chatline.chatdirection == 1) ? "<" : ">");
                    workRow["Chat"] = chatline.chattext;
                    chatTable.Rows.Add(workRow);
                }
            }
        }

        private void InitPersonTable(DataTable personTable)
        {
            personTable.Columns.Add("ID");
            personTable.Columns.Add("Name");
            personTable.Columns.Add("1stContact", System.Type.GetType("System.DateTime"));
            personTable.Columns.Add("Count", System.Type.GetType("System.Int32"));
            personTable.Locale = System.Globalization.CultureInfo.InvariantCulture;
        }

        private void FillPersonTable(DataTable personTable)
        {
            personTable.Clear();
            foreach (QDChatPerson person in QDPersons.List)
            {
                DataRow personRow = personTable.NewRow();
                if (!person.isMe)
                {
                    personRow["ID"] = person.id;
                    personRow["Name"] = person.name;
                    personRow["1stContact"] = person.firstAppearance;
                    personRow["Count"] = person.count;
                    personTable.Rows.Add(personRow);
                }
            }
        }

        private void UpdateDirections()
        {
            string me = QDPersons.Me.id;
            foreach (QDChatLine chatline in QDChat)
            {
                if (chatline.senderid == me)
                {
                    chatline.chatdirection = QDChatLine.OUT;
                }
            }
        }

        private void loadDbButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDBFileDialog = new OpenFileDialog();
            openDBFileDialog.Filter = "all (*.*)|*.*|DBs (*.db)|*.db";
            openDBFileDialog.InitialDirectory = ((App)Application.Current).QDChatReaderData.ActiveDBFile;
            if (openDBFileDialog.ShowDialog() == true)
            {
                string dbFileName = openDBFileDialog.FileName;
                ((App)Application.Current).QDChatReaderData.ActiveDBFile = dbFileName;
            }
        }

        private void fileNameLabel_TextChanged(object sender, TextChangedEventArgs e)
        {
            string dbFileName = fileNameLabel.Text;
            ReadDBFileAndUpdateAllTables(dbFileName);
        }


        private void ReadDBFileAndUpdateAllTables(string dbFileName)
        {
            Console.WriteLine("Update Chats from new file: "+dbFileName);
            ReadDBFile(dbFileName);
            UpdatePersons();
            ((App)Application.Current).QDChatReaderData.PersonSelected = QDPersons.Selected.name;
            UpdateDirections();
            FillChatTable(ChatTable);
        }

        private void ReadDBFile(string filename)
        {
            qddb.dbfilename = filename;
            qddb.Open();
            qddb.ReadChat(QDChat);
        }

        private void UpdatePersons()
        {
            QDPersons.ReadFromChatList(QDChat);
            FillPersonTable(PersonTable);
            //QDPersons.SerializeToXML();
            personSerializer.SerializeToXML();

        }

        private void personGridView_GotFocus(object sender, RoutedEventArgs e)
        {
            int selectedRow  = personGridView.Items.IndexOf(personGridView.CurrentItem);
            DataRowView row = (DataRowView)personGridView.CurrentItem;
            if (row != null)
            {
                string id = row["ID"].ToString();
                string name = row["Name"].ToString();
                ((App)Application.Current).QDChatReaderData.PersonSelected = name;
                Console.WriteLine("Name gewählt:" + id+"="+name);
                UpdateChatTableFromNewRow(id);
            }
        }

        private void personGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void personGridView_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var editedTextbox = e.EditingElement as TextBox;
            string colheader = personGridView.CurrentColumn.Header.ToString();
            DataRowView row = (DataRowView)personGridView.SelectedItems[0];
            if (row != null && editedTextbox != null && (colheader == "Name")) //Eingabe in ein mögliches Namensfeld
            {
                string id = row["ID"].ToString();
                int personindex = QDPersons.List.FindIndex(x => x.id == id);
                if (personindex >= 0)
                {
                    string name = editedTextbox.Text;
                    QDPersons.List[personindex].name = name; ;
                    personSerializer.SerializeToXML();
                    QDPersons.Selected = QDPersons.List[personindex];
                    ((App)Application.Current).QDChatReaderData.PersonSelected = QDPersons.Selected.name;
                    FillChatTable(ChatTable);
                }
            }
        }

        private void seekButton_Click(object sender, RoutedEventArgs e)
        {
            DBSeekerWindow dbSeekerDlg = new DBSeekerWindow();
            dbSeekerDlg.ShowDialog();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {

        }

 
    }
}
