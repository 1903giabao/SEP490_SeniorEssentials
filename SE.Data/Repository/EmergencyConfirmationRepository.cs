using Microsoft.EntityFrameworkCore;
using SE.Data.Base;
using SE.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Data.Repository
{
    public class EmergencyConfirmationRepository: GenericRepository<EmergencyConfirmation>
    {
        public EmergencyConfirmationRepository() { }
        public EmergencyConfirmationRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
    }
}
