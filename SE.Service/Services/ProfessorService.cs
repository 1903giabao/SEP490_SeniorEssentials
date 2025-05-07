using AutoMapper;
using SE.Common.Enums;
using SE.Common.Request;
using SE.Common;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Common.Response.Professor;
using static Google.Cloud.Vision.V1.ProductSearchResults.Types;
using SE.Common.Request.Professor;
using SE.Common.DTO;
using Firebase.Auth;
using SE.Service.Helper;
using Microsoft.Identity.Client;
using SE.Common.Request.Subscription;
using Google.Api.Gax;
using TagLib.Ape;
using Org.BouncyCastle.Ocsp;
using SE.Common.Request.SE.Common.Request;
using Google.Cloud.Firestore;
using System.Text.RegularExpressions;
using CloudinaryDotNet;
using System.Diagnostics;

namespace SE.Service.Services
{
    public interface IProfessorService
    {
        Task<IBusinessResult> GetNumberOfMeetingLeftByElderly(int elderlyId);
        Task<IBusinessResult> GetListElderlyByProfessorId(int professorId);
        Task<IBusinessResult> AddProfessorToSubscriptionByElderly(AddProfessorToSubscriptionRequest req);
        Task<IBusinessResult> GetAllProfessor();
        Task<IBusinessResult> GetProfessorDetail(int professorId);
        Task<IBusinessResult> GetTimeSlot(int professorId, DateTime date);
        Task<IBusinessResult> GetFilteredProfessors(FilterProfessorRequest request);
        Task<IBusinessResult> GetProfessorSchedule(int accountId, string type);
        Task<IBusinessResult> GetReportInAppointment(int appointmentId);
        Task<IBusinessResult> GetProfessorDetailOfElderly(int elderlyId);
        Task<IBusinessResult> GetProfessorDetailByAccountId(int accountId);
        Task<IBusinessResult> UpdateProfessorInfor(UpdateProfessorRequest req);
        Task<IBusinessResult> GetScheduleOfElderlyByProfessorId(int professorAccountId, string type);
        Task<IBusinessResult> GetProfessorWeeklyTimeSlots(int accountId);
        Task<IBusinessResult> GetProfessorScheduleInProfessor(int professorId);
        Task<IBusinessResult> BookProfessorAppointment(BookProfessorAppointmentRequest req);
        Task<IBusinessResult> CreateAppointmentReport(CreateReportRequest request);
        Task<IBusinessResult> GiveProfessorFeedbackByAccount(GiveProfessorFeedbackByAccountVM request);
        Task<IBusinessResult> GetAllRatingsByProfessorId(int accountId);
        Task<IBusinessResult> ConfirmMeeting(int appointmentId, List<int> participantAccountIds);
        Task<IBusinessResult> CancelMeeting(int appointmentId, int accountId);

        Task<IBusinessResult> GetAllAppointment();

    }

    public class ProfessorService : IProfessorService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IGroupService _groupService;
        private readonly FirestoreDb _firestoreDb;
        private readonly INotificationService _notificationService;

        public ProfessorService(UnitOfWork unitOfWork, IMapper mapper, IGroupService groupService, FirestoreDb firestoreDb, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _groupService = groupService;
            _firestoreDb = firestoreDb;
            _notificationService = notificationService;
        }

        public async Task<IBusinessResult> GetAllAppointment()
        {
            try
            {
                var appointments =await _unitOfWork.ProfessorAppointmentRepository.GetAllIncludeSub();
                var rs = new List<GetAllAppointmentResponse>();

                foreach (var appointment in appointments)
                {
                    var newRs = new GetAllAppointmentResponse();
                    newRs.ProAvatar = appointment.UserSubscription.Professor.Account.Avatar;
                    newRs.ProName = appointment.UserSubscription.Professor.Account.FullName;
                    newRs.ElderlyAvatar = appointment.Elderly.Account.Avatar;
                    newRs.ElderlyFullName = appointment.Elderly.Account.FullName;
                    newRs.DateOfAppointment = appointment.AppointmentTime.ToString("dd-MM-yyyy");
                    newRs.TimeOfAppointment =$"{appointment.StartTime} - {appointment.EndTime}";
                    newRs.ReasonOfMeeting = appointment.Description;
                    newRs.Status = appointment.Status;
                    newRs.ProEmail = appointment.UserSubscription.Professor.Account.Email;
                    newRs.ElderlyEmail = appointment.Elderly.Account.Email;
                    rs.Add(newRs);
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, rs.OrderByDescending(a=>a.DateOfAppointment));

            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }

        public async Task<IBusinessResult> CancelMeeting(int appointmentId, int accountId)
        {
            try
            {
                var appointment = await _unitOfWork.ProfessorAppointmentRepository.GetByIdAsync(appointmentId);
                if (appointment == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Appointment not found.");
                }
                var getSubcription = _unitOfWork.SubscriptionRepository.FindByCondition(s => s.NumberOfMeeting == 1 && s.ValidityPeriod ==0 && s.Status=="Active").FirstOrDefault();

                var getUserSub =await _unitOfWork.ProfessorAppointmentRepository.GetUserSubcriptionByAppointmentAsync(appointmentId);

                if (getUserSub.UserSubscription.Booking.Subscription.SubscriptionId == getSubcription.SubscriptionId)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, $"Không thể hủy lịch gói đăng kí {getSubcription.Name}.");
                }
                // Kiểm tra thời gian hủy có trước 6 tiếng so với thời gian hẹn không
                var currentTime = DateTime.UtcNow.AddHours(7);
                var timeDifference = appointment.AppointmentTime - currentTime;

                if (timeDifference.TotalHours < 6)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG,
                        "Chỉ có thể hủy lịch trước 6 tiếng");
                }

                // Kiểm tra nếu cuộc hẹn đã bị hủy rồi
                if (appointment.Status == SD.ProfessorAppointmentStatus.CANCELLED)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Appointment already cancelled.");
                }

                appointment.Status = SD.ProfessorAppointmentStatus.CANCELLED;
                var rs = await _unitOfWork.ProfessorAppointmentRepository.UpdateAsync(appointment);


                var getTokenProfessor = _unitOfWork.AccountRepository.FindByCondition(a => a.AccountId == getUserSub.UserSubscription.Professor.AccountId).FirstOrDefault();
                var getTokenElderly = _unitOfWork.AccountRepository.FindByCondition(a => a.AccountId == getUserSub.Elderly.AccountId).FirstOrDefault();
                var getTokenFamilyMember = _unitOfWork.AccountRepository.FindByCondition(a => a.AccountId == getUserSub.UserSubscription.Booking.AccountId).FirstOrDefault();



                var checkAccount = _unitOfWork.AccountRepository.FindByCondition(a => a.AccountId == accountId).FirstOrDefault();

                //send professor noti 
                if (checkAccount.RoleId != 4)
                {
                    if ( getTokenProfessor != null && getTokenProfessor.DeviceToken != "string")
                    {
                        await _notificationService.SendNotification(
                                    getTokenProfessor.DeviceToken,
                                    "Hủy Lịch Khám",
                                    $"{checkAccount.FullName} đã hủy lịch hẹn vào lúc {appointment.AppointmentTime.ToString("HH:mm")} ngày {appointment.AppointmentTime.ToString("dd-MM-yyyy")}");

                        var newNotification = new Notification
                        {
                            NotificationType = "Hủy Lịch Khám",
                            AccountId = getTokenProfessor.AccountId,
                            Status = SD.NotificationStatus.SEND,
                            Title = "Hủy Lịch Khám",
                            Message = $"{checkAccount.FullName} đã hủy lịch hẹn vào lúc {appointment.AppointmentTime.ToString("HH:mm")} ngày {appointment.AppointmentTime.ToString("dd-MM-yyyy")}",
                            CreatedDate = System.DateTime.UtcNow.AddHours(7)
                        };

                        await _unitOfWork.NotificationRepository.CreateAsync(newNotification);
                    }
                }

                // send elderly noti

                if (checkAccount.RoleId != 2)
                {
                    if (getTokenElderly != null && getTokenElderly.DeviceToken != "string")
                    {
                        await _notificationService.SendNotification(
                                    getTokenElderly.DeviceToken,
                                    "Hủy Lịch Khám",
                                    $"{checkAccount.FullName} đã hủy lịch hẹn vào lúc {appointment.AppointmentTime.ToString("HH:mm")} ngày {appointment.AppointmentTime.ToString("dd-MM-yyyy")}");

                        var newNotification = new Notification
                        {
                            NotificationType = "Hủy Lịch Khám",
                            AccountId = getTokenElderly.AccountId,
                            Status = SD.NotificationStatus.SEND,
                            Title = "Hủy Lịch Khám",
                            Message = $"{checkAccount.FullName} đã hủy lịch hẹn vào lúc {appointment.AppointmentTime.ToString("HH:mm")} ngày {appointment.AppointmentTime.ToString("dd-MM-yyyy")}",
                            CreatedDate = System.DateTime.UtcNow.AddHours(7)
                        };

                        await _unitOfWork.NotificationRepository.CreateAsync(newNotification);
                    }
                }

                // send family member noti
                if (checkAccount.RoleId != 3)
                {
                    if (getTokenFamilyMember != null && getTokenFamilyMember.DeviceToken != "string")
                    {
                        await _notificationService.SendNotification(
                                    getTokenFamilyMember.DeviceToken,
                                    "Hủy Lịch Khám",
                                    $"{checkAccount.FullName} đã hủy lịch hẹn vào lúc {appointment.AppointmentTime.ToString("HH:mm")} ngày {appointment.AppointmentTime.ToString("dd-MM-yyyy")}");

                        var newNotification = new Notification
                        {
                            NotificationType = "Hủy Lịch Khám",
                            AccountId = getTokenFamilyMember.AccountId,
                            Status = SD.NotificationStatus.SEND,
                            Title = "Hủy Lịch Khám",
                            Message = $"{checkAccount.FullName} đã hủy lịch hẹn vào lúc {appointment.AppointmentTime.ToString("HH:mm")} ngày {appointment.AppointmentTime.ToString("dd-MM-yyyy")}",
                            CreatedDate = System.DateTime.UtcNow.AddHours(7)
                        };

                        await _unitOfWork.NotificationRepository.CreateAsync(newNotification);
                    }
                }

                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Failed to cancel");
                }
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, "Cancel successfully");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }

        public async Task<IBusinessResult> ConfirmMeeting(int appointmentId, List<int> participantAccountIds)
        {
            try
            {
                // 1. Lấy appointment với đầy đủ thông tin liên quan
                var appointment = await _unitOfWork.ProfessorAppointmentRepository
                    .GetAppointmentWithParticipantsAsync(appointmentId);

                if (appointment == null)
                    return new BusinessResult(Const.FAIL_READ, "Appointment not found");

                // 2. Xác minh thông tin Elderly
                var elderlyAccountId = appointment.Elderly?.Account?.AccountId;
                if (elderlyAccountId == null || elderlyAccountId == 0)
                    return new BusinessResult(Const.FAIL_READ, "Invalid Elderly information");

                // 3. Xác minh thông tin Professor
                var professorAccountId = appointment.UserSubscription?.Professor?.Account?.AccountId;
                if (professorAccountId == null || professorAccountId == 0)
                    return new BusinessResult(Const.FAIL_READ, "Invalid Professor information");

                // 4. Kiểm tra danh tính thực sự của người tham gia
                bool isRealElderly = participantAccountIds.Contains(elderlyAccountId.Value);
                bool isRealProfessor = participantAccountIds.Contains(professorAccountId.Value);

                if (!isRealElderly || !isRealProfessor)
                {
                    var errorMsg = new StringBuilder("Missing required participants:");
                    if (!isRealElderly) errorMsg.Append(" Elderly");
                    if (!isRealProfessor) errorMsg.Append(" Professor");
                    return new BusinessResult(2, errorMsg.ToString());
                }
                // 6. Cập nhật trạng thái
                appointment.Status = SD.ProfessorAppointmentStatus.JOINED;
                var updateResult = await _unitOfWork.ProfessorAppointmentRepository.UpdateAsync(appointment);

                if (updateResult < 1)
                    return new BusinessResult(Const.FAIL_UPDATE, "Failed to update appointment status");

                return new BusinessResult(Const.SUCCESS_UPDATE, "Meeting confirmed successfully");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ,Const.FAIL_READ_MSG,ex.Message);
            }
        }
        public async Task<IBusinessResult> GetNumberOfMeetingLeftByElderly(int elderlyId)
        {
            try
            {
                var elderlyAccount = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(elderlyId);

                if (elderlyAccount == null || elderlyAccount.RoleId != 2)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var bookings = _unitOfWork.BookingRepository
                                .FindByCondition(b => b.ElderlyId == elderlyAccount.Elderly.ElderlyId && b.Status.Equals(SD.BookingStatus.PAID))
                                .Select(b => b.BookingId)
                                .ToList();

                if (!bookings.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Bookings of elderly not found.");
                }

                var userSubscription = await _unitOfWork.UserServiceRepository.GetUserSubscriptionByBookingIdAsync(bookings, SD.UserSubscriptionStatus.AVAILABLE);

                if (userSubscription?.Booking == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Booking details not found.");
                }

                var result = userSubscription.NumberOfMeetingLeft;

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetListElderlyByProfessorId(int professorId)
        {
            try
            {
                var getProfessorInfor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(professorId);

                if (getProfessorInfor == null || getProfessorInfor.RoleId != 4)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor does not exist!");
                }

                var listUserElderly = await _unitOfWork.UserServiceRepository.GetListElderlyByProfessorId(getProfessorInfor.Professor.ProfessorId);

                var result = _mapper.Map<List<AccountElderlyDTO>>(listUserElderly);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateAppointmentReport(CreateReportRequest request)
        {
            try
            {
                var getAppointment = await _unitOfWork.ProfessorAppointmentRepository.GetByProfessorAppointmentAsync(request.ProfessorAppointmentId);
                if (getAppointment == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Cannot find appointment");
                }

                getAppointment.Content = request.Content;
                getAppointment.Solution = request.Solution;

                var rs = await _unitOfWork.ProfessorAppointmentRepository.UpdateAsync(getAppointment);
                if (rs < 1)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Cannot create report");

                }

                var familyMember = await _unitOfWork.AccountRepository.GetAccountAsync(getAppointment.UserSubscription.Booking.AccountId);
                var elderly = await _unitOfWork.AccountRepository.GetAccountAsync(getAppointment.UserSubscription.Booking.Elderly.AccountId);

                if (familyMember != null && elderly != null)
                {
                    if (!string.IsNullOrEmpty(familyMember.DeviceToken) && familyMember.DeviceToken != "string")
                    {
                        // Send notification
                        await _notificationService.SendNotification(
                            familyMember.DeviceToken,
                            "Báo cáo tư vấn bác sĩ",
                            $"Bạn đã nhận được báo cáo về buổi tư vấn của {elderly.FullName} và bác sĩ.");

                        var newNotification = new Data.Models.Notification
                        {
                            NotificationType = "Báo cáo tư vấn bác sĩ",
                            AccountId = familyMember.AccountId,
                            Status = SD.GeneralStatus.ACTIVE,
                            Title = "Báo cáo tư vấn bác sĩ",
                            Message = $"Bạn đã nhận được báo cáo về buổi tư vấn của {elderly.FullName} và bác sĩ.",
                            CreatedDate = System.DateTime.UtcNow.AddHours(7),
                        };

                        await _unitOfWork.NotificationRepository.CreateAsync(newNotification);
                    }
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, "Created report succesfully!");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllRatingsByProfessorId(int accountId)
        {
            try
            {
                var list = new List<GetProfessorRatingVM>();
                var professorId = _unitOfWork.ProfessorRepository.FindByCondition(p=>p.AccountId == accountId).FirstOrDefault();
                var ratings = _unitOfWork.ProfessorRatingRepository
                    .FindByCondition(r => r.ProfessorId == professorId.ProfessorId && r.Status == SD.GeneralStatus.ACTIVE)
                    .ToList();

                foreach (var rating in ratings)
                {
                    var rs = new GetProfessorRatingVM();
                    var getAppointment = _unitOfWork.ProfessorAppointmentRepository.GetById((int)rating.ProfessorAppointmentId);
                    var getElderly = await _unitOfWork.ElderlyRepository.GetAccountByElderlyId(rating.ElderlyId);
                    var getCreatedBy = _unitOfWork.AccountRepository.FindByCondition(a=>a.FullName == rating.CreatedBy).FirstOrDefault();
                    rs.CreatedBy = rating.CreatedBy;
                    rs.Content = rating.RatingComment;
                    rs.DateOfAppointment = getAppointment.AppointmentTime.ToString("dd-MM-yyyy");
                    rs.Avatar = getElderly.Account.Avatar;
                    rs.TimeOfAppointment = $"{getAppointment.StartTime} - {getAppointment.EndTime}";
                    rs.Star = (int)rating.Rating;
                    rs.ReasonOfMeeting = getAppointment.Description;
                    rs.FullName = getElderly.Account.FullName;
                    rs.CreatedByAvatar = getCreatedBy.Avatar;
                    list.Add(rs);
                }

                var result = new
                {
                    TotalRating = ratings.Count(),
                    ListOfRating = list
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<IBusinessResult> AddProfessorToSubscriptionByElderly(AddProfessorToSubscriptionRequest req)
        {
            try
            {
                var getProfessorInfor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(req.ProfessorId);

                if (getProfessorInfor == null || getProfessorInfor.RoleId != 4)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor does not exist!");
                }

                var elderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(req.ElderlyId);

                if (elderly == null || elderly.RoleId != 2)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }                               

                var bookings = _unitOfWork.BookingRepository.FindByCondition(b => b.ElderlyId == elderly.Elderly.ElderlyId && b.Status.Equals(SD.BookingStatus.PAID))
                                                            .Select(b => b.BookingId).ToList();

                if (bookings.Any())
                {
                    var userSubscription = await _unitOfWork.UserServiceRepository.GetUserSubscriptionByBookingIdAsync(bookings, "Đang khả dụng");

                    if (userSubscription.ProfessorId != null)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly already had professor!");
                    }

                    userSubscription.ProfessorId = getProfessorInfor.Professor.ProfessorId;

                    var updateRs = await _unitOfWork.UserServiceRepository.UpdateAsync(userSubscription);

                    if (updateRs < 1)
                    {
                        return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                    }

                    var expiredUserSubscription = await _unitOfWork.UserServiceRepository.GetUserSubscriptionByElderlyAndProfessorAsync(getProfessorInfor.AccountId, userSubscription.Booking.AccountId, userSubscription.Booking.Elderly.AccountId, SD.UserSubscriptionStatus.EXPIRED);

                    if (expiredUserSubscription == null)
                    {
                        var groupMembers = new List<GroupMemberRequest>
                        {
                            new GroupMemberRequest
                            {
                                AccountId = userSubscription.Booking.AccountId,
                                IsCreator = true
                            },
                            new GroupMemberRequest
                            {
                                AccountId = getProfessorInfor.AccountId,
                                IsCreator = false
                            },
                            new GroupMemberRequest
                            {
                                AccountId = elderly.AccountId,
                                IsCreator = false
                            }
                        };

                        var currentTime = DateTime.UtcNow.AddHours(7);
                        var groupId = Guid.NewGuid().ToString();

                        if (groupMembers.Count > 2)
                        {
                            DocumentReference groupChatRoomRef = _firestoreDb.Collection("ChatRooms").Document(groupId);

                            var groupChatRoomData = new Dictionary<string, object>
                        {
                        { "CreatedAt", currentTime.ToString("dd-MM-yyyy HH:mm") },
                        { "IsGroupChat", true },
                        { "IsProfessorChat", true },
                        { "IsDisabled", false },
                        { "RoomName", "Bác sĩ " + getProfessorInfor.FullName + ", " + elderly.FullName + ", " + userSubscription.Booking.Account.FullName},
                        { "RoomAvatar", "https://icons.veryicon.com/png/o/miscellaneous/standard/avatar-15.png" },
                        { "SenderId", 0 },
                        { "LastMessage", "" },
                        { "SentDate", currentTime.ToString("dd-MM-yyyy") },
                        { "SentTime", currentTime.ToString("HH:mm") },
                        { "SentDateTime", currentTime.ToString("dd-MM-yyyy HH:mm") },
                            {
                                "MemberIds", groupMembers
                                    .DistinctBy(m => m.AccountId)
                                    .ToDictionary(m => m.AccountId.ToString(), m => (object)true)
                            }
                        };

                            await groupChatRoomRef.SetAsync(groupChatRoomData);

                            foreach (var member in groupMembers)
                            {
                                await groupChatRoomRef.Collection("Members").Document(member.AccountId.ToString()).SetAsync(new { IsCreator = member.IsCreator });
                            }
                        }

                        var onlineMembersRef = _firestoreDb.Collection("OnlineMembers");

                        foreach (var member in groupMembers)
                        {
                            var onlineMemberData = new Dictionary<string, object>
                            {
                                { "IsOnline", true }
                            };

                            await onlineMembersRef.Document(member.AccountId.ToString()).SetAsync(onlineMemberData);
                        }

                        userSubscription.ProfessorGroupChatId = groupId;
                    }
                    else
                    {
                        userSubscription.ProfessorGroupChatId = expiredUserSubscription.ProfessorGroupChatId;


                        var groupRef = _firestoreDb.Collection("ChatRooms").Document(expiredUserSubscription.ProfessorGroupChatId);
                        var groupDoc = await groupRef.GetSnapshotAsync();

                        if (groupDoc.Exists)
                        {
                            var updateData = new Dictionary<string, object>
                                {
                                    { "IsDisabled", true }
                                };

                            await groupRef.UpdateAsync(updateData);
                        }
                    }

                    var updateRs1 = await _unitOfWork.UserServiceRepository.UpdateAsync(userSubscription);

                    if (updateRs1 < 1)
                    {
                        return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                    }
                     if (getProfessorInfor.DeviceToken != null && getProfessorInfor.DeviceToken != "string")
                    {
                        var title = "Bạn đã được chọn làm bác sĩ tư vấn";
                        var body = $"{elderly.FullName} đã chọn bạn làm bác sĩ tư vấn.";

                        await _notificationService.SendNotification(getProfessorInfor.DeviceToken, title, body);

                        var newNoti = new Notification
                        {
                            Title = title,
                            AccountId = getProfessorInfor.AccountId,
                            CreatedDate = DateTime.UtcNow.AddHours(7),
                            Message = body,
                            NotificationType = title,
                            Status = SD.NotificationStatus.SEND
                        };
                        await _unitOfWork.NotificationRepository.CreateAsync(newNoti);
                    }
                    return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG);
                }

                return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }
        public async Task<IBusinessResult> GiveProfessorFeedbackByAccount(GiveProfessorFeedbackByAccountVM request)
        {
            try
            {
                // Get the appointment by ID
                var appointment = _unitOfWork.ProfessorAppointmentRepository
                    .FindByCondition(a => a.ProfessorAppointmentId == request.AppointmentId)
                    .FirstOrDefault();
                var rs = 0;
                if (appointment == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Cuộc hẹn không tồn tại!");
                }

                var getUseSub = _unitOfWork.UserServiceRepository.FindByCondition(us => us.UserSubscriptionId == appointment.UserSubscriptionId).FirstOrDefault();

                var professorId = getUseSub.ProfessorId;
                var elderlyId = appointment.ElderlyId;

                // Create feedback
                var feedback = new ProfessorRating
                {
                    ProfessorId = (int)professorId,
                    ElderlyId = elderlyId,
                    RatingComment = request.Content,
                    Rating = request.Star,
                    Status = SD.GeneralStatus.ACTIVE,
                    CreatedBy = request.CreatedBy,
                    ProfessorAppointmentId = request.AppointmentId
                };

                await _unitOfWork.ProfessorRatingRepository.CreateAsync(feedback);

                // Update professor average rating
                var allRatings = _unitOfWork.ProfessorRatingRepository
                    .FindByCondition(r => r.ProfessorId == professorId && r.Status == SD.GeneralStatus.ACTIVE)
                    .ToList();

                if (allRatings.Any())
                {
                    var averageRating = allRatings.Average(r => r.Rating);
                    var professor = _unitOfWork.ProfessorRepository
                        .FindByCondition(p => p.ProfessorId == professorId)
                        .FirstOrDefault();

                    if (professor != null)
                    {
                        professor.Rating = averageRating;
                        rs = await _unitOfWork.ProfessorRepository.UpdateAsync(professor);
                    }
                }


                if (rs > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, "Create succesffuly");
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Fail to create");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<IBusinessResult> GetAllProfessor()
        {
            try
            {
                var getAllProfessor = _unitOfWork.ProfessorRepository.FindByCondition(p => p.Status.Equals(SD.GeneralStatus.ACTIVE));
                var result = new List<GetAllProfessorReponse>();
                var currentDate = DateTime.UtcNow.AddHours(7);
                var currentDayOfWeek = currentDate.DayOfWeek;

                foreach (var item in getAllProfessor)
                {
                    var professor = new GetAllProfessorReponse();
                    var professorInfor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(item.AccountId);
                    var rating = _unitOfWork.ProfessorRatingRepository.FindByCondition(p=>p.ProfessorId == item.ProfessorId).Count();

                    professor.ProfessorAvatar = professorInfor.Avatar;
                    professor.ProfessorName = professorInfor.FullName;
                    professor.ProfessorId = professorInfor.Professor.ProfessorId;
                    professor.Major = professorInfor.Professor.Knowledge;
                    professor.Rating = (decimal) professorInfor.Professor.Rating;
                    professor.AccountId = professorInfor.AccountId;
                    professor.TotalRating = rating;

                    result.Add(professor);

                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IBusinessResult> GetProfessorDetail(int professorId)
        {
            try
            {
                var getProfessor = await _unitOfWork.ProfessorRepository.GetAccountByProfessorId(professorId);

                var getProfessorInfor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(getProfessor.AccountId);

                var professor = _mapper.Map<GetProfessorDetail>(getProfessor);

                professor.ProfessorId = professorId;
                professor.FullName = getProfessorInfor.FullName;
                professor.Avatar = getProfessorInfor.Avatar;

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, professor);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IBusinessResult> GetProfessorDetailByAccountId(int accountId)
        {
            try
            {
                var getProfessorInfor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(accountId);
                if (getProfessorInfor == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor does not existed!");
                }
                var professor = _mapper.Map<GetProfessorDetail>(getProfessorInfor.Professor);

                professor.ProfessorId = getProfessorInfor.Professor.ProfessorId;
                professor.FullName = getProfessorInfor.FullName;
                professor.Avatar = getProfessorInfor.Avatar;

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, professor);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetTimeSlot(int professorId, DateTime date)
        {
            try
            {
                // Lấy thông tin professor để đảm bảo tồn tại
                var professor = await _unitOfWork.ProfessorRepository.GetByIdAsync(professorId);
                if (professor == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor does not exist!");
                }

                // Lấy tất cả các cuộc hẹn của professor trong ngày cụ thể
                var appointments = await _unitOfWork.ProfessorAppointmentRepository
                    .GetAppointmentsByProfessorAndDateAsync(professorId, date);

                var response = new ViewProfessorScheduleResponse
                {
                    Date = date.ToString("dd-MM-yyyy"),
                    TimeEachSlots = new List<TimeEachSlot>()
                };

                // Xác định thời gian làm việc trong ngày (7h - 19h)
                TimeOnly workStart = new TimeOnly(7, 0);
                TimeOnly workEnd = new TimeOnly(19, 0);

                // Tạo danh sách các khoảng thời gian rảnh
                List<(TimeOnly Start, TimeOnly End)> availableSlots = new List<(TimeOnly, TimeOnly)>();

                // Thời gian bắt đầu kiểm tra
                TimeOnly currentStart = workStart;

                // Lọc các appointments có thời gian hợp lệ
                var validAppointments = appointments
                    .Where(a => a.StartTime.HasValue && a.EndTime.HasValue)
                    .OrderBy(a => a.StartTime)
                    .ToList();

                foreach (var appointment in validAppointments)
                {
                    var apptStart = appointment.StartTime.Value;
                    var apptEnd = appointment.EndTime.Value;

                    // Nếu có khoảng trống trước cuộc hẹn
                    if (currentStart < apptStart)
                    {
                        availableSlots.Add((currentStart, apptStart));
                    }

                    // Cập nhật thời gian bắt đầu kiểm tra tiếp theo
                    currentStart = apptEnd;
                }

                // Kiểm tra khoảng thời gian sau cuộc hẹn cuối cùng
                if (currentStart < workEnd)
                {
                    availableSlots.Add((currentStart, workEnd));
                }

                // Chia các khoảng thời gian rảnh thành các slot 1 tiếng
                foreach (var slot in availableSlots)
                {
                    TimeOnly slotStart = slot.Start;
                    TimeOnly slotEnd = slot.End;

                    while (slotStart.AddHours(1) <= slotEnd)
                    {
                        response.TimeEachSlots.Add(new TimeEachSlot
                        {
                            StartTime = slotStart.ToString("HH:mm"),
                            EndTime = slotStart.AddHours(1).ToString("HH:mm")
                        });
                        slotStart = slotStart.AddHours(1);
                    }
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, response);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }
        public async Task<IBusinessResult> GetFilteredProfessors(FilterProfessorRequest request)
        {
            try
            {
                var getAllProfessor = _unitOfWork.ProfessorRepository.GetAll();
                var result = new List<GetAllProfessorReponse>();
                var currentDate = DateTime.Now;
                var currentDayOfWeek = currentDate.DayOfWeek;

                foreach (var item in getAllProfessor)
                {
                    var professor = new GetAllProfessorReponse();
                    var professorInfor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(item.AccountId);

                    professor.ProfessorName = professorInfor.FullName;
                    professor.ProfessorAvatar = professorInfor.Avatar;
                    professor.ProfessorId = professorInfor.Professor.ProfessorId;
                    professor.Major = professorInfor.Professor.Knowledge;
                    professor.Rating = professorInfor.Professor == null ? 0 : (decimal)professorInfor.Professor.Rating;
                    professor.TotalRating = 0;

                    
                    result.Add(professor);
                }
                // Apply filters
                if (!string.IsNullOrEmpty(request.NameSortOrder))
                {
                    result = request.NameSortOrder.ToLower() == "asc"
                        ? result.OrderBy(p => p.ProfessorName).ToList()
                        : result.OrderByDescending(p => p.ProfessorName).ToList();
                }

                if (request.DayOfWeekFilter != null && request.DayOfWeekFilter.Any())
                {
                    result = result
                        .Where(p => p.DateTime != null && request.DayOfWeekFilter.Contains(p.DateTime.Split('/')[0], StringComparer.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!string.IsNullOrEmpty(request.RatingSortOrder))
                {
                    result = request.RatingSortOrder.ToLower() == "asc"
                        ? result.OrderBy(p => p.Rating).ToList()
                        : result.OrderByDescending(p => p.Rating).ToList();
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IBusinessResult> GetProfessorSchedule(int accountId, string type)
        {
            try
            {
                var getElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(accountId);
                if (getElderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Account does not exist!");
                }
                var appointments = await _unitOfWork.ProfessorAppointmentRepository
                    .GetByElderlyIdAsync(getElderly.Elderly.ElderlyId, type);

                var result = new List<GetProfessorScheduleOfElderly>();

                foreach (var appointment in appointments)
                {
                    var peopleInAppointment = new List<PeopleOfSchedule>();

                    var professor = await _unitOfWork.ProfessorRepository
                        .GetByIdAsync(appointment.UserSubscription.ProfessorId);

                    if (professor == null)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor does not exist!");

                    }
                    var professorAccount = await _unitOfWork.AccountRepository
                        .GetByIdAsync(professor.AccountId);

                    if (professorAccount == null)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Account of professor does not exist!");

                    }

                    // Check if there is a report (Content in ProfessorAppointment)
                    bool isReport = !string.IsNullOrEmpty(appointment.Content);

                    // Check if feedback exists for this appointment
                    bool isFeedback = _unitOfWork.ProfessorRatingRepository
                        .FindByCondition(r => r.ProfessorAppointmentId == appointment.ProfessorAppointmentId)
                        .Any();

                    var schedule = new GetProfessorScheduleOfElderly
                    {
                        ProfessorName = professorAccount.FullName,
                        DateTime = $"{appointment.AppointmentTime:dd/MM/yyyy HH:mm}",
                        Status = appointment.Status,
                        ProfessorAppointmentId = appointment.ProfessorAppointmentId,
                        ProfessorAvatar = professorAccount.Avatar,
                        IsOnline = (bool)appointment.IsOnline,
                        IsReport = isReport,   // Set IsReport
                        IsFeedback = isFeedback,  // Set IsFeedback
                        People = new List<PeopleOfSchedule>()
                    };

                    if (schedule.Status == SD.ProfessorAppointmentStatus.NOTYET)
                    {
                        // Add professor
                        peopleInAppointment.Add(new PeopleOfSchedule
                        {
                            Id = professorAccount.AccountId,
                            Name = professorAccount.FullName
                        });

                        // Add elderly
                        var elderlyId = await _unitOfWork.ElderlyRepository.GetAccountByElderlyId(appointment.ElderlyId);
                        peopleInAppointment.Add(new PeopleOfSchedule
                        {
                            Id = elderlyId.Account.AccountId,
                            Name = elderlyId.Account.FullName
                        });

                        // Add family members
                        var familyMemberAccountIds = await GetAllFamilyMemberByElderlyId(elderlyId.AccountId);
                        var familyMembers = _unitOfWork.AccountRepository.GetAll()
                            .Where(a => familyMemberAccountIds.Contains(a.AccountId))
                            .Select(a => new PeopleOfSchedule
                            {
                                Id = a.AccountId,
                                Name = a.FullName
                            })
                            .ToList();

                        peopleInAppointment.AddRange(familyMembers);

                        schedule.People.AddRange(peopleInAppointment);
                    }

                    result.Add(schedule);
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result.OrderBy(p => p.Status));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IBusinessResult> GetProfessorScheduleInProfessor(int professorId)
        {
            try
            {
                // Kiểm tra account professor tồn tại và có role phù hợp
                var accountProfessor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(professorId);

                if (accountProfessor == null || accountProfessor.RoleId != 4)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor doesn't exist!");
                }

                // Lấy thông tin professor
                var professor =  _unitOfWork.ProfessorRepository
                    .FindByCondition(p => p.ProfessorId == accountProfessor.Professor.ProfessorId)
                    .FirstOrDefault();

                if (professor == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor doesn't exist!");
                }

                // Lấy tất cả các cuộc hẹn trong tuần hiện tại
                var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(7);

                var appointments = await _unitOfWork.ProfessorAppointmentRepository
                    .GetAppointmentsByProfessorInDateRangeAsync(professor.ProfessorId, startOfWeek, endOfWeek);

                // Chuẩn bị danh sách ngày trong tuần
                var daysOfWeek = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                var result = new List<GetScheduleOfProfessorVM>();

                // Xử lý từng ngày trong tuần
                foreach (var day in daysOfWeek)
                {
                    var dayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), day);
                    var currentDate = startOfWeek.AddDays((int)dayOfWeek);

                    var newDay = new GetScheduleOfProfessorVM
                    {
                        DayOfWeek = day,
                        Times = new List<Time>()
                    };

                    // Lấy các cuộc hẹn trong ngày
                    var dayAppointments = appointments
                        .Where(a => a.AppointmentTime.Date == currentDate.Date)
                        .OrderBy(a => a.StartTime)
                        .ToList();

                    // Xác định thời gian làm việc (7h - 19h)
                    TimeOnly workStart = new TimeOnly(7, 0);
                    TimeOnly workEnd = new TimeOnly(19, 0);
                    TimeOnly currentStart = workStart;

                    // Tìm các khoảng thời gian rảnh
                    foreach (var appointment in dayAppointments)
                    {
                        if (appointment.StartTime.HasValue && appointment.EndTime.HasValue)
                        {
                            var apptStart = appointment.StartTime.Value;
                            var apptEnd = appointment.EndTime.Value;

                            // Thêm khoảng thời gian trước cuộc hẹn
                            if (currentStart < apptStart)
                            {
                                newDay.Times.Add(new Time
                                {
                                    Start = currentStart.ToString("HH:mm"),
                                    End = apptStart.ToString("HH:mm")
                                });
                            }

                            currentStart = apptEnd;
                        }
                    }

                    // Thêm khoảng thời gian sau cuộc hẹn cuối cùng
                    if (currentStart < workEnd)
                    {
                        newDay.Times.Add(new Time
                        {
                            Start = currentStart.ToString("HH:mm"),
                            End = workEnd.ToString("HH:mm")
                        });
                    }

                    result.Add(newDay);
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }
        public async Task<List<int>> GetAllFamilyMemberByElderlyId(int userId)
        {

            var groupId = _unitOfWork.GroupMemberRepository.GetAll()
                .Where(gm => gm.AccountId == userId && gm.Status == SD.GeneralStatus.ACTIVE)
                .Select(gm => gm.GroupId)
                .FirstOrDefault();


            var group = await _unitOfWork.GroupRepository.GetByIdAsync(groupId);

            var groupMembers = _unitOfWork.GroupMemberRepository.GetAll()
                .Where(gm => gm.GroupId == groupId &&
                             gm.Status == SD.GeneralStatus.ACTIVE &&
                             gm.AccountId != userId)
                .Select(gm => gm.AccountId)
                .ToList();

            var users = _unitOfWork.AccountRepository.GetAll()
                     .Where(a => groupMembers.Contains(a.AccountId) && a.RoleId == 3)
                     .Select(a => a.AccountId)
                     .ToList();

            return users;
        }


        public async Task<IBusinessResult> GetProfessorDetailOfElderly(int elderlyId)
        {
            try
            {
                var getElderlyInfor = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(elderlyId);

                if (getElderlyInfor == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Account not found");
                }
                var getElderlyEntity = _unitOfWork.ElderlyRepository.FindByCondition(e => e.AccountId == elderlyId).FirstOrDefault();
                if (getElderlyEntity == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly not found!");

                }

                var findProfessorId = await _unitOfWork.UserServiceRepository.GetProfessorByElderlyId(getElderlyEntity.ElderlyId);
                if (findProfessorId == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor not found");

                }
                var getProfessor = await _unitOfWork.ProfessorRepository.GetAccountByProfessorId((int)findProfessorId.ProfessorId);

                var getProfessorInfor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(getProfessor.AccountId);


                // Map the professor details to the response model
                var professor = _mapper.Map<GetProfessorDetail>(getProfessor);

                // Set additional properties
                professor.ProfessorId = getProfessor.ProfessorId;
                professor.FullName = getProfessorInfor.FullName;
                professor.Avatar = getProfessorInfor.Avatar;
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, professor);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<IBusinessResult> GetReportInAppointment(int appointmentId)
        {
            try
            {
                var getAppointment = _unitOfWork.ProfessorAppointmentRepository.GetById(appointmentId);
                if (getAppointment == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Appointment not found");
                }
                var rs = new
                {
                    Content = (getAppointment.Content == null) ? "" : getAppointment.Content,
                    Solution = (getAppointment.Solution == null) ? "" : getAppointment.Solution
                };
                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, rs);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IBusinessResult> UpdateProfessorInfor(UpdateProfessorRequest req)
        {
            try
            {
                var getProfessorAccountInfor = await _unitOfWork.AccountRepository.GetProfessorByAccountIDAsync(req.AccountId);

                if (getProfessorAccountInfor == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Account does not exist");
                }

                var professor = await _unitOfWork.ProfessorRepository.GetByIdAsync(getProfessorAccountInfor.Professor.ProfessorId);

                if (professor == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor does not exist");
                }

                if (req.Avatar != null)
                {
                    var avatar = ("", "");

                    if (req.Avatar != null)
                    {
                        avatar = await CloudinaryHelper.UploadImageAsync(req.Avatar);
                    }

                    getProfessorAccountInfor.Avatar = avatar.Item2;

                    var updateAccountRs = await _unitOfWork.AccountRepository.UpdateAsync(getProfessorAccountInfor);

                    if (updateAccountRs < 1)
                    {
                        return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                    }
                }

                professor.Knowledge = req.Knowledge;
                professor.Achievement = req.Achievement;
                professor.ClinicAddress = req.ClinicAddress;
                professor.Career = req.Career;
                professor.ConsultationFee = req.ConsultationFee;
                professor.ExperienceYears = req.ExperienceYears;
                professor.Qualification = req.Qualification;
                professor.Specialization = req.Specialization;

                var updateRs = await _unitOfWork.ProfessorRepository.UpdateAsync(professor);

                if (updateRs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                var professorRs = _mapper.Map<GetProfessorDetail>(professor);

                professorRs.ProfessorId = getProfessorAccountInfor.Professor.ProfessorId;
                professorRs.FullName = getProfessorAccountInfor.FullName;
                professorRs.Avatar = getProfessorAccountInfor.Avatar;

                return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG, professorRs);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetScheduleOfElderlyByProfessorId(int professorAccountId, string type)
        {
            try
            {
                // Get the professor by account ID
                var professor = await _unitOfWork.ProfessorRepository
                    .GetByAccountIdAsync(professorAccountId);

                if (professor == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor does not exist!");
                }

                // Get all appointments for this professor with type filtering
                var appointments = await _unitOfWork.ProfessorAppointmentRepository
                    .GetByProfessorIdAsync(professor.ProfessorId, type);

                var result = new List<GetScheduleOfElderlyByProfessorVM>();

                foreach (var appointment in appointments)
                {
                    var elderly = await _unitOfWork.ElderlyRepository
                        .GetByIdAsync(appointment.ElderlyId);

                    if (elderly == null)
                    {
                        continue; // Skip if elderly not found
                    }

                    var elderlyAccount = await _unitOfWork.AccountRepository
                        .GetByIdAsync(elderly.AccountId);

                    if (elderlyAccount == null)
                    {
                        continue; // Skip if elderly account not found
                    }

                    // Check if there is a report (Content in ProfessorAppointment)
                    bool isReport = !string.IsNullOrEmpty(appointment.Content);

                    // Check if feedback exists for this appointment
                    bool isFeedback = _unitOfWork.ProfessorRatingRepository
                        .FindByCondition(r => r.ProfessorAppointmentId == appointment.ProfessorAppointmentId)
                        .Any();

                    var schedule = new GetScheduleOfElderlyByProfessorVM
                    {
                        ProfessorAppointmentId = appointment.ProfessorAppointmentId,
                        AccountId  = elderly.AccountId,
                        ElderlyId = elderly.ElderlyId,
                        ElderlyName = elderlyAccount.FullName,
                        Avatar = elderlyAccount.Avatar,
                        PhoneNumber = elderlyAccount.PhoneNumber,
                        DateTime = $"{appointment.AppointmentTime:dd/MM/yyyy HH:mm}",
                        Description = appointment.Description,
                        Status = appointment.Status,
                        IsOnline = (bool)appointment.IsOnline,
                        IsReport = isReport,  // Set IsReport
                        IsFeedback = isFeedback,  // Set IsFeedback
                        People = new List<PeopleOfScheduleVM>()
                    };

                    // Only add people if status is NOTYET (similar to first function)
                    if (appointment.Status == SD.ProfessorAppointmentStatus.NOTYET)
                    {
                        // Add professor
                        schedule.People.Add(new PeopleOfScheduleVM
                        {
                            Id = professorAccountId,
                            Name = (await _unitOfWork.AccountRepository.GetByIdAsync(professorAccountId))?.FullName
                        });

                        // Add elderly
                        schedule.People.Add(new PeopleOfScheduleVM
                        {
                            Id = elderly.AccountId,
                            Name = elderlyAccount.FullName
                        });

                        // Add family members
                        var familyMemberAccountIds = await GetAllFamilyMemberByElderlyId(elderly.AccountId);
                        var familyMembers = _unitOfWork.AccountRepository.GetAll()
                            .Where(a => familyMemberAccountIds.Contains(a.AccountId))
                            .Select(a => new PeopleOfScheduleVM
                            {
                                Id = a.AccountId,
                                Name = a.FullName
                            })
                            .ToList();

                        schedule.People.AddRange(familyMembers);
                    }

                    result.Add(schedule);
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result.OrderBy(p => p.Status));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<IBusinessResult> GetProfessorWeeklyTimeSlots(int accountId)
        {
            try
            {
                // Kiểm tra và lấy ProfessorId từ AccountId
                var professor =  _unitOfWork.ProfessorRepository
                    .FindByCondition(p => p.AccountId == accountId)
                    .FirstOrDefault();

                if (professor == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor not found");
                }

                // Tính toán tuần hiện tại (từ thứ 2 đến chủ nhật)
                var today = DateTime.UtcNow.AddHours(7);
                var daysFromMonday = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
                var monday = today.AddDays(-daysFromMonday);
                var sunday = monday.AddDays(6);

                // Lấy tất cả cuộc hẹn trong tuần
                var weeklyAppointments = await _unitOfWork.ProfessorAppointmentRepository
                    .GetByProfessorAndDateRangeAsync(professor.ProfessorId, monday, sunday);

                var result = new List<ViewProfessorScheduleResponse>();

                // Duyệt qua từng ngày trong tuần
                for (var date = monday; date <= sunday; date = date.AddDays(1))
                {
                    var dayAppointments = weeklyAppointments
                        .Where(a => a.AppointmentTime.Date == date.Date)
                        .OrderBy(a => a.StartTime)
                        .ToList();

                    var dayResult = new ViewProfessorScheduleResponse
                    {
                        Date = date.ToString("dddd dd-MM-yy"), // Ví dụ: "Monday 01-04-24"
                        TimeEachSlots = new List<TimeEachSlot>()
                    };

                    // Tạo danh sách tất cả các slot 1 tiếng trong ngày (7h-19h)
                    var allSlots = new List<(TimeOnly Start, TimeOnly End)>();
                    for (var hour = 7; hour < 19; hour++)
                    {
                        allSlots.Add((
                            new TimeOnly(hour, 0),
                            new TimeOnly(hour + 1, 0)
                        ));
                    }

                    // Lọc các slot trống (không bị cuộc hẹn nào chiếm)
                    foreach (var slot in allSlots)
                    {
                        bool isSlotAvailable = true;

                        foreach (var appointment in dayAppointments)
                        {
                            if (appointment.StartTime.HasValue && appointment.EndTime.HasValue)
                            {
                                var apptStart = appointment.StartTime.Value;
                                var apptEnd = appointment.EndTime.Value;

                                // Kiểm tra slot có bị trùng với cuộc hẹn không
                                if (!(slot.End <= apptStart || slot.Start >= apptEnd))
                                {
                                    isSlotAvailable = false;
                                    break;
                                }
                            }
                        }

                        if (isSlotAvailable)
                        {
                            dayResult.TimeEachSlots.Add(new TimeEachSlot
                            {
                                StartTime = slot.Start.ToString("HH:mm"),
                                EndTime = slot.End.ToString("HH:mm")
                            });
                        }
                    }

                    result.Add(dayResult);
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, ex.Message);
            }
        }
        public async Task<IBusinessResult> BookProfessorAppointment(BookProfessorAppointmentRequest req)
        {
            try
            {
                // 1. Validate Elderly account
                var accountElderly = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(req.ElderlyId);
                if (accountElderly == null || accountElderly.RoleId != 2)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly doesn't exist!");
                }

                // 2. Verify elderly exists
                var elderly = _unitOfWork.ElderlyRepository
                    .FindByCondition(p => p.ElderlyId == accountElderly.Elderly.ElderlyId)
                    .FirstOrDefault();
                if (elderly == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly doesn't exist!");
                }

                // 3. Check valid bookings
                var bookings = _unitOfWork.BookingRepository
                    .FindByCondition(b => b.ElderlyId == elderly.ElderlyId && b.Status.Equals(SD.BookingStatus.PAID))
                    .Select(b => b.BookingId)
                    .ToList();
                if (!bookings.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Bookings of elderly not found.");
                }

                var userSubscription = new UserSubscription();

                if (req.ProfessorId == null)
                {
                    userSubscription = await _unitOfWork.UserServiceRepository.GetUserSubscriptionByBookingIdAsync(bookings, SD.UserSubscriptionStatus.AVAILABLE);
                    if (userSubscription?.Booking == null)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Booking details not found.");
                    }

                    var professor = _unitOfWork.ProfessorRepository
                        .FindByCondition(p => p.ProfessorId == userSubscription.ProfessorId)
                        .FirstOrDefault();
                    if (professor == null)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Need to choose professor first!");
                    }
                }
                else
                {
                    userSubscription = await _unitOfWork.UserServiceRepository.GetAppointmentUserSubscriptionByBookingIdAsync(bookings, SD.UserSubscriptionStatus.ONETIME);
                    if (userSubscription?.Booking == null)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Booking details not found.");
                    }

                    var accountProfessor = await _unitOfWork.AccountRepository.GetAccountAsync((int)req.ProfessorId);

                    var professor = _unitOfWork.ProfessorRepository
                        .FindByCondition(p => p.ProfessorId == accountProfessor.Professor.ProfessorId)
                        .FirstOrDefault();

                    if (professor == null)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Professor doesn't exist!");
                    }

                    userSubscription.ProfessorId = professor.ProfessorId;
                    userSubscription.Status = SD.UserSubscriptionStatus.BOOKED;
                }

                // 6. Parse and validate appointment date/time
                if (!DateTime.TryParse(req.Day, out DateTime appointmentDate))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid date format!");
                }

                if (!TimeOnly.TryParse(req.StartTime, out TimeOnly startTime) ||
                    !TimeOnly.TryParse(req.EndTime, out TimeOnly endTime))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid time format!");
                }

                // New condition: Check if appointment is at least 15 minutes from now
                var appointmentDateTime = appointmentDate.Date.Add(startTime.ToTimeSpan());
                var currentDateTime = DateTime.UtcNow.AddHours(7);
                var timeUntilAppointment = appointmentDateTime - currentDateTime;

                if (timeUntilAppointment.TotalMinutes < 15)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Chỉ có thể đặt lịch trước 15 phút!");
                }

                // 7. Check if time slot is available (no overlapping appointments)
                var existingAppointments = await _unitOfWork.ProfessorAppointmentRepository
                    .GetByProfessorAndDateAsync((int)userSubscription.ProfessorId, appointmentDate.Date);

                bool isSlotAvailable = !existingAppointments.Any(a =>
                    (a.StartTime < endTime && a.EndTime > startTime));

                if (!isSlotAvailable)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Time slot is already booked!");
                }

                if (req.ProfessorId == null)
                {
                    if (DateTime.Compare(appointmentDateTime, (DateTime)userSubscription.EndDate) > 0)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Không thể đặt lịch vượt quá ngày hết hạn gói!");
                    }
                }

                if (userSubscription.NumberOfMeetingLeft == 0)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Hết số lần gặp bác sĩ!");
                }

                // 9. Create new appointment
                var professorAppointment = new ProfessorAppointment
                {
                    ElderlyId = elderly.ElderlyId,
                    UserSubscriptionId = userSubscription.UserSubscriptionId,
                    AppointmentTime = appointmentDateTime,
                    StartTime = startTime,
                    EndTime = endTime,
                    Description = req.Description ?? "Nothing",
                    CreatedDate = currentDateTime,
                    Status = SD.ProfessorAppointmentStatus.NOTYET,
                    IsOnline = true,
                    Content = null,
                    Solution = null
                };

                var createRs = await _unitOfWork.ProfessorAppointmentRepository.CreateAsync(professorAppointment);
                if (createRs < 1)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
                }

                userSubscription.NumberOfMeetingLeft--;

                var updateUserSubscriptionRs = await _unitOfWork.UserServiceRepository.UpdateAsync(userSubscription);

                if (updateUserSubscriptionRs < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                var professorNoti = await _unitOfWork.AccountRepository.GetAccountAsync((int)userSubscription.Professor.AccountId);

                if (!string.IsNullOrEmpty(professorNoti.DeviceToken) && professorNoti.DeviceToken != "string")
                {
                    // Send notification
                    await _notificationService.SendNotification(
                        professorNoti.DeviceToken,
                        "Đặt lịch hẹn tư vấn",
                        $"{accountElderly.FullName} đã đặt lịch hẹn tư vấn vào lúc {appointmentDateTime.ToString("HH:mm dd/MM/yyyy")}.");

                    var newNotification = new Data.Models.Notification
                    {
                        NotificationType = "Đặt lịch hẹn tư vấn",
                        AccountId = professorNoti.AccountId,
                        Status = SD.GeneralStatus.ACTIVE,
                        Title = "Đặt lịch hẹn tư vấn",
                        Message = $"{accountElderly.FullName} đã đặt lịch hẹn tư vấn vào lúc {appointmentDateTime.ToString("HH:mm dd/MM/yyyy")}.",
                        CreatedDate = System.DateTime.UtcNow.AddHours(7),
                    };

                    await _unitOfWork.NotificationRepository.CreateAsync(newNotification);
                }

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, ex.Message);
            }
        }
    }

}
