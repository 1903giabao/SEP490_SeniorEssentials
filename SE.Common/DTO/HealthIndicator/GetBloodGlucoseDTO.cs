using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO.HealthIndicator
{
    public class GetBloodGlucoseDTO
    {
        public int BloodGlucoseId { get; set; }
        public int ElderlyId { get; set; }
        public string DateRecorded { get; set; }
        public string BloodGlucose { get; set; }
        public string BloodGlucoseSource { get; set; }
        public string Status { get; set; }
    }
}
