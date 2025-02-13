using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class ReplyMessageRequest
    {
        public int SenderId { get; set; }
        public string RoomId { get; set; }
        public string RepliedMessageId { get; set; }
        public string RepliedMessage { get; set; }
        public string RepliedMessageType { get; set; }
        public string ReplyTo { get; set; }
        public string? Message { get; set; }
        public IFormFile? FileMessage { get; set; }
        public string MessageType { get; set; }
    }
}
