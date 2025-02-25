using SE.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class GetAllGroupMembersDTO
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public List<UserDTO> Members { get; set; }
    }
}
