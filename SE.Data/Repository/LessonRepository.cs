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
    public class LessonRepository : GenericRepository<Lesson>
    {
        public LessonRepository() { }
        public LessonRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<List<Lesson>> GetLessonsByPlaylist (int playlistId)
        {
            var result = await _context.Lessons.Include(l => l.Playlist).Where(l => l.PlaylistId == playlistId && l.Status.Equals("Active")).ToListAsync();
            return result;
        }
    }
}
