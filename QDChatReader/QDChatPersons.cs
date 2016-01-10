using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDChatReader
{
    public class QDChatPersons
    {
        public QDChatPerson Me = new QDChatPerson();
        public QDChatPerson Selected = new QDChatPerson();
        public List<QDChatPerson> List = new List<QDChatPerson>();
        public const int INDEXNOTFOUND= -1;
        
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

        public int GetPersonIndexFromId(string id)
        {
            return List.FindIndex(x => x.id == id);
        }

        public int SetSelectedPerson(int personindex)
        {
            if (personindex != INDEXNOTFOUND)
            {
                Selected = List[personindex];
                Selected.isMe = (Selected.id == Me.id);
            }
            return personindex;
        }

        public QDChatPerson ChangePersonName(string id, string name)
        {
            QDChatPerson foundPerson = new QDChatPerson();
            int personindex = GetPersonIndexFromId(id);
            if (personindex != QDChatPersons.INDEXNOTFOUND)
            {
                List[personindex].name = name;
                if (Selected.id==id)
                {
                    Selected.name = name;
                }
                if (Me.id==id)
                {
                    Me.name = name;
                }
                //personSerializer.SerializeToXML();
                foundPerson = List[personindex];
            }
            return foundPerson;
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
            int personindex = INDEXNOTFOUND;
            personindex = List.FindIndex(x => personid == x.id);
            if (personindex == INDEXNOTFOUND)       //new name ID added, it wasnot found in the list
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
    }

}
