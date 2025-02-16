using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Data.Base;
using SE.Data.Models;

namespace SE.Data.Repository
{
    public class PrescriptionRepository : GenericRepository<Prescription>
    {
        public PrescriptionRepository() { }
        public PrescriptionRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
    }
}
