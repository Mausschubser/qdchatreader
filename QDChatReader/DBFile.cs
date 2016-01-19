using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDChatReader
{
    public class DBFile
    {
        public string name = "";
        public DateTime timestamp = new DateTime();
        public long size = 0;


        public bool IsValidChatDB(string filename)
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

    }


}
