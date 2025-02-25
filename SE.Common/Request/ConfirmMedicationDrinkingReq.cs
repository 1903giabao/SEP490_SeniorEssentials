using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Request
{
    public class ConfirmMedicationDrinkingReq
    {
        public List<MedicationConfirmation> Confirmations { get; set; }
    }

    public class MedicationConfirmation
    {
        public string DateTaken { get; set; }
        public int MedicationId { get; set; }
    }
}
