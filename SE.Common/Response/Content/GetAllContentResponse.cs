using SE.Common.DTO.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Content
{
    public class GetAllContentResponse
    {
        public int Books {  get; set; }
        public int Musics {  get; set; }
        public int Lessons {  get; set; }
        public int MusicPlaylists {  get; set; }
        public int LessonPlaylists {  get; set; }
    }
}
