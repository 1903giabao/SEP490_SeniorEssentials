using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Data.Models
{
    public class GroupMemberDTO
    {
        public int GroupId { get; set; }
        public int AccountId { get; set; }
        public string FullName { get; set; }
        public string Avatar {  get; set; }
        public bool IsCreator { get; set; }
        public string GroupName { get; set; }
    }
}
