using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO.Emergency
{
    public class GetEmergencyInformationDTO
    {
        public int? EmergencyInformationId { get; set; }
        public int? EmergencyConfirmationId { get; set; }
        public string? ConfirmationAccountName { get; set; }
        public string? ConfirmationDate { get; set; }
        public string? ConfirmationTime { get; set; }
        public bool? IsConfirmed { get; set; }
        public string? FrontCameraImage { get; set; }
        public string? RearCameraImage { get; set; }
        public string? Longitude { get; set; }
        public string? Latitude { get; set; }
        public string? InformationDate { get; set; }
        public string? InformationTime { get; set; }
        public string? Status { get; set; }
    }
}
