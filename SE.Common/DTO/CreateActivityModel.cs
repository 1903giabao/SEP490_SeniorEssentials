using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class CreateActivityModel
    {
        public int ElderlyId { get; set; }
        public string ActivityName { get; set; }
        public string ActivityDescription { get; set; }
        public string CreatedBy { get; set; }
        public string Status { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
