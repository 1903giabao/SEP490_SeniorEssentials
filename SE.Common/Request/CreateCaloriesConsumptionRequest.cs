using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class CreateCaloriesConsumptionRequest
    {
        public int AccountId { get; set; }
        public int ElderlyId { get; set; }
        public decimal? CaloriesConsumption1 { get; set; }
        public string CaloriesConsumptionSource { get; set; }
    }
}
