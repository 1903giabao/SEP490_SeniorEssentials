using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class CreateBloodGlucoseRequest
    {
        public int ElderlyId { get; set; }
        public DateTime DateRecorded { get; set; }
        public string BloodGlucose { get; set; }
        public string BloodGlucoseSource { get; set; }
        public string Status { get; set; }
    }
}
