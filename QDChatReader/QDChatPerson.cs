using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDChatReader
{
    public class QDChatPerson
    {
        public string id = "";
        public string name = "";
        public int count = 0;
        public DateTime firstAppearance;
        public DateTime lastAppearance;
        public int numberOfLines=0;
        public bool isMe = false;
    }
}
