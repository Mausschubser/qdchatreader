using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDChatReader
{
    public class QDChatLine
    {
        public string senderid = "";
        public string receiverid = "";
        public string chattext = "";
        public double timeoffset = 0.0;
        public DateTime timestamp;
        public int chatdirection = IN;
        private int numberOfLines=0;

        private DateTime basetime = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public const int IN = 0;
        public const int OUT = 1;

        public int NumberOfLines
        {
            get
            {
                numberOfLines = (chattext.Length / 20) + 1;
                return numberOfLines;
            }
        }
 
        public int ConvertRawField()
        {
            this.timestamp = basetime.AddSeconds(this.timeoffset);
            return 0;
        }
    }
}
