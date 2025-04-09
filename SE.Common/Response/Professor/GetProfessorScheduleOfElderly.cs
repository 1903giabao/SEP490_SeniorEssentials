using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Professor
{
    public class GetProfessorScheduleOfElderly
    {
        public int ProfessorAppointmentId { get; set; }
        public string ProfessorAvatar { get; set; }
        public string ProfessorName { get; set; }
        public string DateTime { get; set; }
        public string Status { get; set; }
        public bool IsOnline { get; set; }

        public bool IsReport {  get; set; }

        public bool IsFeedback { get; set; }
       
        public List<PeopleOfSchedule> People { get; set; }
    }

    public class PeopleOfSchedule
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
