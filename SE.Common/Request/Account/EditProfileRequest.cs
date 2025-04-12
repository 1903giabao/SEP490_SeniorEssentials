using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Account
{
    public class EditProfileRequest
    {
        public int AccountId { get; set; }
        public string FullName { get; set; }
        public IFormFile Avatar {  get; set; }
        public string Gender { get; set; }
        public DateTime Dob {  get; set; }
    }
}
