using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO.HealthIndicator
{
    public class GetHeartRateDTO
    {
        public int HeartRateId { get; set; }
        public int ElderlyId { get; set; }
        public string DateRecorded { get; set; }
        public int? HeartRate { get; set; }
        public string HeartRateSource { get; set; }
        public string Status { get; set; }
    }
}
