using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class CreateComboModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Fee { get; set; }
        public DateTime ValidityPeriod { get; set; }
    }
}
