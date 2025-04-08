using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class ProfessorScheduleRequest
    {
        public int ProfessorId { get; set; }
        public List<TimeRequest> ListTime {  get; set; }
    }
    public class TimeRequest
    {

        public string DayOfWeek { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }
    }
}
