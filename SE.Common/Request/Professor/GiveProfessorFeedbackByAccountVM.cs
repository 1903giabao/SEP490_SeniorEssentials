using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Professor
{
    public class GiveProfessorFeedbackByAccountVM
    {
        public int AppointmentId { get; set; }
        public string Content { get; set; }
        public int Star {  get; set; }
        public string CreatedBy { get; set; }
    }
}
