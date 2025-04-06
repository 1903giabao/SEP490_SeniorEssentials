using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class ComboDto
    {
        public int SubscriptionId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Fee { get; set; }

        public int ValidityPeriod { get; set; }

        public string CreatedDate { get; set; }
        public string CreatedTime { get; set; }
        public string UpdatedTime { get; set; }


        public string UpdatedDate { get; set; }

        public string Status { get; set; }

        public int? AccountId { get; set; }

        public int? NumberOfMeeting { get; set; }
    }
}
