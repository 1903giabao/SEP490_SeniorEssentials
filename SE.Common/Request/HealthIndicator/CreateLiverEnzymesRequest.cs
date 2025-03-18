using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.HealthIndicator
{
    public class CreateLiverEnzymesRequest
    {
        public int AccountId { get; set; }
        public string Alt { get; set; }
        public string Ast { get; set; }
        public string Alp { get; set; }
        public string Ggt { get; set; }
        public string LiverEnzymesSource { get; set; }
        public string CreatedBy { get; set; }

    }
}
