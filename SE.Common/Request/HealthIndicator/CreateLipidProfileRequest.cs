using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.HealthIndicator
{
    public class CreateLipidProfileRequest
    {
        public int AccountId { get; set; }
        public string TotalCholesterol { get; set; }
        public string LDLCholesterol { get; set; }
        public string HDLCholesterol { get; set; }
        public string Triglycerides { get; set; }
        public string LipidProfileSource { get; set; }
        public string CreatedBy { get; set; }

    }
}
