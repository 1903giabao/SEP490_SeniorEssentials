using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class CreateUserReq
    {
        public string? OTPCode { get; set; }
        public string Account { get; set; } = string.Empty;

    }
}
