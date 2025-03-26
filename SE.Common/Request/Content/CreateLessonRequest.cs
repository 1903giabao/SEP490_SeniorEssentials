using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Content
{
    public class CreateLessonRequest
    {
        public int AccountId { get; set; }
        public int PlaylistId { get; set; }
        public string LessonName { get; set; }
        public IFormFile? LessonFile { get; set; }
    }
}
