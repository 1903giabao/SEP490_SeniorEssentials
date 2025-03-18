using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.HealthIndicator
{
    public class UpdateHealthIndicatorRequest
    {
        public class KidneyFunctionUpdateRequest
        {
            public decimal Creatinine { get; set; }
            public decimal Bun { get; set; }
            public decimal EGfr { get; set; }
        }

        public class LiverEnzymesUpdateRequest
        {
            public decimal Alt { get; set; }
            public decimal Ast { get; set; }
            public decimal Alp { get; set; }
            public decimal Ggt { get; set; }
        }

        public class LipidProfileUpdateRequest
        {
            public decimal TotalCholesterol { get; set; }
            public decimal LdlCholesterol { get; set; }
            public decimal HdlCholesterol { get; set; }
            public decimal Triglycerides { get; set; }
        }

        public class BloodGlucoseUpdateRequest
        {
            public decimal BloodGlucoseUpdate { get; set; }
            public string Time { get; set; }
        }

        public class BloodPressureUpdateRequest
        {
            public int Systolic { get; set; }
            public int Diastolic { get; set; }
        }
    }
}
