using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Account
{
    public class CreateProfessorAccountRequest
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string FullName { get; set; }

        public IFormFile? Avatar { get; set; }

        public string Gender { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Specialization { get; set; }

        public string ClinicAddress { get; set; }

        public decimal ConsultationFee { get; set; }

        public int ExperienceYears { get; set; }

        public string Qualification { get; set; }

        public string Knowledge { get; set; }

        public string Career { get; set; }

        public string Achievement { get; set; }
    }
}
