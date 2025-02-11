using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class SendMessageRequest
    {
        public int SenderId { get; set; }
        public string RoomId { get; set; }
        public string Message { get; set; }
        public string MessageType { get; set; }
        public string RepliedMessage { get; set; }
        public string RepliedTo { get; set; }
        public string RepliedMessageType { get; set; }
    }
}
