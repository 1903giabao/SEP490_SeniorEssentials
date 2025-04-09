using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Professor
{
    public class CreateReportRequest
    {
        public int ProfessorAppointmentId { get; set; }
        public string Content { get; set; }
        public string Solution { get; set; }
    }
}
