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
using SE.Common.Request;
using SE.Common.DTO;
using SE.Service.Helper;
using CloudinaryDotNet;

namespace SE.Service.Services
{
    public interface IEmergencyContactService
    {
        Task<IBusinessResult> CreateEmergencyContact(CreateEmergencyContactRequest request);
        Task<IBusinessResult> UpdateEmergencyContact(UpdateEmergencyContactRequest request);
        Task<IBusinessResult> GetEmergencyContactsByElderlyId(int elderlyId);
        Task<IBusinessResult> UpdateEmergencyContactStatus(int emergencyContactId);
        Task<string> ExecuteEmergencyCall(List<string> phoneNums, string doctorNumber);
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

        public async Task<string> ExecuteEmergencyCall(List<string> phoneNums, string doctorNumber)
        {
            try
            {

                foreach (var phone in phoneNums)
                {
                    if (!FunctionCommon.IsValidPhoneNumber(phone))
                    {
                        return $"Invalid phone number: {phone}.";
                    }
                }

                if (!FunctionCommon.IsValidPhoneNumber(doctorNumber))
                {
                    return $"Invalid doctor phone number: {doctorNumber}.";
                }

                var callTasks = phoneNums.Select(phone => MakeCallAsync(phone)).ToList();
                await Task.WhenAll(callTasks);

                await Task.Delay(TimeSpan.FromMinutes(1));

                var doctorCallResult = await MakeCallAsync(doctorNumber);

                return "Emergency calls completed successfully.";
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }

        private async Task<string> MakeCallAsync(string phone)
        {
            try
            {
                var callUrl = $"https://voiceapi.esms.vn/MainService.svc/json/MakeCallRecord_V2?ApiKey={_apiKey}&SecretKey={_secretKey}&TemplateId={115714}&Phone={phone}&MaxRetry={2}&WaitRetry={15}";

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(callUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return result;
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

        public async Task<IBusinessResult> CreateEmergencyContact(CreateEmergencyContactRequest request)
        {
            try
            {
                if (request == null || request.ElderlyId <= 0 || request.AccountIds == null || request.ContactNames == null ||
                    request.AccountIds.Count != request.ContactNames.Count)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Invalid emergency contact data.");
                }

                var emergencyContacts = request.AccountIds
                    .Select((accountId, index) => new EmergencyContact
                    {
                        ElderlyId = request.ElderlyId,
                        AccountId = accountId,
                        ContactName = request.ContactNames[index],
                        Priority = request.Priorities[index],
                        Status = SD.GeneralStatus.ACTIVE 
                    })
                    .ToList();

                await _unitOfWork.EmergencyContactRepository.CreateRangeAsync(emergencyContacts);

                return new BusinessResult(Const.SUCCESS_CREATE, "Emergency contacts created successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateEmergencyContact(UpdateEmergencyContactRequest request)
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
        }


    }
}
