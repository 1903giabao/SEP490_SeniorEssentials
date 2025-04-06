using SE.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Group
{
    public class GetAllElderlyInGroupResponse
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public List<AccountElderlyDTO> Members { get; set; }
    }
}
