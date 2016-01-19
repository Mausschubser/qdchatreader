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
            string sql = "select ZSENTDATE, ZTO, ZFROM, ZTEXT from ZQFMESSAGE";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            try
            {
                SQLiteDataReader reader = command.ExecuteReader();
                Console.WriteLine("start");
                qdchatlist.Clear();
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
            catch (Exception)
            {
                qdchatlist.Clear();
            }
            return -1;
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


}
