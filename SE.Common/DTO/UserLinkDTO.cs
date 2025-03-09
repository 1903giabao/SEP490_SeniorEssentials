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
        public UserInUserLinkDTO User { get; set; }
    }

    public class UserInUserLinkDTO
    {
        public int RequestUserId { get; set; }

        public int AccountId { get; set; }

        public int RoleId { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string FullName { get; set; }

        public string Avatar { get; set; }

        public string Gender { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string Status { get; set; }

    }
}
