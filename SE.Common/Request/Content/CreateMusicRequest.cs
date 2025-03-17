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
        public string MusicName { get; set; }
        public IFormFile MusicFile { get; set; }
        public string Singer { get; set; }
    }
}
