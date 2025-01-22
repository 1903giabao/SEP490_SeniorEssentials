using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class CreateLessonRequest
    {
        public int ContentProviderId { get; set; }
        public string LessonName { get; set; }
        public string LessonDescription { get; set; }
    }
}
