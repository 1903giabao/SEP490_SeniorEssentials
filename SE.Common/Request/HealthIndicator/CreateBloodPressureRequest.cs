using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.HealthIndicator
{
    public class CreateBloodPressureRequest
    {
        public int AccountId { get; set; }
        public int ElderlyId { get; set; }

        public decimal? Systolic { get; set; }
        public decimal? Diastolic { get; set; }
        public string SystolicSource { get; set; }

        public string DiastolicSource { get; set; }
        public string CreatedBy { get; set; }

    }
}
