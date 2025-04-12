using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Profile
{
    public class EditElderlyProfile
    {
        public int ElderlyId { get; set; }    
        public string? Allergy { get; set; }
        public string? LivingSituation { get; set; }
        public List<string>? MedicalRecord { get; set; }
    }
}
