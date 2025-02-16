using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class MessageDTO
    {
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderAvatar { get; set; }
        public string MessageId { get; set; }
        public string Message { get; set; }
        public string MessageType { get; set; }
        public string SentDate { get; set; }
        public string SentTime { get; set; }
        public string SentDateTime { get; set; }
        public bool IsSeen { get; set; }
        public string RepliedMessage { get; set; }
        public string ReplyTo { get; set; }
        public string RepliedMessageType { get; set; }

    }
}
