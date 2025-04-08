using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Professor
{
    public class GetScheduleOfProfessorVM
    {
        public string DayOfWeek { get; set; }
        public List<Time> Times { get; set; } = new List<Time>();
    }
    public class Time
    {
        public string Start
        {
            get; set;
        }
        public string End { get; set; }
    }
}
