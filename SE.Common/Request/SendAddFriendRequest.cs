using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class SendAddFriendRequest
    {
        public int RequestUserId { get; set; }
        public int ResponseUserId { get; set; }
        public string RelationshipType { get; set; }
    }
}
