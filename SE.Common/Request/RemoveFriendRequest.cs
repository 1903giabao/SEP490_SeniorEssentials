using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class RemoveFriendRequest
    {
        public int RequestUserId { get; set; }
        public int ResponseUserId { get; set; }
    }
}
