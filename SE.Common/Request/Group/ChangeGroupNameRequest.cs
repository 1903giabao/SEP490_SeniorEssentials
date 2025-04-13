using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Group
{
    public class ChangeGroupNameRequest
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }
}
