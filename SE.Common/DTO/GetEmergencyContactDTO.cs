using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class GetEmergencyContactDTO
    {
        public int EmergencyContactId { get; set; }

        public int ElderlyId { get; set; }

        public int AccountId { get; set; }

        public int Priority { get; set; }

        public string ContactName { get; set; }

        public string Status { get; set; }

    }
}
