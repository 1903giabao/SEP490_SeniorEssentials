using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class VideoCallModel
    {
        public int VideoCallId { get; set; }

        public int CallerId { get; set; }

        public string CallerName { get; set; }

        public int ReceiverId { get; set; }

        public string ReceiverName { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string Status { get; set; }
    }
}
