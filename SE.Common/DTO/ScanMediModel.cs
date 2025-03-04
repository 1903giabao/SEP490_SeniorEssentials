using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class ScanMediModel
    {
        public string Treatment { get; set; }
        public List<MediModel> mediModels { get; set; }
    }

    public class MediModel()
    {
        public int Quantity { get; set; }
        public string Dosage { get; set; }

        public string Name { get; set; }

        public string Time { get; set; }
    }
}
