using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Content
{
    public class CreatePlaylistRequest
    {
        public int AccountId { get; set; }
        public string PlaylistName { get; set; }
        public IFormFile? PlaylistImage { get; set; }
        public bool IsLesson { get; set; }
    }
}
