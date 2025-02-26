using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO.HealthIndicator
{
    public class GetHeightDTO
    {
        public int HeightId { get; set; }
        public int ElderlyId { get; set; }
        public string DateRecorded { get; set; }
        public decimal? Height { get; set; }
        public string HeightSource { get; set; }
        public string? Status { get; set; }
    }
}
