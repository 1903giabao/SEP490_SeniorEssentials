using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SE.Data.Models;
using SE.Data.Base;

namespace SE.Data.Repository
{
    public class ProfessorRepository : GenericRepository<Professor>
    {
        public ProfessorRepository() { }
        public ProfessorRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<Professor> GetAccountByProfessorId (int professorId)
        {
            var rs = await _context.Professors.Include(p=>p.Account).FirstOrDefaultAsync(p=>p.ProfessorId == professorId);
            return rs;
        } 
    }

}
