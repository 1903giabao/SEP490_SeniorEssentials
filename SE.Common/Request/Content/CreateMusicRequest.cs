using Microsoft.AspNetCore.Http;
using SE.Data.Base;
using SE.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Content
{
    public class CreateMusicRequest
    {
        public int AccountId { get; set; }
        public int PlaylistId { get; set; }
        public List<IFormFile> MusicFiles { get; set; }
    }
}
