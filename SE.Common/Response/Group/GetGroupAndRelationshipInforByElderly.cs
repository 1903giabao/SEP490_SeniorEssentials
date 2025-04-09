using SE.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Group
{
    public class GetGroupAndRelationshipInforByElderly
    {
        public List<UserDTO> RequestUsers { get; set; }
        public List<UserDTO> ResponseUsers { get; set; }
        public List<UserDTO> FamilyNotInGroup { get; set; }
        public GroupInfor GroupInfor { get; set; }
    }

    public class GroupInfor
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public List<UserDTO> UsersInGroup { get; set; }
    }
}
