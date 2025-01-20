using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Data.Models;
using SE.Data.Base;

namespace SE.Data.Repository
{
    public class GroupMemberRepository : GenericRepository<GroupMember>
    {
        public GroupMemberRepository() { }
        public GroupMemberRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
    }
}
