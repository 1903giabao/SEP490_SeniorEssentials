﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.HealthIndicator
{
    public class CreateBloodGlucoseRequest
    {
        public int AccountId { get; set; }
        public int ElderlyId { get; set; }

        public string BloodGlucose1 { get; set; }
        public string BloodGlucoseSource { get; set; }
        public string Time {  get; set; }
    }
}
