using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class LessonModel
    {
        public int Id { get; set; }
        public int ContentProviderId { get; set; }
        public string LessonName { get; set; }
        public string LessonDescription { get; set; }
        public string Status { get; set; }
    }
}
