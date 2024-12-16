using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Data.Models;
using StudySpace.Data.Base;

namespace SE.Data.Repository
{
    public class EmergencyContactRepository : GenericRepository<EmergencyContact>
    {
        public EmergencyContactRepository() { }
        public EmergencyContactRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
    }
}
