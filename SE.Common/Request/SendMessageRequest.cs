﻿using Microsoft.AspNetCore.Http;
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
        public string? Message { get; set; }
        public IFormFile? FileMessage { get; set; }
        public string MessageType { get; set; }
    }
}
