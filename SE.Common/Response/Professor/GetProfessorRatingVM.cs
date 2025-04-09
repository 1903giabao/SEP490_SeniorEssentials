using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Professor
{
    public class GetProfessorRatingVM
    {
        public string CreatedBy { get; set; }
        public string Content { get; set; }
        public int Star {  get; set; }

    }
}
