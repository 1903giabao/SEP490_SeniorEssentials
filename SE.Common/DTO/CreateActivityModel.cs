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
        public int Duration { get; set; }
        public List<CreateActivitySchedule> Schedules { get; set; }


    }

    public class CreateActivitySchedule
    {
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }


    }
}