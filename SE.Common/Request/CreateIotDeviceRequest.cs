using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class CreateIotDeviceRequest
    {
        public int ElderlyId { get; set; }
        public string DeviceName { get; set; }
        public string SerialNumber { get; set; }
        public int? BatteryLevel { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
    }
}
