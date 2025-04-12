using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request.Professor
{
    public class FilterProfessorRequest
    {
        public string? NameSortOrder { get; set; } // "asc" or "desc"
        public List<string>? DayOfWeekFilter { get; set; } // ["Monday", "Tuesday", ...]
        public string? RatingSortOrder { get; set; } // "asc" or "desc"
    }
}
