using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Account
{
    public class ChangeAccountStatusReq
    {
        public int AccountId { get; set; }
        public string Status { get; set; }
    }
}
