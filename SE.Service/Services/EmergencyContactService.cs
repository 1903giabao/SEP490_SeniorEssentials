﻿using System;
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
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using SE.Common.DTO.Emergency;
using SE.Common.Response.Emergency;
using SE.Common.Request;
using System.Text.RegularExpressions;

namespace SE.Service.Services
{
    public interface IEmergencyContactService
    {
        Task<IBusinessResult> GetAllEmergencyConfirmation();
        Task<IBusinessResult> FamilyEmergencyCall(int accountId);
        Task<IBusinessResult> GetCallStatus(string callId);
        Task<IBusinessResult> DoctorEmergencyCall(int accountId);
        Task<IBusinessResult> SoS115EmergencyCall();
        Task<IBusinessResult> GetEmergencyConfirmation(int emergencyId);
        Task<IBusinessResult> GetListEmergencyConfirmationByElderly(int elderlyId);
        Task<IBusinessResult> GetListEmergencyInformation(int emergencyId);
        Task<IBusinessResult> GetNewestEmergencyInformation(int emergencyId);
        Task<IBusinessResult> CreateEmergencyInformation(CreateEmergencyInformationRequest request);
        Task<IBusinessResult> CreateEmergencyConfirmation(int elderlyId);
        Task<IBusinessResult> ConfirmEmergency(int accountId, int emergencyId);
        Task<IBusinessResult> GetListEmergencyConfirmationByFamilyMember(int familyMemberId);
        Task<IBusinessResult> GetListEmergencyConfirmationByProfessor(int professorId);
        Task<IBusinessResult> ExpiredEmergency(int emergencyId);
    }

    public class EmergencyContactService : IEmergencyContactService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly ISmsService _smsService;
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly IGroupService _groupService;

        public EmergencyContactService(UnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, ISmsService smsService, IGroupService groupService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _apiKey = Environment.GetEnvironmentVariable("VoiceCallApiKey");
            _secretKey = Environment.GetEnvironmentVariable("VoiceCallSecretKey");
            _notificationService = notificationService;
            _smsService = smsService;
            _groupService = groupService;
        }

        public async Task<IBusinessResult> GetAllEmergencyConfirmation()
        {
            try
            {
                var result = new List<GetAllEmergencyResponse>();

                var emergencyList = await _unitOfWork.EmergencyConfirmationRepository.GetAllEmergencyConfirmationAsync();

                foreach (var e in emergencyList)
                {
                    var listInformation = await _unitOfWork.EmergencyInformationRepository.GetListEmergencyInformation(e.EmergencyConfirmationId);

                    var emergencyInformationResponseList = listInformation.Select(information => new GetAllEmergencyInformation
                    {
                        EmergencyInformationId = information.EmergencyInformationId,
                        FrontCameraImage = information.FrontCameraImage,
                        RearCameraImage = information.RearCameraImage,
                        Latitude = information.Latitude,
                        Longitude = information.Longitude,
                        InformationDate = information.DateTime?.ToString("dd-MM-yyyy"),
                        InformationTime = information.DateTime?.ToString("HH:mm"),
                        Status = information.Status,
                        LatitudeIot = information.LatitudeIot, 
                        LongitudeIot = information.LongitudeIot
                    }).ToList();

                    var emergencyContacts = await GetAllFamilyMemberByElderlyId((int)e.Elderly.AccountId);

                    var emergencyConfirmationResponse = new GetAllEmergencyResponse
                    {
                        EmergencyConfirmationId = e.EmergencyConfirmationId,
                        ElderlyId = e.ElderlyId,
                        ElderlyName = e.Elderly.Account.FullName,
                        EmergencyDate = e.EmergencyDate?.ToString("dd-MM-yyyy"),
                        EmergencyTime = e.EmergencyDate?.ToString("HH:mm"),
                        ConfirmationAccountName = e.ConfirmationAccount == null ? "" : e.ConfirmationAccount.FullName,
                        ConfirmationDate = e.ConfirmationDate?.ToString("dd-MM-yyyy HH:mm"),
                        IsConfirmed = (bool)(e.IsConfirm == null ? false : e.IsConfirm),
                        EmergencyInformations = emergencyInformationResponseList,
                        EmergencyDateTime = e.EmergencyDate,
                        EmergencyContacts = emergencyContacts,
                        Status = e.Status
                    };

                    result.Add(emergencyConfirmationResponse);
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result.OrderByDescending(r => r.EmergencyDateTime).ToList());
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, $"An unexpected error occurred: {ex.Message}");
            }
        }

        private async Task<List<GetAllEmergencyContact>> GetAllFamilyMemberByElderlyId(int accountId)
        {
            try
            {
                var elderly = _unitOfWork.AccountRepository.FindByCondition(a => a.AccountId == accountId && a.RoleId == 2 && a.Status.Equals(SD.GeneralStatus.ACTIVE)).FirstOrDefault();

                if (elderly == null)
                {
                    return new List<GetAllEmergencyContact>();
                }

                var userGroup = _unitOfWork.GroupMemberRepository.GetAll()
                    .Where(gm => gm.AccountId == accountId && gm.Status == SD.GeneralStatus.ACTIVE)
                    .Select(gm => gm.GroupId)
                    .FirstOrDefault();

                var result = new List<GetAllEmergencyContact>();

                var group = await _unitOfWork.GroupRepository.GetByIdAsync(userGroup);

                if (group == null)
                {
                    return new List<GetAllEmergencyContact>();
                }

                var familyMembers = await _unitOfWork.GroupMemberRepository.GetAccountFamilyMemberInGroupByGroupIdAsync(group.GroupId, SD.GeneralStatus.ACTIVE);

                if (familyMembers.Any())
                {
                    var users = familyMembers.Select(f => new GetAllEmergencyContact
                    {
                        AccountId = f.AccountId,
                        FullName = f.FullName,
                        PhoneNumber = f.PhoneNumber,
                    }).ToList();

                    result.AddRange(users);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
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

                var groupMembers = await _unitOfWork.GroupMemberRepository.GetFamilyMemberInGroupByGroupIdAsync(groupId, SD.GeneralStatus.ACTIVE);

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

                var doctor = await _unitOfWork.UserServiceRepository.GetProfessorByBookingIdAsync(bookings, SD.UserSubscriptionStatus.AVAILABLE);

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
        
        public async Task<IBusinessResult> SoS115EmergencyCall()
        {
            try
            {
                var callResults = await MakeCallAsync("0912393903");

                var response = new
                {
                    Phone = "0912393903",
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

        public async Task<IBusinessResult> GetNewestEmergencyInformation(int emergencyId)
        {
            try
            {
                var emergency = await _unitOfWork.EmergencyConfirmationRepository.GetEmergencyConfirmationByIdAsync(emergencyId);

                if (emergency == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Không tìm thấy đợt emergency này!");
                }

                var newestInformation = await _unitOfWork.EmergencyInformationRepository.GetNewestEmergencyInformation(emergency.EmergencyConfirmationId);

                if (newestInformation == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Không tìm thấy emergency information nào!");
                }

                var result = new GetEmergencyInformationDTO
                {
                    EmergencyInformationId = newestInformation.EmergencyInformationId,
                    EmergencyConfirmationId = emergency.EmergencyConfirmationId,
                    ConfirmationAccountName = newestInformation.EmergencyConfirmation.ConfirmationAccount == null ? "" : newestInformation.EmergencyConfirmation.ConfirmationAccount.FullName,
                    ConfirmationDate = newestInformation.EmergencyConfirmation.ConfirmationDate?.ToString("dd-MM-yyyy"),
                    ConfirmationTime = newestInformation.EmergencyConfirmation.ConfirmationDate?.ToString("HH:mm"),
                    IsConfirmed = newestInformation.EmergencyConfirmation.IsConfirm,
                    FrontCameraImage = newestInformation.FrontCameraImage,
                    RearCameraImage = newestInformation.RearCameraImage,
                    Latitude = newestInformation.Latitude,
                    Longitude = newestInformation.Longitude,
                    InformationDate = newestInformation.DateTime?.ToString("dd-MM-yyyy"),
                    InformationTime = newestInformation.DateTime?.ToString("HH:mm"),
                    Status = newestInformation.Status
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, $"An unexpected error occurred: {ex.Message}");
            }
        }        
        
        public async Task<IBusinessResult> GetListEmergencyInformation(int emergencyId)
        {
            try
            {
                var emergency = await _unitOfWork.EmergencyConfirmationRepository.GetEmergencyConfirmationByIdAsync(emergencyId);

                if (emergency == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Không tìm thấy đợt emergency này!");
                }

                var listInformation = await _unitOfWork.EmergencyInformationRepository.GetListEmergencyInformation(emergency.EmergencyConfirmationId);

                var result = listInformation.Select(information => new GetEmergencyInformationDTO
                {
                    EmergencyInformationId = information.EmergencyInformationId,
                    EmergencyConfirmationId = emergency.EmergencyConfirmationId,
                    ConfirmationAccountName = information.EmergencyConfirmation.ConfirmationAccount == null ? "" : information.EmergencyConfirmation.ConfirmationAccount.FullName,
                    ConfirmationDate = information.EmergencyConfirmation.ConfirmationDate?.ToString("dd-MM-yyyy"),
                    ConfirmationTime = information.EmergencyConfirmation.ConfirmationDate?.ToString("HH:mm"),
                    IsConfirmed = information.EmergencyConfirmation.IsConfirm,
                    FrontCameraImage = information.FrontCameraImage,
                    RearCameraImage = information.RearCameraImage,
                    Latitude = information.Latitude,
                    Longitude = information.Longitude,
                    InformationDate = information.DateTime?.ToString("dd-MM-yyyy"),
                    InformationTime = information.DateTime?.ToString("HH:mm"),
                    Status = information.Status
                });

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, $"An unexpected error occurred: {ex.Message}");
            }
        }        
        
        public async Task<IBusinessResult> GetListEmergencyConfirmationByElderly(int elderlyId)
        {
            try
            {
                var emergency = await _unitOfWork.EmergencyConfirmationRepository.GetListEmergencyConfirmationByElderlyIdAsync(elderlyId);

                var result = emergency.Select(e => new GetEmergencyConfirmationDTO
                {
                    EmergencyConfirmationId = e.EmergencyConfirmationId,
                    ElderlyId = e.ElderlyId,
                    EmergencyDate = e.EmergencyDate?.ToString("dd-MM-yyyy"),
                    EmergencyTime = e.EmergencyDate?.ToString("HH:mm"),
                    ConfirmationAccountName = e.ConfirmationAccount == null ? "" : e.ConfirmationAccount.FullName,
                    ConfirmationDate = e.ConfirmationDate?.ToString("dd-MM-yyyy HH:mm"),
                    IsConfirmed = (bool)(e.IsConfirm == null ? false : e.IsConfirm),
                    Status = e.Status
                });

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, $"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<IBusinessResult> GetListEmergencyConfirmationByFamilyMember(int familyMemberId)
        {
            try
            {
                var account = await _unitOfWork.AccountRepository.GetByIdAsync(familyMemberId);

                if (account == null || account.RoleId != 3)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Family member does not exist!");
                }

                var groupMember = _unitOfWork.GroupMemberRepository.GetAll()
                    .FirstOrDefault(gm => gm.AccountId == familyMemberId && gm.Status == SD.GeneralStatus.ACTIVE);

                if (groupMember == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Account is not in any group.");
                }

                var groupId = groupMember.GroupId;

                var eldersInGroup = await _unitOfWork.GroupMemberRepository.GetElderlyInGroupByGroupIdAsync(groupId, SD.GeneralStatus.ACTIVE);

                var otherMembers = eldersInGroup
                    .Where(id => id.AccountId != familyMemberId)
                    .ToList();

                var totalResult = new List<GetListEmergencyConfirmationByFamilyMemberDTO>();

                foreach ( var elderly in otherMembers )
                {
                    var emergency = await _unitOfWork.EmergencyConfirmationRepository.GetListEmergencyConfirmationByElderlyIdAsync(elderly.ElderlyId);

                    var listResult = emergency.Select(e => new GetEmergencyConfirmationDTO
                    {
                        EmergencyConfirmationId = e.EmergencyConfirmationId,
                        ElderlyId = e.ElderlyId,
                        EmergencyDate = e.EmergencyDate?.ToString("dd-MM-yyyy"),
                        EmergencyTime = e.EmergencyDate?.ToString("HH:mm"),
                        ConfirmationAccountName = e.ConfirmationAccount == null ? "" : e.ConfirmationAccount.FullName,
                        ConfirmationDate = e.ConfirmationDate?.ToString("dd-MM-yyyy HH:mm"),
                        IsConfirmed = (e.IsConfirm == null ? false : e.IsConfirm),
                        Status = e.Status,
                    }).ToList();

                    totalResult.Add(new GetListEmergencyConfirmationByFamilyMemberDTO
                    {
                        ElderlyId = elderly.ElderlyId,
                        ElderlyName = elderly.Account.FullName,
                        PhoneNumber = elderly.Account.PhoneNumber,
                        GetEmergencyConfirmationDTOs = listResult
                    });
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, totalResult);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, $"An unexpected error occurred: {ex.Message}");
            }
        }        
        
        public async Task<IBusinessResult> GetListEmergencyConfirmationByProfessor(int professorId)
        {
            try
            {
                var account = await _unitOfWork.AccountRepository.GetAccountAsync(professorId);

                if (account == null || account.RoleId != 4)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor does not exist!");
                }

                var listUserSubscription = await _unitOfWork.UserServiceRepository.GetUserSubscriptionByProfessorAsync(account.AccountId, SD.UserSubscriptionStatus.AVAILABLE);

                if (!listUserSubscription.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, new List<int>());
                }

                var totalResult = new List<GetListEmergencyConfirmationByFamilyMemberDTO>();

                foreach ( var elderly in listUserSubscription)
                {
                    var emergency = await _unitOfWork.EmergencyConfirmationRepository.GetListEmergencyConfirmationByElderlyIdAsync(elderly.Booking.ElderlyId);

                    var listResult = emergency.Select(e => new GetEmergencyConfirmationDTO
                    {
                        EmergencyConfirmationId = e.EmergencyConfirmationId,
                        ElderlyId = e.ElderlyId,
                        EmergencyDate = e.EmergencyDate?.ToString("dd-MM-yyyy"),
                        EmergencyTime = e.EmergencyDate?.ToString("HH:mm"),
                        ConfirmationAccountName = e.ConfirmationAccount == null ? "" : e.ConfirmationAccount.FullName,
                        ConfirmationDate = e.ConfirmationDate?.ToString("dd-MM-yyyy HH:mm"),
                        IsConfirmed = (e.IsConfirm == null ? false : e.IsConfirm),
                        Status = e.Status,
                    }).ToList();

                    totalResult.Add(new GetListEmergencyConfirmationByFamilyMemberDTO
                    {
                        ElderlyId = elderly.Booking.ElderlyId,
                        ElderlyName = elderly.Booking.Elderly.Account.FullName,
                        PhoneNumber = elderly.Booking.Elderly.Account.PhoneNumber,
                        GetEmergencyConfirmationDTOs = listResult
                    });
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, totalResult);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, $"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<IBusinessResult> GetEmergencyConfirmation(int emergencyId)
        {
            try
            {
                var e = await _unitOfWork.EmergencyConfirmationRepository.GetEmergencyConfirmationByIdAsync(emergencyId);

                var result = new GetEmergencyConfirmationDTO
                {
                    EmergencyConfirmationId = e.EmergencyConfirmationId,
                    ElderlyId = (int)e.ElderlyId,
                    EmergencyDate = e.EmergencyDate?.ToString("dd-MM-yyyy"),
                    EmergencyTime = e.EmergencyDate?.ToString("HH:mm"),
                    ConfirmationAccountName = e.ConfirmationAccount == null ? "" : e.ConfirmationAccount.FullName,
                    ConfirmationDate = e.ConfirmationDate?.ToString("dd-MM-yyyy HH:mm"),
                    IsConfirmed = (bool)(e.IsConfirm == null ? false : e.IsConfirm),
                    Status = e.Status
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, $"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<IBusinessResult> CreateEmergencyInformation(CreateEmergencyInformationRequest request)
        {
            try
            {
                if (request == null || request.EmergencyConfirmationId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "Invalid emergency data.");
                }

                var emergencyConfirmation = await _unitOfWork.EmergencyConfirmationRepository.GetEmergencyConfirmationByIdAsync(request.EmergencyConfirmationId);

                if (emergencyConfirmation == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Emergency does not exist.");
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
                    EmergencyConfirmationId = request.EmergencyConfirmationId,
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

                if (request.IsSendMessage)
                {
                    var groupMember = _unitOfWork.GroupMemberRepository.GetAll()
                   .FirstOrDefault(gm => gm.AccountId == emergencyConfirmation.Elderly.AccountId && gm.Status == SD.GeneralStatus.ACTIVE);

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
                        .Where(id => id != emergencyConfirmation.Elderly.AccountId)
                        .ToList();

                    var phoneNums = new List<string>();
                    var deviceTokens = new List<string>();

                    foreach (var member in otherMembers)
                    {
                        var accountPhone = await _unitOfWork.AccountRepository.GetByIdAsync(member);

                        if (accountPhone != null && accountPhone.RoleId == 3)
                        {
                            phoneNums.Add(accountPhone.PhoneNumber);
                            deviceTokens.Add(accountPhone.DeviceToken);
                        }
                    }

                    string latitude = request.Latitude;
                    string longitude = request.Longitude;
                    string googleMapsUrl = $"https://www.google.com/maps?q={latitude},{longitude}";

                    var smsTasks = phoneNums.Select<string, Task<dynamic>>(async phone =>
                    {
                        try
                        {
                            var result = await _smsService.SendSmsAsync(phone,"Đây là vị trí của người thân bạn, hãy đến mau: " + googleMapsUrl);
                            return new { Phone = phone, Result = result };
                        }
                        catch (Exception ex)
                        {
                            return new { Phone = phone, Result = ex.Message };
                        }
                    }).ToList();

                    var notiTasks = deviceTokens.Select<string, Task<dynamic>>(async token =>
                    {
                        try
                        {
                            var result = await _notificationService.SendNotification(token, "TÍN HIỆU CẦU CỨU KHẨN CẤP", "Người thân của bạn đang gặp tình trạng khẩn cấp, hãy truy cập vào ứng dụng để xem vị trí!");
                            return new { Token = token, Result = result };
                        }
                        catch (Exception ex)
                        {
                            return new { Token = token, Result = ex.Message };
                        }
                    }).ToList();

                    if (request.CallProfessor)
                    {
                        var bookings = _unitOfWork.BookingRepository.FindByCondition(b => b.ElderlyId == emergencyConfirmation.ElderlyId && b.Status.Equals(SD.BookingStatus.PAID))
                                                                .Select(b => b.BookingId).ToList();

                        var doctor = await _unitOfWork.UserServiceRepository.GetProfessorByBookingIdAsync(bookings, SD.UserSubscriptionStatus.AVAILABLE);

                        if (doctor != null)
                        {
                            var phone = doctor.Account.PhoneNumber;
                            var doctorToken = doctor.Account.DeviceToken;

                            if (FunctionCommon.IsValidPhoneNumber(phone))
                            {
                                await _smsService.SendSmsAsync(phone, "Đây là vị trí của bệnh nhân bạn: " + googleMapsUrl);
                            }

                            await _notificationService.SendNotification(doctorToken, "TÍN HIỆU CẦU CỨU KHẨN CẤP", "Bệnh nhân của bạn đang gặp tình trạng khẩn cấp, hãy truy cập vào ứng dụng để xem vị trí!");
                        }
                    }
                }              

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateEmergencyConfirmation(int elderlyId)
        {
            try
            {
                var elderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(elderlyId);

                if (elderly == null || elderly.RoleId != 2)
                {
                    return new BusinessResult(Const.FAIL_READ, "Elderly does not exist.");
                }

                var emergencyConfirmation = new EmergencyConfirmation
                {
                    ElderlyId = elderly.Elderly.ElderlyId,
                    ConfirmationAccountId = null,
                    ConfirmationDate = null,
                    IsConfirm = false,
                    EmergencyDate = DateTime.UtcNow.AddHours(7),
                    Status = SD.EmergencyStatus.PENDING,
                };

                var createRs = await _unitOfWork.EmergencyConfirmationRepository.CreateAsync(emergencyConfirmation);

                if (createRs < 1)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
                }

                var listFamilyMember = await _groupService.GetAllFamilyMembersByElderly(elderly.AccountId);

                foreach (var member in listFamilyMember)
                {
                    var familyMember = await _unitOfWork.AccountRepository.GetByIdAsync(member);

                    if (!string.IsNullOrEmpty(familyMember.DeviceToken) && familyMember.DeviceToken != "string")
                    {
                        // Send notification
                        await _notificationService.SendNotification(
                            familyMember.DeviceToken,
                            "Tín hiệu cầu cứu khẩn cấp",
                            $"Người già {elderly.FullName} đang gặp phải tình huống khẩn cấp, nhanh chóng truy cập vào ứng dụng Senior Essentials để xem thêm các thông tin chi tiết.");

                        var newNotification = new Data.Models.Notification
                        {
                            NotificationType = "SOS",
                            AccountId = familyMember.AccountId,
                            Status = SD.NotificationStatus.SEND,
                            Title = "Tín hiệu cầu cứu khẩn cấp",
                            Message = $"Người già {elderly.FullName} đang gặp phải tình huống khẩn cấp, nhanh chóng truy cập vào ứng dụng Senior Essentials để xem thêm các thông tin chi tiết.",
                            CreatedDate = System.DateTime.UtcNow.AddHours(7),
                        };

                        await _unitOfWork.NotificationRepository.CreateAsync(newNotification);
                    }
                }

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, new { EmergencyConfirmationId = emergencyConfirmation.EmergencyConfirmationId });
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, ex.Message);
            }
        }        
        
        public async Task<IBusinessResult> ConfirmEmergency(int accountId, int emergencyId)
        
        {
            try
            {
                var account = await _unitOfWork.AccountRepository.GetAccountAsync(accountId);

                if (account == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Account does not exist.");
                }                
                
                if (account.RoleId != 2 && account.RoleId != 3)
                {
                    return new BusinessResult(Const.FAIL_READ, "Invalid role.");
                }

                var emergencyConfirmation = await _unitOfWork.EmergencyConfirmationRepository.GetByIdAsync(emergencyId);

                if (emergencyConfirmation == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Emergency Confirmation does not exist.");
                }

                emergencyConfirmation.ConfirmationAccountId = account.AccountId;
                emergencyConfirmation.IsConfirm = true;
                emergencyConfirmation.ConfirmationDate = DateTime.UtcNow.AddHours(7);

                if (account.Elderly != null && emergencyConfirmation.ElderlyId == account.Elderly.ElderlyId)
                {
                    emergencyConfirmation.Status = SD.EmergencyStatus.CANCELLED;
                }
                else
                {
                    emergencyConfirmation.Status = SD.EmergencyStatus.CONFIRMED;
                }

                var updateRs = await _unitOfWork.EmergencyConfirmationRepository.UpdateAsync(emergencyConfirmation);

                if (updateRs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG, "Confirm success");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, ex.Message);
            }
        }        
        
        public async Task<IBusinessResult> ExpiredEmergency(int emergencyId)
        
        {
            try
            {
                var emergencyConfirmation = await _unitOfWork.EmergencyConfirmationRepository.GetByIdAsync(emergencyId);

                if (emergencyConfirmation == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Emergency Confirmation does not exist.");
                }

                emergencyConfirmation.ConfirmationAccountId = null;
                emergencyConfirmation.IsConfirm = true;
                emergencyConfirmation.ConfirmationDate = DateTime.UtcNow.AddHours(7);

                emergencyConfirmation.Status = SD.EmergencyStatus.EXPIRED;

                var updateRs = await _unitOfWork.EmergencyConfirmationRepository.UpdateAsync(emergencyConfirmation);

                if (updateRs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG, "Emergency expired successfully!");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, ex.Message);
            }
        }
    }
}
