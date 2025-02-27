using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class AddMemberToGroupRequest
    {
        public int GroupId { get; set; }
        public List<int> MemberIds { get; set; }
    }
}
