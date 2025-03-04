using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class MakeCallResponseDTO
    {
        public string CodeResult { get; set; }
        public string SMSID { get; set; }
        public string ErrorMessage { get; set; }
    }
}
