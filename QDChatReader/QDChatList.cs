using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDChatReader
{
    public class QDChatList:List<QDChatLine>
    {
        //
        public void WriteChatOfPerson(string filename, QDChatPerson qdperson)
        {
            string filetype = System.IO.Path.GetExtension(filename);
            if (filetype == "xlsx")
            {
                //Storage as Excel file
            }
            else
            {
                WriteTxtFile(filename, qdperson);
            }
        }


        private void WriteTxtFile(string targetfile,QDChatPerson qdperson)
        {
            FileStream file = null;
            string tempfile = Path.GetTempPath() + "qdchat.txt"; // GetTempFileName();
            try
            {
                file = new FileStream(tempfile, FileMode.Create);
                using (StreamWriter writer = new StreamWriter(file))
                {
                    foreach (QDChatLine chatline in this)
                    {
                        if (chatline.receiverid == qdperson.id || chatline.senderid == qdperson.id)
                        {
                            string workRow = "";
                            workRow += chatline.timestamp.ToString();
                            workRow += "\t"+((chatline.chatdirection == 1) ? "me:" : qdperson.name+":");
                            workRow += "\t"+chatline.chattext;
                            writer.WriteLine(workRow);
                        }
                    }
                }

            }
            finally
            {
                if (file != null) file.Dispose();
            }
            try
            {
                if (File.Exists(tempfile))
                {
                    File.Copy(tempfile, targetfile, true);
                    if (File.Exists(targetfile))
                    {
                        File.Delete(tempfile);
                        System.Diagnostics.Process.Start(targetfile);
                    }
                }
            }
            catch 
            {
                //no success
            }
        }
    }
}
