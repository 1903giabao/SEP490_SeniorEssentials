using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.Response.Emergency
{
    public class GetAllEmergencyResponse
    {
        public int? EmergencyConfirmationId { get; set; }
        public int? ElderlyId { get; set; }
        public string? ElderlyName { get; set; }
        public string? ConfirmationAccountName { get; set; }
        public string? EmergencyDate { get; set; }
        public string? EmergencyTime { get; set; }
        public DateTime? EmergencyDateTime { get; set; }
        public string? ConfirmationDate { get; set; }
        public bool? IsConfirmed { get; set; }
        public string? Status { get; set; }
        public List<GetAllEmergencyInformation>? EmergencyInformations { get; set; }
        public List<GetAllEmergencyContact> EmergencyContacts { get; set; }
    }

    public class GetAllEmergencyInformation
    {
        public int? EmergencyInformationId { get; set; }
        public string? FrontCameraImage { get; set; }
        public string? RearCameraImage { get; set; }
        public string? Longitude { get; set; }
        public string? Latitude { get; set; }
        public string? LongitudeIot { get; set; }
        public string? LatitudeIot { get; set; }
        public string? InformationDate { get; set; }
        public string? InformationTime { get; set; }
        public string? Status { get; set; }
    }

    public class GetAllEmergencyContact
    {
        public int AccountId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
    }
}
