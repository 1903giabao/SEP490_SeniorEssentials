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
        public decimal? BloodPressureDiastolic { get; set; }
        public string BloodPressureSource { get; set; }
        public int? HeartRate { get; set; }
        public string HeartRateSource { get; set; }
        public decimal? Weight { get; set; }
        public string WeightSource { get; set; }
        public decimal? Height { get; set; }
        public string HeightSource { get; set; }
        public string BloodGlucose { get; set; }
        public string BloodGlucoseSource { get; set; }
        public string TotalCholesterol { get; set; }
        public string LDLCholesterol { get; set; }
        public string HDLCholesterol { get; set; }
        public string Triglycerides { get; set; }
        public string LipidProfileSource { get; set; }
        public string ALT { get; set; }
        public string AST { get; set; }
        public string ALP { get; set; }
        public string GGT { get; set; }
        public string LiverEnzymesSource { get; set; }
        public string BUN { get; set; }
        public string eGFR { get; set; }
        public string KidneyFunctionSource { get; set; }
        public string Status { get; set; }
    }
}
