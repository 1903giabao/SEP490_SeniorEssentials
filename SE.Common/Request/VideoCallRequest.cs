using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class VideoCallRequest
    {
        public int CallerId { get; set; }

        public List<int> ListReceiverId { get; set; }

        public string Duration { get; set; }

        public bool Status { get; set; }
    }
}
