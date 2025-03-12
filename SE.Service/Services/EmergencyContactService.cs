using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SE.Common.Enums;
using SE.Common;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using SE.Common.DTO;
using SE.Service.Helper;
using CloudinaryDotNet;
using System.Numerics;
using Newtonsoft.Json;
using Firebase.Auth;
using SE.Common.Request.Emergency;
using Org.BouncyCastle.Ocsp;

namespace SE.Service.Services
{
    public interface IEmergencyContactService
    {
/*        Task<IBusinessResult> CreateEmergencyContact(CreateEmergencyContactRequest request);
        Task<IBusinessResult> UpdateEmergencyContact(UpdateEmergencyContactRequest request);
        Task<IBusinessResult> GetEmergencyContactsByElderlyId(int elderlyId);
        Task<IBusinessResult> UpdateEmergencyContactStatus(int emergencyContactId);*/
        Task<IBusinessResult> FamilyEmergencyCall(int accountId);
        Task<IBusinessResult> GetCallStatus(string callId);
        Task<IBusinessResult> DoctorEmergencyCall(int accountId);
    }

    public class EmergencyContactService : IEmergencyContactService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly string _apiKey;
        private readonly string _secretKey;

        public EmergencyContactService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _apiKey = Environment.GetEnvironmentVariable("VoiceCallApiKey");
            _secretKey = Environment.GetEnvironmentVariable("VoiceCallSecretKey");
        }

        public async Task<IBusinessResult> GetCallStatus(string callId)
        {
            try
            {
                var url = $"https://voiceapi.esms.vn/MainService.svc/json/GetSendStatus?ApiKey={_apiKey}&SecretKey={_secretKey}&ReferenceId={callId}";

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var jsonResponse = JsonConvert.DeserializeObject<GetCallStatusDTO>(result);
                        return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, jsonResponse);
                    }
                    else
                    {
                        throw new Exception($"Failed to get status.");
                    }
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, $"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<IBusinessResult> FamilyEmergencyCall(int accountId)
        {
            try
            {
                var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);

                if (account == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Account does not exist!");
                }

                var groupMember = _unitOfWork.GroupMemberRepository.GetAll()
                    .FirstOrDefault(gm => gm.AccountId == accountId && gm.Status == SD.GeneralStatus.ACTIVE);

                if (groupMember == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Account is not in any group.");
                }

                var groupId = groupMember.GroupId;

                var groupMembers = _unitOfWork.GroupMemberRepository.GetAll()
                    .Where(gm => gm.GroupId == groupId && gm.Status == SD.GeneralStatus.ACTIVE)
                    .Select(gm => gm.AccountId)
                    .Distinct()
                    .ToList();

                var otherMembers = groupMembers
                    .Where(id => id != accountId)
                    .ToList();

                if (!otherMembers.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "No other members found in the same group.");
                }

                var phoneNums = new List<string>();

                foreach (var member in otherMembers)
                {
                    var accountPhone = await _unitOfWork.AccountRepository.GetByIdAsync(member);

                    if (accountPhone != null && accountPhone.RoleId == 3)
                    {
                        phoneNums.Add(accountPhone.PhoneNumber);
                    }

                }

                var callTasks = phoneNums.Select<string, Task<dynamic>>(async phone =>
                {
                    try
                    {
                        var result = await MakeCallAsync(phone);
                        return new { Phone = phone, Result = result};
                    }
                    catch (Exception ex)
                    {
                        return new { Phone = phone, Result = ex.Message};
                    }
                }).ToList();

                var callResults = await Task.WhenAll(callTasks);

                var response = callResults.Select(cr => new
                {
                    Phone = cr.Phone,
                    Message = cr.Result
                }).ToList();

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, response);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, $"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<IBusinessResult> DoctorEmergencyCall(int accountId)
        {
            try
            {
                var account = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);

                if (account == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Account does not exist!");
                }

                var bookings = _unitOfWork.BookingRepository.FindByCondition(b => b.ElderlyId == account.Elderly.ElderlyId && b.Status.Equals(SD.BookingStatus.PAID))
                                                           .Select(b => b.BookingId).ToList();

                var doctor = await _unitOfWork.UserServiceRepository.GetProfessorByBookingIdAsync(bookings, SD.GeneralStatus.ACTIVE);

                if (doctor == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Không tìm thấy bác sĩ cho người già này.");
                }

                var phone = doctor.Account.PhoneNumber;

                if (!FunctionCommon.IsValidPhoneNumber(phone))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Số điện thoại của bác sĩ không hợp lệ hoặc không tìm thấy.");
                }

                var callResults = await MakeCallAsync(phone);

                var response = new
                {
                    Phone = phone,
                    Message = callResults
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, response);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, $"An unexpected error occurred: {ex.Message}");
            }
        }

        private async Task<MakeCallResponseDTO> MakeCallAsync(string phone)
        {
            try
            {
                var callUrl = $"https://voiceapi.esms.vn/MainService.svc/json/MakeCallRecord_V2?ApiKey={_apiKey}&SecretKey={_secretKey}&TemplateId={115723}&Phone={phone}&MaxRepeat={2}&MaxRetry={2}&WaitRetry={10}";

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(callUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var jsonResponse = JsonConvert.DeserializeObject<MakeCallResponseDTO>(result);
                        return jsonResponse;
                    }
                    else
                    {
                        throw new Exception($"Failed to call {phone}.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calling {phone}: {ex.Message}");
            }
        }

        public async Task<IBusinessResult> CreateEmergencyInformation(CreateEmergencyInformationRequest request)
        {
            try
            {
                if (request == null || request.ElderlyId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "Invalid emergency data.");
                }

                var elderly = await _unitOfWork.ElderlyRepository.GetByIdAsync(request.ElderlyId);

                if (elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Elderly does not exist.");
                }

                var frontCameraImageLink = ("", "");

                if (request.FrontCameraImage != null)
                {
                    frontCameraImageLink = await CloudinaryHelper.UploadImageAsync(request.FrontCameraImage);
                }     
                
                var rearCameraImageLink = ("", "");

                if (request.RearCameraImage != null)
                {
                    rearCameraImageLink = await CloudinaryHelper.UploadImageAsync(request.RearCameraImage);
                }

                var emergencyInformation = new EmergencyInformation
                {
                    ElderlyId = request.ElderlyId,
                    FrontCameraImage = frontCameraImageLink.Item2,
                    RearCameraImage = rearCameraImageLink.Item2,
                    DateTime = DateTime.UtcNow.AddHours(7),
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Status = SD.GeneralStatus.ACTIVE,
                };

                var createRs = await _unitOfWork.EmergencyInformationRepository.CreateAsync(emergencyInformation);

                if (createRs < 1)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, emergencyInformation);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, ex.Message);
            }
        }

        /*public async Task<IBusinessResult> UpdateEmergencyContact(UpdateEmergencyContactRequest request)
        {
            try
            {
                if (request == null || request.ElderlyId <= 0 || request.AccountId <= 0 ||
                    string.IsNullOrWhiteSpace(request.ContactName) || request.Priority < 0)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Invalid emergency contact update data.");
                }

                var emergencyContact = await _unitOfWork.EmergencyContactRepository
                    .GetByElderlyIdAndAccountIdAsync(request.ElderlyId, request.AccountId);

                if (emergencyContact == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Emergency contact not found.");
                }

                emergencyContact.ContactName = request.ContactName;
                emergencyContact.Priority = request.Priority;

                await _unitOfWork.EmergencyContactRepository.UpdateAsync(emergencyContact);

                return new BusinessResult(Const.SUCCESS_UPDATE, "Emergency contact updated successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, ex.Message);
            }
        }


        public async Task<IBusinessResult> GetEmergencyContactsByElderlyId(int elderlyId)
        {
            try
            {
                if (elderlyId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "Invalid elderly ID.");
                }

                var emergencyContacts = _unitOfWork.EmergencyContactRepository
                    .FindByCondition(ec => ec.ElderlyId == elderlyId)
                    .ToList();

                if (emergencyContacts == null || !emergencyContacts.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "No emergency contacts found for the given elderly ID.");
                }

                var result = _mapper.Map<List<GetEmergencyContactDTO>>(emergencyContacts);

                return new BusinessResult(Const.SUCCESS_READ, "Emergency contacts retrieved successfully.", result);
            }
         
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateEmergencyContactStatus(int emergencyContactId)
        {
            try
            {
                if (emergencyContactId <= 0)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Invalid emergency contact ID.");
                }

                var emergencyContact = _unitOfWork.EmergencyContactRepository
                    .FindByCondition(ec => ec.EmergencyContactId == emergencyContactId)
                    .FirstOrDefault();

                if (emergencyContact == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Emergency contact not found.");
                }

                emergencyContact.Status = SD.GeneralStatus.INACTIVE;

                await _unitOfWork.EmergencyContactRepository.UpdateAsync(emergencyContact);

                return new BusinessResult(Const.SUCCESS_UPDATE, "Emergency contact status updated successfully.");
            }
        
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }*/


    }
}
