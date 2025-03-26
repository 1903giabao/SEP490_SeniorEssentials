using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Professor
{
    public class UpdateProfessorRequest
    {
        public int AccountId { get; set; }
        public IFormFile? Avatar { get; set; }

        public string? Specialization { get; set; }
        
        public string? ClinicAddress { get; set; }

        public decimal? ConsultationFee { get; set; }

        public int? ExperienceYears { get; set; }

        public string? Qualification { get; set; }

        public string? Knowledge { get; set; }

        public string? Career { get; set; }

        public string? Achievement { get; set; }
    }
}
