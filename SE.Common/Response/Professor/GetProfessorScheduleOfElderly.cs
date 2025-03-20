using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Professor
{
    public class GetProfessorScheduleOfElderly
    {
        public string ProfessorName { get; set; }
        public string DateTime {  get; set; }
        public string Status { get; set; }
    }
}
