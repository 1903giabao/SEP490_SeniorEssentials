using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class GetCallStatusDTO
    {
        public string CallDuration { get; set; }
        public string CallStatus { get; set; }
        public string CodeResponse { get; set; }
        public string Ivr { get; set; }
        public string ReferenceId { get; set; }
        public string SendStatus { get; set; }
        public string SentResult { get; set; }
    }
}
