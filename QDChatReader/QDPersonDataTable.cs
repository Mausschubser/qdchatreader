using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDChatReader
{
    public class QDPersonDataTable : DataTable
    {
        public void Init()
        {
            Columns.Add("ID");
            Columns.Add("Name");
            Columns.Add("1stContact", System.Type.GetType("System.DateTime"));
            Columns.Add("Count", System.Type.GetType("System.Int32"));
            Locale = System.Globalization.CultureInfo.InvariantCulture;
        }

        public void Fill(QDChatPersons qdpersons)
        {

            if (Columns.Count < 1)
            {
                Init();
            }
            Clear();
            foreach (QDChatPerson person in qdpersons.List)
            {
                DataRow personRow = NewRow();
                if (!person.isMe)
                {
                    personRow["ID"] = person.id;
                    personRow["Name"] = person.name;
                    personRow["1stContact"] = person.firstAppearance;
                    personRow["Count"] = person.count;
                    Rows.Add(personRow);
                }
            }
        }

    }
}
