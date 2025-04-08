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
        public int TimeSlotId { get; set; }
        public string Day {  get; set; }
        public string? Description {  get; set; }
    }
}
