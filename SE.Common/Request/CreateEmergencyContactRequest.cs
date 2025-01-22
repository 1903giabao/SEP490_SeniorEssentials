using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class CreateEmergencyContactRequest
    {
        public int ElderlyId { get; set; }
        public List<int> AccountIds { get; set; }
        public List<string> ContactNames { get; set; }
    }
}
