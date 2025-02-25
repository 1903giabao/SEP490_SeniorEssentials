using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class CreateKidneyFunctionRequest
    {
        public int ElderlyId { get; set; }
        public DateTime DateRecorded { get; set; }
        public string BUN { get; set; }
        public string eGFR { get; set; }
        public string KidneyFunctionSource { get; set; }
        public string Status { get; set; }
    }
}
