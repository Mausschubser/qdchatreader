using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace QDChatReader
{
    class QDSerializer
    {
        private object theObject;
        private string shortFileName;
        private string fullFileName;
        
        #region Getters and Setters
        public object TheObject
        {
            get { return theObject; }
            set { theObject = value; }
        }
        public string FileName
        {
            get { return shortFileName; }
            set {
                    shortFileName = value;
                    string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string targetFolder = Path.Combine(appDataFolder, "QDChat");
                    if (!Directory.Exists(targetFolder))
                    {
                        Directory.CreateDirectory(targetFolder);
                    }
                    fullFileName = Path.Combine(targetFolder, shortFileName);
                }
        }
        public string FullFileName
        {
            get { return fullFileName; }
        }
        #endregion

        public QDSerializer(object theNewObject, string theNewFileName)
        {
            FileName = theNewFileName;
            TheObject = theNewObject;
        }

        public void SerializeToXML()
        {
            try
            {
                //Erstelle einen XML-Serialisierer für Objekte vom Typ Blog
                XmlSerializer serializer = new XmlSerializer(theObject.GetType());

                //Erstelle einen FileStream auf die Datei, in die unserer
                //Blog-Objekt in XML-Form gespeichert werden soll.
                FileStream file = new FileStream(fullFileName, FileMode.Create);
                //Serialisiere das übergebene Objekt (toBeSerialized)
                //und schreibe es in den FileStream.
                serializer.Serialize(file, theObject);

                //Schließe die XML-Datei.
                file.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("Serialisierung fehlgeschlagen");

            }
        }

        public object DeserializeFromXML()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(theObject.GetType());
                FileStream file = new FileStream(fullFileName, FileMode.Open);
                //Die Deserialize()-Methode gibt ein Object zurück. => casten!
                theObject = serializer.Deserialize(file);
                file.Close();

            }
            catch (Exception)
            {
                Console.WriteLine("Deserialisierung fehlgeschlagen");

                //                throw;
            }
            return TheObject;
        }

    }
}
