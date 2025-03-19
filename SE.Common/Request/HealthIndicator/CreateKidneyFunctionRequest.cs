using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.HealthIndicator
{
    public class CreateKidneyFunctionRequest
    {
        public int AccountId { get; set; }
        public int ElderlyId { get; set; }
        public string Creatinine { get; set; }
        public string BUN { get; set; }
        public string EGFR { get; set; }
        public string KidneyFunctionSource { get; set; }

    }
}
