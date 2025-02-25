using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class CreateBloodPressureRequest
    {
        public int ElderlyId { get; set; }
        public decimal? BloodPressureSystolic { get; set; }
        public decimal? BloodPressureDiastolic { get; set; }
        public string BloodPressureSource { get; set; }
        public string Status { get; set; }
    }
}
