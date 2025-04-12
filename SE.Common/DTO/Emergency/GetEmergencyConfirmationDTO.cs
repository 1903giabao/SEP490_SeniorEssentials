using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO.Emergency
{
    public class GetEmergencyConfirmationDTO
    {
        public int? EmergencyConfirmationId {  get; set; }
        public int? ElderlyId {  get; set; }
        public string? ConfirmationAccountName { get; set; }
        public string? EmergencyDate { get; set; }
        public string? EmergencyTime { get; set; }
        public string? ConfirmationDate { get; set; }
        public bool? IsConfirmed { get; set; }
        public string? Status { get; set; }
    }
}
