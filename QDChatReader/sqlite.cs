using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDChatReader
{
    public class sqlite
    {
        private SQLiteConnection m_dbConnection;

        public int Open(string dbfilename)
        {
            m_dbConnection = new SQLiteConnection("Data Source=" + dbfilename + ";Version=3;");
            m_dbConnection.Open();
            return 0;
        }

        public void CLose()
        {
            m_dbConnection.Close();
        }

        public bool CheckStructure()
        {
            bool retvalue = false;
            string sql;
            //Prüfe, ob eine Tabelle mit Namen ZQFMESSAGE vorliegt
            sql = "select name from sqlite_master WHERE TYPE='table' AND tbl_name = 'ZQFMESSAGE'";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            if (reader.HasRows) //ZQFMESSAGE liegt vor --> hasRows=true
            {
                //Prüfe, ob die wichtigsten Spalten vorhanden sind, anhand der Spaltennamen
                sql = "select * from ZQFMESSAGE LIMIT 1";  //hole eine Zeile aus der Tabelle
                SQLiteCommand command2 = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader2 = command2.ExecuteReader();
                List<string> columnNames = new List<string>() { "ZFROM", "ZTO", "ZTEXT", "ZSENTDATE" }; // Liste der notwendigen Spalten
                if (reader2.HasRows && reader2.FieldCount > 0)
                {
                    bool someNameIsMissing = false;
                    foreach (string columnName in columnNames)    //prüfe alle geforderten Spaltennamen
                    {
                        bool columnNameIsMissing = true;
                        for (int i = 0; i < reader2.FieldCount; i++)    //prüfe ob der Name in irgend einer Spalet steht.
                        {
                            if (reader2.GetName(i) == columnName)
                            { columnNameIsMissing = false; }
                        }
                        if (columnNameIsMissing)
                        { someNameIsMissing = true; }
                    }
                    if (!someNameIsMissing)     //nix fehlt, alle Namen tauchten in der Abfrage auf...
                    {
                        retvalue = true;
                    }
                }
                Console.WriteLine("ZQFMESSAGE gefunden");
            }

            return retvalue;
        }
    }

}
