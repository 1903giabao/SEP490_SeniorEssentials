using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SE.Common.DTO
{
    public class SignUpModel
    {
        public int AccountId {  get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public IFormFile Avatar { get; set; }
        public string PhoneNumber { get; set; }

        public int RoleId {  get; set; }
        public List<string> MedicalRecord { get; set; }
        public string Height {  get; set; }
        public string Weight { get; set; }

    }
}
