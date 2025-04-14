using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Professor
{
    public class BookProfessorAppointmentRequest
    {
        public int ElderlyId { get; set; }
        public int? ProfessorId { get; set; }
        public string Day { get; set; } // Date string
        public string StartTime { get; set; } // e.g. "09:00"
        public string EndTime { get; set; } // e.g. "10:00"
        public string Description { get; set; }
    }
}
