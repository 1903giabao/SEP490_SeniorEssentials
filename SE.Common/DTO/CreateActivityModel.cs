using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class CreateActivityModel
    {
        public int AccountId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
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