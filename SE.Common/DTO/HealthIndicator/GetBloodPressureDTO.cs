using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO.HealthIndicator
{
    public class GetBloodPressureDTO
    {
        public int BloodPressureId { get; set; }
        public int ElderlyId { get; set; }
        public string DateRecorded { get; set; }
        public decimal? BloodPressureSystolic { get; set; }
        public decimal? BloodPressureDiastolic { get; set; }
        public string BloodPressureSource { get; set; }
        public string Status { get; set; }
    }
}
