using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace QDChatReader
{
    public class QDChatDB
    {
        private string _dbfilename;
        public string dbfilename
        {
            get { return _dbfilename; }
            set { _dbfilename = value; }
        }
        private int _entries = 0;
        private SQLiteConnection m_dbConnection;
        public int entries { get { return _entries; } }

        public int Open()
        {
            try
            {
                if (m_dbConnection!= null)
                {
                    if(m_dbConnection.State.ToString() == "Open")
                    {
                        m_dbConnection.Close();
                    }
                }
                m_dbConnection = new SQLiteConnection("Data Source=" + this.dbfilename + ";Version=3;");
                m_dbConnection.Open();
                return 0;
            }
            catch (Exception)
            {
                //throw;
            }
            return -1;
        }

        public int ReadChat(List<QDChatLine> qdchatlist)
        {
            qdchatlist.Clear();
            string sql = "select ZSENTDATE, ZTO, ZFROM, ZTEXT from ZQFMESSAGE";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            Console.WriteLine("start");
            this._entries = 0;
            while (reader.Read())
            {
                QDChatLine chatentry = new QDChatLine();
                chatentry.senderid = reader["ZFROM"].ToString();
                chatentry.receiverid = reader["ZTO"].ToString();
                chatentry.chattext = reader["ZTEXT"].ToString();
                chatentry.timeoffset = reader.GetDouble(0);
                chatentry.ConvertRawField();
                qdchatlist.Add(chatentry);
                this._entries++;

                //               Console.WriteLine(" " + _entries + "date:" + chatentry.timestamp + "\tfrom: " + chatentry.sendername + "\t" + chatentry.chattext);
            }
            Console.WriteLine("sortieren anfang");
            qdchatlist.Sort(new QDChatComparer());
            Console.WriteLine("sortieren fertig");
            return 0;
        }

        public void Close()
        {
            try
            {
                if (m_dbConnection!=null)
                {
                    m_dbConnection.Close();
                }
            }
            catch (Exception)
            {

                throw;
            }
     
        }
    }

    public class QDChatPersons
    {
        public QDChatPerson Me = new QDChatPerson();
        public QDChatPerson Selected = new QDChatPerson();
        public List<QDChatPerson> List = new List<QDChatPerson>();

        public bool isMe(QDChatPerson person)
        {
            return (person.id == Me.id);
        }

        public int ReadFromChatList(List<QDChatLine> qdchatlist)
        {
            ResetCounter();
            foreach (QDChatLine chatline in qdchatlist)
            {
                AddPersonToList(chatline.senderid, chatline);
                AddPersonToList(chatline.receiverid, chatline);
            }
            List.Sort(new QDPersonsCountComparer());
            List[0].isMe = true;
            Me = List[0];
            if ((Selected.id == "") && (List.Count > 0))
            {
                Selected = List[1];
            }
            return 0;
        }

        private void ResetCounter()
        {
            foreach (QDChatPerson person in List)
            {
                person.count = 0;
            }
        }

        private int AddPersonToList(string personid, QDChatLine chatline)
        {
            int personindex = -1;
            personindex = List.FindIndex(x => personid == x.id);
            if (personindex < 0)       //new name ID found
            {
                QDChatPerson newperson = new QDChatPerson();
                newperson.id = personid;
                newperson.name = personid;
                newperson.count = 1;
                newperson.firstAppearance = chatline.timestamp;
                List.Add(newperson);
            }
            else
            {
                List[personindex].count++;
            }
            return personindex;
        }

        public class QDPersonsCountComparer : IComparer<QDChatPerson>
        {
            public int Compare(QDChatPerson person2, QDChatPerson person1)
            {
                int personcount1 = person1.count;
                int personcount2 = person2.count;
                if (personcount1 > personcount2)
                {
                    return 1;
                }
                else if (personcount1 < personcount2)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }

        public void SerializeToXML()
        {
            try {
                //Erstelle einen XML-Serialisierer für Objekte vom Typ Blog
                XmlSerializer serializer = new XmlSerializer(typeof(QDChatPersons));

                //Erstelle einen FileStream auf die Datei, in die unserer
                //Blog-Objekt in XML-Form gespeichert werden soll.
                FileStream file = new FileStream("E:\\persons.xml", FileMode.Create);
                //Serialisiere das übergebene Objekt (blogObj)
                //und schreibe es in den FileStream.
                serializer.Serialize(file, this);

                //Schließe die XML-Datei.
                file.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("Serialisierung fehlgeschlagen");

            }
        }

        public QDChatPersons DeserializeFromXML()
        {
            QDChatPersons qdpersons = this;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(QDChatPersons));
                FileStream file = new FileStream("E:\\persons.xml", FileMode.Open);
                //Die Deserialize()-Methode gibt ein Object zurück. => casten!
                qdpersons = serializer.Deserialize(file) as QDChatPersons;
                file.Close();

            }
            catch (Exception)
            {
                Console.WriteLine("Deserialisierung fehlgeschlagen");

                //                throw;
            }
            return qdpersons;
        }

    }

    public class QDChatPerson
    {
        public string id = "";
        public string name = "";
        public int count = 0;
        public DateTime firstAppearance;
        public bool isMe = false;
    }
}
