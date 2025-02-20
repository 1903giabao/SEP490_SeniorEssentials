using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class UserLinkDTO
    {
        public int RequestUserId { get; set; }
        public string RequestUserName { get; set; }
        public string RequestUserAvatar { get; set; }        
        public int ResponseUserId { get; set; }
        public string ResponseUserName { get; set; }
        public string ResponseUserAvatar { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
    }
}
