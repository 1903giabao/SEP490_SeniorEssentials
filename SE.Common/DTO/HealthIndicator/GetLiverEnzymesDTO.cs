using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO.HealthIndicator
{
    public class GetLiverEnzymesDTO
    {
        public int LiverEnzymesId { get; set; }
        public int ElderlyId { get; set; }
        public string DateRecorded { get; set; }
        public string ALT { get; set; }
        public string AST { get; set; }
        public string ALP { get; set; }
        public string GGT { get; set; }
        public string LiverEnzymesSource { get; set; }
        public string Status { get; set; }
    }
}
