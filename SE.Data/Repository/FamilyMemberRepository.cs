using SE.Data.Models;
using SE.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Data.Repository
{
    public class FamilyMemberRepository : GenericRepository<FamilyMember>
    {
        public FamilyMemberRepository() { }
        public FamilyMemberRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
    }
}
