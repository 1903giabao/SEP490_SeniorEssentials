using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class WaterReminder
    {
        public string Time { get; set; } // Format: "HH:mm"
        public string Amount { get; set; }
        public string Reason { get; set; }
    }
}
