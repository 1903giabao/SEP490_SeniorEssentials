using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Professor
{
    public class ViewProfessorScheduleResponse
    {
        public string Date { get; set; }

        public List<TimeEachSlot> TimeEachSlots { get; set; }

    }
    public class TimeEachSlot
    {
        public int TimeSlotId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }

    }
}
