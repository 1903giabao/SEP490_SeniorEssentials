using SE.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Group
{
    public class GetGroupAndRelationshipInforByFamilyMember
    {
        public List<UserDTO> RequestUsers { get; set; }
        public List<UserDTO> ResponseUsers { get; set; }
        public List<UserDTO> FamilyNotInGroup { get; set; }
        public List<GroupInfor> GroupInfors { get; set; }
    }
}
