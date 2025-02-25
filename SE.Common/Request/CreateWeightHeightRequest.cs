using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class CreateWeightHeightRequest
    {
        public int ElderlyId { get; set; }
        public decimal? Weight { get; set; }
        public string WeightSource { get; set; }
        public decimal? Height { get; set; }
        public string HeightSource { get; set; }
        public string? Status { get; set; }
    }
}
