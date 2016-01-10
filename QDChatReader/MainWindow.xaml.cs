using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        QDChatList QDChat = new QDChatList();
        static QDChatPersons QDPersons = new QDChatPersons();
        QDPersonDataTable PersonTable = new QDPersonDataTable();
        QDChatDataTable ChatTable = new QDChatDataTable();
        QDSerializer personSerializer = new QDSerializer(QDPersons, "QDPersons.xml");
        private string selectedNameID= "";

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = ((App)Application.Current).QDChatReaderData;
            ((App)Application.Current).QDChatReaderData.PropertyChanged += QDChatReaderData_Changed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            personGridView.ItemsSource = PersonTable.AsDataView();
            chatGridView.ItemsSource = ChatTable.AsDataView();
            labelVersion.Content = ((App)Application.Current).Version;
        }

        private void UpdateChatTableFromNewRow(string newID)
        {
            if (newID != selectedNameID)
            {
                int personindex = QDPersons.GetPersonIndexFromId(newID);
                if (personindex >= 0)
                {
                    QDPersons.SetSelectedPerson(personindex);
                    ((App)Application.Current).QDChatReaderData.PersonSelected = QDPersons.Selected.name;
                    ChatTable.FillFromChatByPerson(QDChat, QDPersons);
                }
                selectedNameID = newID;
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
            openDBFileDialog.Filter = "DataBase files (*.db)|*.db|all files (*.*)|*.*";
            openDBFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(((App)Application.Current).QDChatReaderData.ActiveDBFile);
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
            ChatTable.FillFromChatByPerson(QDChat, QDPersons);
        }

        private void ReadDBFile(string filename)
        {
            qddb.dbfilename = filename;
            qddb.Open();
            qddb.ReadChat(QDChat);
        }

        private void UpdatePersons()
        {
            if (QDPersons.List.Count==0)
            {
                QDPersons = personSerializer.DeserializeFromXML() as QDChatPersons;
            }
            QDPersons.ReadFromChatList(QDChat);
            PersonTable.Fill(QDPersons);
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
                int personindex = QDPersons.GetPersonIndexFromId(id);
                if (personindex!=QDChatPersons.INDEXNOTFOUND)
                {
                    QDPersons.SetSelectedPerson(personindex);
                    ((App)Application.Current).QDChatReaderData.PersonSelected = name;
                    Console.WriteLine("Name gewählt:" + id + "=" + name);
                    UpdateChatTableFromNewRow(id);
                }
            }
        }

        private void personGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private QDChatPerson ChangePersonName(string id, string name)
        {
            QDChatPerson foundPerson = QDPersons.ChangePersonName(id, name);
            if (foundPerson.id!="")
            {
                personSerializer.SerializeToXML();
            }
            return foundPerson;
        }

        private void seekButton_Click(object sender, RoutedEventArgs e)
        {
            DBSeekerWindow dbSeekerDlg = new DBSeekerWindow();
            dbSeekerDlg.ShowDialog();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if(ChatTable.Rows.Count>0)
            {
                string newfilename;
                string chatpartner = ((App)Application.Current).QDChatReaderData.PersonSelected;
                string allowedchars = "a-zA-Z0-9äÄöÖüÜß +&";//allowed characters for the filename derived from chat partner's name
                Regex r = new Regex("[^"+allowedchars+"]"); //regular expression to exclude these characters form beeing removed
                chatpartner = r.Replace(chatpartner,"");    //remove all characters NOT listed above
                Console.WriteLine("name="+chatpartner);
                chatpartner =chatpartner.Trim(System.IO.Path.GetInvalidFileNameChars());
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "text (*.txt)|*.txt|Excel (*.xlsx)|*.xlsx|all files (*.*)|*.*";
                saveFileDialog.DefaultExt = "*.txt";
                saveFileDialog.InitialDirectory = ((App)Application.Current).QDChatReaderData.ExportFolder;
                saveFileDialog.FileName = "QDChat "+chatpartner+".txt";
                //saveFileDialog.ValidateNames = false;
                //saveFileDialog.CheckFileExists = true;
                //saveFileDialog.CreatePrompt = true;
                saveFileDialog.OverwritePrompt = true;
                if (saveFileDialog.ShowDialog() == true)
                {
                    newfilename = saveFileDialog.FileName;
                    try
                    {
                        QDChat.WriteChatOfPerson(newfilename,QDPersons.Selected);
                        ((App)Application.Current).QDChatReaderData.ExportFolder=System.IO.Path.GetDirectoryName(newfilename);
                    }
                    catch
                    {
                    }
                }
            }
        }
        
        void QDChatReaderData_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName=="MyName")   //Anwender hat einen neuen Namen eingegeben
            {
                string myNewName = ((App)Application.Current).QDChatReaderData.MyName;
                string id = QDPersons.Me.id;
                QDPersons.Me.name = myNewName;
                ChangePersonName(id, myNewName);
                Console.WriteLine("mein Name geändert: " + myNewName);
            }

            if (e.PropertyName=="PersonSelected") //Anwender will evtl.Namen der Chatperson verändert
            {
                string oldname = QDPersons.Selected.name;
                string newName = ((App)Application.Current).QDChatReaderData.PersonSelected;
                if (newName!= oldname)  //erkenne, dass der Name der Selektierten Person geändert wird
                {
                    string id = QDPersons.Selected.id;  //die id der bisher selektierten Person
                    ChangePersonName(id, newName);    //bekommt den neuen Namen
                    UpdatePersons();
                    Console.WriteLine("Name des Chat-Partners ist geändert worden: " +oldname + " --> "+ newName);
                }
            }
        }
    }
}
