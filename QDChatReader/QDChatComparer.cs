using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDChatReader
{
    public class QDChatComparer : IComparer<QDChatLine>
    {
        public int Compare(QDChatLine qdchatline1, QDChatLine qdchatline2)
        {
            double timeoffset1 = qdchatline1.timeoffset;
            double timeoffset2 = qdchatline2.timeoffset;
            if (timeoffset1 > timeoffset2)
            {
                return 1;
            }
            else if (timeoffset1 < timeoffset2)
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
