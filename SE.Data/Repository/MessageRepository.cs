using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Data.Models;
using SE.Data.Base;

namespace SE.Data.Repository
{
    public class MessageRepository : GenericRepository<Message>
    {
        public MessageRepository() { }
        public MessageRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }
    }
}
