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
    public class PlaylistRepository : GenericRepository<Playlist>
    {
        public PlaylistRepository() { }
        public PlaylistRepository(SeniorEssentialsContext context)
        {
            _context = context;
        }

        public async Task<List<Playlist>> GetAllMusicsPlaylist(string status)
        {
            var result = await _context.Playlists.Include(p => p.Musics).Where(p => p.IsLesson == false && p.Status.Equals(status)).ToListAsync();
            return result;
        }        
        
        public async Task<List<Playlist>> GetAllLessonsPlaylist(string status)
        {
            var result = await _context.Playlists.Include(p => p.Lessons).Where(p => p.IsLesson == true && p.Status.Equals(status)).ToListAsync();
            return result;
        }
    }
}
