using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response
{
    public class GetScheduleInDayResponse
    {
        public string Title {  get; set; }
        public string Description { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int ElderlyId { get; set; }
        public string Type { get; set; }
    }
}
