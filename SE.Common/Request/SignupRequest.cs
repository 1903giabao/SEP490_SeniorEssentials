using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class SignupRequest
    {
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
