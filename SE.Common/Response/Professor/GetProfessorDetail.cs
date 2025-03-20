using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Professor
{
    public class GetProfessorDetail
    {
        public string FullName { get; set; }
        public string Avatar { get; set; }

        public string DateTime { get; set; }
        public int ProfessorId { get; set; }

        public string Specialization { get; set; }

        public string ClinicAddress { get; set; }

        public decimal ConsultationFee { get; set; }

        public int ExperienceYears { get; set; }

        public decimal Rating { get; set; }

        public string Qualification { get; set; }

        public string Knowledge { get; set; }

        public string Career { get; set; }

        public string Achievement { get; set; }
    }
}
