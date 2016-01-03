using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDChatReader
{
    public class DBFiles
    {
        public List<DBFile> FileList = new List<DBFile>();
        public List<DBFile> ValidFileList = new List<DBFile>();
        public DBFile SelectedFile = new DBFile();

        public class DBFile
        {
            public string name = "";
            public DateTime timestamp = new DateTime();
            public long size = 0;
        }

        public void SeekAll(string rootPath)
        {
            Console.WriteLine("start seeking...");
            FileList.Clear();
            WalkThruFileSystem(rootPath, FileList);
            Console.WriteLine("end seekin. found: " + FileList.Count);
            Console.WriteLine("start sorting...");
            FileList.Sort(new DBFileComparer());
            Console.WriteLine("end sorting");
        }

        public void ValidateAll()
        {
            Console.WriteLine("start validating...");
            foreach (DBFile file in FileList)
            {
                if (FileValidate(file.name))
                {
                    ValidFileList.Add(file);
                }
            }
            Console.WriteLine("end validating found: " + ValidFileList.Count);
        }


        /// <summary>
        /// http://www.mycsharp.de/wbb2/thread.php?threadid=58665
        /// </summary>
        /// <param name="strDir">root folder for recursive search</param>
        public static void WalkThruFileSystem(string strDir, List<DBFile> list)
        {
            // 0. Einstieg in die Rekursion auf oberster Ebene
            WalkThruFileSystem(new DirectoryInfo(strDir), list);
        }

        private static void WalkThruFileSystem(DirectoryInfo di, List<DBFile> list)
        {
            try
            {
                // 1. Für alle Dateien im aktuellen Verzeichnis
                foreach (FileInfo fi in di.GetFiles())
                {
                    // 1a. Statt Console.WriteLine hier die gewünschte Aktion
                    //Console.WriteLine("file: " + fi.FullName);
                    DBFile newfile = new DBFile();
                    newfile.name = fi.FullName;
                    newfile.timestamp = fi.LastWriteTime;
                    newfile.size = fi.Length;
                    list.Add(newfile);
                }

                // 2. Für alle Unterverzeichnisse im aktuellen Verzeichnis
                foreach (DirectoryInfo diSub in di.GetDirectories())
                {
                    // 2a. Statt Console.WriteLine hier die gewünschte Aktion
                    //Console.WriteLine("dir :" + diSub.FullName);

                    // 2b. Rekursiver Abstieg
                    WalkThruFileSystem(diSub, list);
                }
            }
            catch (Exception)
            {
                // 3. Statt Console.WriteLine hier die gewünschte Aktion
            }
        }

        public class DBFileComparer : IComparer<DBFile>
        {
            public int Compare(DBFile file2, DBFile file1)
            {
                var property1 = file1.timestamp;
                var Property2 = file2.timestamp;
                if (property1 > Property2)
                { return 1; }
                else if (property1 < Property2)
                { return -1; }
                else { return 0; }
            }
        }

        public int GetValidFileByName(string name)
        {
            int retvalue = -1;
            int index = ValidFileList.FindIndex(x => x.name == name);
            if (index >= 0)
            {
                SelectedFile = ValidFileList[index];
                retvalue = 0;
            }
            return retvalue;
        }

        public bool FileValidate(string filename)
        {
            bool retvalue = false;
            try
            {
                if (ValidateHeader(filename))
                {
                    if (ValidateContent(filename))
                        retvalue = true;
                }

            }
            catch (IOException)
            {
                //irgend nen Fehler passiert
            }
            return retvalue;
        }

        private static bool ValidateHeader(string filename)
        {
            bool retvalue = false;
            // Prüfen ob die Datei existiert
            var fileInfo = new FileInfo(filename);
            if (fileInfo.Exists)
            {
                //Prüfe den Header erste 15 Byte
                // Datei in einen FileStream laden
                var fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read);
                // Buffergröße festlegen
                var buffer = new byte[15];
                // Lesen der ersten 15 Byte. Prüfe, ob lang genug
                if (fileStream.Read(buffer, 0, buffer.Length) == 15)
                {
                    //Prüfe auf sqlite header
                    if (System.Text.Encoding.ASCII.GetString(buffer) == "SQLite format 3")
                    {
                        retvalue = true;
                    }
                }
                // Stream schließen
                fileStream.Close();
            }
            else
            {
                // nicht gefunden Console.WriteLine("Die Datei demo.txt wurde nicht gefunden.");
            }
            return retvalue;
        }

        private static bool ValidateContent(string filename)
        {
            bool retvalue = false;
            sqlite database = new sqlite();
            database.Open(filename);
            if (database.CheckStructure())
            {
                retvalue = true;
            }
            database.CLose();
            return retvalue;
        }

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




}
