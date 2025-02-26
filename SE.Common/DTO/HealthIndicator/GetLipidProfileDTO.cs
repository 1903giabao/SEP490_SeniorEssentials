using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO.HealthIndicator
{
    public class GetLipidProfileDTO
    {
        public int LipidProfileId { get; set; }
        public int ElderlyId { get; set; }
        public string DateRecorded { get; set; }
        public string TotalCholesterol { get; set; }
        public string LDLCholesterol { get; set; }
        public string HDLCholesterol { get; set; }
        public string Triglycerides { get; set; }
        public string LipidProfileSource { get; set; }
        public string Status { get; set; }
    }
}
