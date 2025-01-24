using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    using System.Collections.Generic;

    namespace SE.Common.Request
    {
        public class CreateGroupRequest
        {
            public string GroupName { get; set; }
            public int CreatorAccountId { get; set; }
            public List<GroupMemberRequest> Members { get; set; }
        }

        public class GroupMemberRequest
        {
            public int AccountId { get; set; }
            public bool IsCreator { get; set; }
        }
    }
}
