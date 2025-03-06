using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Data.Models;
using SE.Data.Base;
using Microsoft.EntityFrameworkCore;

namespace SE.Data.Repository
{
    public class EmergencyInformationRepository : GenericRepository<EmergencyInformation>
    {
        public EmergencyInformationRepository() { }
        public EmergencyInformationRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
    }
}
