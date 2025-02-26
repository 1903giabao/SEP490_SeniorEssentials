using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO.HealthIndicator
{
    public class GetKidneyFunctionDTO
    {
        public int KidneyFunctionId { get; set; }
        public int ElderlyId { get; set; }
        public string DateRecorded { get; set; }
        public string Creatinine { get; set; }
        public string BUN { get; set; }
        public string eGFR { get; set; }
        public string KidneyFunctionSource { get; set; }
        public string Status { get; set; }
    }
}
