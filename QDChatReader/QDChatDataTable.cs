using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDChatReader
{
    public class QDChatDataTable : DataTable
    {
        public void Init()
        {
            Columns.Add("Date");
            Columns.Add("Direction");
            Columns.Add("Chat");
            Locale = System.Globalization.CultureInfo.InvariantCulture;
        }

        public void FillFromChatByPerson(QDChatList qdchat, QDChatPersons qdpersons)
        {
            if (this.Columns.Count < 1)
            {
                this.Init();
            }
            this.Clear();
            foreach (QDChatLine chatline in qdchat)
            {
                if (chatline.receiverid == qdpersons.Selected.id || chatline.senderid == qdpersons.Selected.id)
                {
                    DataRow workRow = this.NewRow();
                    workRow["Date"] = chatline.timestamp.ToString();
                    workRow["Direction"] = ((chatline.chatdirection == 1) ? "<" : ">");
                    workRow["Chat"] = chatline.chattext;
                    this.Rows.Add(workRow);
                }
            }
        }
    }
}