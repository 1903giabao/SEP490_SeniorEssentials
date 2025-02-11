using SE.Common.Request.SE.Common.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class CreateGroupChatRequest
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupAvatar {  get; set; }
        public List<GroupMemberRequest> Members { get; set; }

    }
}
