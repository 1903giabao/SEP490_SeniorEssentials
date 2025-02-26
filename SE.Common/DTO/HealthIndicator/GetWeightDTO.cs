using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO.HealthIndicator
{
    public class GetWeightDTO
    {
        public int WeightId { get; set; }
        public int ElderlyId { get; set; }
        public string DateRecorded { get; set; }
        public decimal? Weight { get; set; }
        public string WeightSource { get; set; }
        public string? Status { get; set; }
    }
}
