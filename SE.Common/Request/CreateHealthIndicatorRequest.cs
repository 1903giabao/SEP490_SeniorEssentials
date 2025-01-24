using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class CreateHealthIndicatorRequest
    {
        public int ElderlyId { get; set; }
        public DateTime DateRecorded { get; set; }
        public decimal? BloodPressureSystolic { get; set; }
        public string BloodPressureSystolicSource { get; set; }
        public decimal? BloodPressureDiastolic { get; set; }
        public string BloodPressureDiastolicSource { get; set; }
        public int? HeartRate { get; set; }
        public string HeartRateSource { get; set; }
        public decimal? Weight { get; set; }
        public string WeightSource { get; set; }
        public decimal? Height { get; set; }
        public string HeightSource { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
    }
}
