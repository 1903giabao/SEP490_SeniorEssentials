using AutoMapper;
using Google.Cloud.Firestore;
using SE.Common.DTO;
using SE.Common;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using SE.Service.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Data.Models;
using SE.Common.Request;
using SE.Common.Enums;
using Org.BouncyCastle.Ocsp;
using SE.Common.Request.SE.Common.Request;

namespace SE.Service.Services
{
    public interface IUserLinkService
    {
        Task<IBusinessResult> SendAddFriend(SendAddFriendRequest req);
        Task<IBusinessResult> ResponseAddFriend(ResponseAddFriendRequest req);
        Task<IBusinessResult> GetAllByRequestUserId(int requestUserId);
        Task<IBusinessResult> GetAllByResponseUserId(int responseUserId);
        Task<IBusinessResult> RemoveFriend(RemoveFriendRequest req);
    }

    public class UserLinkService : IUserLinkService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly FirestoreDb _firestoreDb;
        private readonly IVideoCallService _videoCallService;
        private readonly INotificationService _notificationService;

        public UserLinkService(UnitOfWork unitOfWork, IMapper mapper, FirestoreDb firestoreDb, IVideoCallService videoCallService, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firestoreDb = firestoreDb;
            _videoCallService = videoCallService;
            _notificationService = notificationService;
        }

        public async Task<IBusinessResult> SendAddFriend(SendAddFriendRequest req)
        {
            try
            {
                var requestUser = await _unitOfWork.AccountRepository.GetByIdAsync(req.RequestUserId);

                if (requestUser == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Request user does not exist!");
                }

                if (requestUser.RoleId != 2 && requestUser.RoleId != 3)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid role!");
                }                
                
                var responseUser = await _unitOfWork.AccountRepository.GetByIdAsync(req.ResponseUserId);

                if (responseUser == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Request user does not exist!");
                }

                if (responseUser.RoleId != 2 && responseUser.RoleId != 3)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid role!");
                }

                var userLinkCheck = await _unitOfWork.UserLinkRepository.GetByUserIdsAsync(requestUser.AccountId, responseUser.AccountId);

                if (userLinkCheck != null)
                {
                    if (userLinkCheck.Status.Equals(SD.UserLinkStatus.PENDING, StringComparison.OrdinalIgnoreCase))
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Already sent request!");
                    }

                    if (userLinkCheck.Status.Equals(SD.UserLinkStatus.ACCEPTED, StringComparison.OrdinalIgnoreCase) && userLinkCheck.RelationshipType.Equals("Friend"))
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Already be Friend!");
                    }
                    else if (userLinkCheck.Status.Equals(SD.UserLinkStatus.ACCEPTED, StringComparison.OrdinalIgnoreCase) && userLinkCheck.RelationshipType.Equals("Family"))
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Already be Family!");
                    }
                }

                var userLink = new UserLink
                {
                    AccountId1 = req.RequestUserId,
                    AccountId2 = req.ResponseUserId,
                    RelationshipType = req.RelationshipType,
                    CreatedAt = DateTime.UtcNow.AddHours(7),
                    UpdatedAt = DateTime.UtcNow.AddHours(7),
                    Status = SD.UserLinkStatus.PENDING,
                };

                var createUserLink = await _unitOfWork.UserLinkRepository.CreateAsync(userLink);

                if (createUserLink > 0)
                {
/*                    if (!string.IsNullOrEmpty(responseUser.DeviceToken) && responseUser.DeviceToken != "string")
                    {
                        if (userLink.RelationshipType.Equals("Friend"))
                        {
                            // Send notification
                            await _notificationService.SendNotification(
                                responseUser.DeviceToken,
                                "Lời mời kết bạn",
                                $"Bạn nhận được lời mời kết bạn từ {requestUser.FullName}.");

                            var newNotification = new Data.Models.Notification
                            {
                                NotificationType = "Kết Bạn Mới",
                                AccountId = responseUser.AccountId,
                                Status = SD.GeneralStatus.ACTIVE,
                                Title = "Lời mời kết bạn",
                                Message = $"Bạn nhận được lời mời kết bạn từ {requestUser.FullName}.",
                                CreatedDate = System.DateTime.UtcNow.AddHours(7),
                            };

                            await _unitOfWork.NotificationRepository.CreateAsync(newNotification);
                        }
                        else 
                        {
                            await _notificationService.SendNotification(
                                responseUser.DeviceToken,
                                "Gửi yêu cầu hỗ trợ",
                                $"Bạn nhận được yêu cầu hỗ trợ từ {requestUser.FullName}.");

                            var newNotification = new Data.Models.Notification
                            {
                                NotificationType = "Gửi Yêu Cầu Hỗ Trợ",
                                AccountId = responseUser.AccountId,
                                Status = SD.GeneralStatus.ACTIVE,
                                Title = "Lời mời kết bạn",
                                Message = $"Bạn nhận được yêu cầu hỗ trợ từ {requestUser.FullName}.",
                                CreatedDate = System.DateTime.UtcNow.AddHours(7),
                            };

                            await _unitOfWork.NotificationRepository.CreateAsync(newNotification);
                        }
                    }*/

                    return new BusinessResult(Const.SUCCESS_CREATE, "Add friend request sent.");
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> ResponseAddFriend(ResponseAddFriendRequest req)
        {
            try
            {
                var requestUser = await _unitOfWork.AccountRepository.GetByIdAsync(req.RequestUserId);

                if (requestUser == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Request user does not exist!");
                }

                var responseUser = await _unitOfWork.AccountRepository.GetByIdAsync(req.ResponseUserId);

                if (responseUser == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Request user does not exist!");
                }

                var userLink = await _unitOfWork.UserLinkRepository.GetByUserIdsAsync(requestUser.AccountId, responseUser.AccountId);

                if (userLink == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Request does not exist!");
                }

                if (!userLink.Status.Equals(SD.UserLinkStatus.PENDING, StringComparison.OrdinalIgnoreCase))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid status!");
                }                

                if (req.ResponseStatus.Equals(SD.UserLinkStatus.CANCELLED, StringComparison.OrdinalIgnoreCase))
                {
                    var removeUserLink = await _unitOfWork.UserLinkRepository.RemoveAsync(userLink);

                    if (removeUserLink)
                    {
                        return new BusinessResult(Const.SUCCESS_CREATE, $"Friend relationship is {userLink.Status}.");
                    }
                }
                else if (req.ResponseStatus.Equals(SD.UserLinkStatus.ACCEPTED, StringComparison.OrdinalIgnoreCase))
                {
                    userLink.Status = SD.UserLinkStatus.ACCEPTED;
                }
                else
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Status must be CANCELLED, REJECTED, ACCEPTED!");
                }

                var updateUserLink = await _unitOfWork.UserLinkRepository.UpdateAsync(userLink);

                if (updateUserLink > 0)
                {
                    if (userLink.Status.Equals(SD.UserLinkStatus.ACCEPTED, StringComparison.OrdinalIgnoreCase))
                    {
                        var listGroupMembers = new List<GroupMemberRequest>
                        {
                            new GroupMemberRequest
                            {
                                AccountId = userLink.AccountId1,
                                IsCreator = false
                            },
                            new GroupMemberRequest
                            {
                                AccountId = userLink.AccountId2,
                                IsCreator = false
                            }
                        };

                        var roomCreateRs = await CreatePairRoomChat(listGroupMembers);

                        if (roomCreateRs.Status < 1)
                        {
                            return new BusinessResult(Const.FAIL_CREATE, roomCreateRs.Message);
                        }

                        if (userLink.RelationshipType.Equals("Family"))
                        {
                            if (requestUser.RoleId == 2 && responseUser.DateOfBirth.HasValue)
                            {
                                var elderlyAccount = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(requestUser.AccountId);

                                var newActivity = new Activity
                                {
                                    ElderlyId = elderlyAccount.Elderly.ElderlyId,
                                    ActivityName = $"Sinh Nhật Của {responseUser.FullName}",
                                    ActivityDescription = $"Sinh nhật của {responseUser.FullName} là vào ngày {responseUser.DateOfBirth.Value.ToShortDateString()}",
                                    CreatedBy = "System",
                                    Status = SD.GeneralStatus.ACTIVE
                                };
                                await _unitOfWork.ActivityRepository.CreateAsync(newActivity);

                                var startDate = responseUser.DateOfBirth.HasValue
                                    ? DateOnly.FromDateTime(responseUser.DateOfBirth.Value)
                                    : DateOnly.FromDateTime(DateTime.MinValue);
                                var year = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
                                int count = 0;
                                while (count < 5)
                                {
                                    year = year.AddYears(1);
                                    var newSchedule = new ActivitySchedule
                                    {
                                        ActivityId = newActivity.ActivityId,
                                        StartTime = new DateTime(year.Year, startDate.Month, startDate.Day, 09, 00, 0),
                                        EndTime = new DateTime(year.Year, startDate.Month, startDate.Day, 10, 00, 0),
                                        Status = SD.GeneralStatus.ACTIVE
                                    };
                                    await _unitOfWork.ActivityScheduleRepository.CreateAsync(newSchedule);
                                    count++;
                                }
                            }

                            if (responseUser.RoleId == 2 && requestUser.DateOfBirth.HasValue)
                            {
                                var elderlyAccount = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(responseUser.AccountId);

                                var newActivity = new Activity
                                {
                                    ElderlyId = elderlyAccount.Elderly.ElderlyId,
                                    ActivityName = $"Sinh Nhật Của {requestUser.FullName}",
                                    ActivityDescription = $"Sinh nhật của {requestUser.FullName} là vào ngày {requestUser.DateOfBirth.Value.ToShortDateString()}",
                                    CreatedBy = "System",
                                    Status = SD.GeneralStatus.ACTIVE
                                };
                                await _unitOfWork.ActivityRepository.CreateAsync(newActivity);

                                var startDate = requestUser.DateOfBirth.HasValue
                                    ? DateOnly.FromDateTime(requestUser.DateOfBirth.Value)
                                    : DateOnly.FromDateTime(DateTime.MinValue);
                                var year = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
                                int count = 0;
                                while (count < 5)
                                {
                                    year = year.AddYears(1);
                                    var newSchedule = new ActivitySchedule
                                    {
                                        ActivityId = newActivity.ActivityId,
                                        StartTime = new DateTime(year.Year, startDate.Month, startDate.Day, 09, 00, 0),
                                        EndTime = new DateTime(year.Year, startDate.Month, startDate.Day, 10, 00, 0),
                                        Status = SD.GeneralStatus.ACTIVE
                                    };
                                    await _unitOfWork.ActivityScheduleRepository.CreateAsync(newSchedule);
                                    count++;
                                }
                            }
                        }
                    }
/*
                    if (!string.IsNullOrEmpty(responseUser.DeviceToken) && responseUser.DeviceToken != "string")
                    {
                        if (userLink.RelationshipType.Equals("Friend"))
                        {
                            // Send notification
                            await _notificationService.SendNotification(
                                responseUser.DeviceToken,
                                "Chấp nhận kết bạn",
                                $"{responseUser.FullName} đã chấp nhận lời mời kết bạn.");

                            var newNotification = new Data.Models.Notification
                            {
                                NotificationType = "Chấp nhận kết bạn",
                                AccountId = responseUser.AccountId,
                                Status = SD.GeneralStatus.ACTIVE,
                                Title = "Chấp nhận kết bạn",
                                Message = $"{responseUser.FullName} đã chấp nhận lời mời kết bạn.",
                                CreatedDate = System.DateTime.UtcNow.AddHours(7),
                            };

                            await _unitOfWork.NotificationRepository.CreateAsync(newNotification);
                        }
                        else
                        {
                            await _notificationService.SendNotification(
                                responseUser.DeviceToken,
                                "Xác Nhận Hỗ Trợ",
                                $"{responseUser.FullName} đã chấp nhận yêu cầu hỗ trợ của bạn.");

                            var newNotification = new Data.Models.Notification
                            {
                                NotificationType = "Xác Nhận Hỗ Trợ",
                                AccountId = responseUser.AccountId,
                                Status = SD.GeneralStatus.ACTIVE,
                                Title = "Xác Nhận Hỗ Trợ",
                                Message = $"{responseUser.FullName} đã chấp nhận yêu cầu hỗ trợ của bạn."),
                                CreatedDate = System.DateTime.UtcNow.AddHours(7),
                            };

                            await _unitOfWork.NotificationRepository.CreateAsync(newNotification);
                        }
                    }*/

                    return new BusinessResult(Const.SUCCESS_CREATE, $"Add friend request is {userLink.Status}.");
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> CreatePairRoomChat(List<GroupMemberRequest> groupMembers)
        {
            try
            {
                if (groupMembers.Count < 2)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "At least two members are required to create chat rooms.");
                }

                List<Task> chatRoomTasks = new List<Task>();

                var currentTime = DateTime.UtcNow.AddHours(7);

                for (int i = 0; i < groupMembers.Count; i++)
                {
                    for (int j = i + 1; j < groupMembers.Count; j++)
                    {
                        var member1 = groupMembers[i];
                        var member2 = groupMembers[j];

                        var chatRoomQuery = _firestoreDb.Collection("ChatRooms")
                            .WhereEqualTo($"MemberIds.{member1.AccountId}", true);

                        var chatRoomSnapshot = await chatRoomQuery.GetSnapshotAsync();

                        bool chatRoomExists = false;
                        foreach (var doc in chatRoomSnapshot.Documents)
                        {
                            var memberIds = doc.GetValue<Dictionary<string, bool>>("MemberIds");
                            if (memberIds != null && memberIds.ContainsKey(member2.AccountId.ToString()))
                            {
                                chatRoomExists = true;
                                break;
                            }
                        }

                        if (chatRoomExists)
                        {
                            continue;
                        }

                        DocumentReference pairChatRoomRef = _firestoreDb.Collection("ChatRooms").Document();

                        var pairChatRoomData = new Dictionary<string, object>
                        {
                            { "CreatedAt",  currentTime.ToString("dd-MM-yyyy HH:mm") },
                            { "IsGroupChat", false },
                            { "RoomName", "" },
                            { "RoomAvatar", "" },
                            { "SenderId", 0 },
                            { "LastMessage", "" },
                            { "SentDate", "" },
                            { "SentTime", "" },
                            { "SentDateTime", "" },
                            { "MemberIds", new Dictionary<string, object>
                                {
                                    { member1.AccountId.ToString(), true },
                                    { member2.AccountId.ToString(), true }
                                }
                            },
                        };

                        await pairChatRoomRef.SetAsync(pairChatRoomData);

                        await pairChatRoomRef.Collection("Members").Document(member1.AccountId.ToString()).SetAsync(new { IsCreator = false });
                        await pairChatRoomRef.Collection("Members").Document(member2.AccountId.ToString()).SetAsync(new { IsCreator = false });
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

                return new BusinessResult(Const.SUCCESS_CREATE, "Chat rooms created successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> RemoveFriend(RemoveFriendRequest req)
        {
            try
            {
                var requestUser = await _unitOfWork.AccountRepository.GetByIdAsync(req.RequestUserId);
                if (requestUser == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Request user does not exist!");
                }

                var responseUser = await _unitOfWork.AccountRepository.GetByIdAsync(req.ResponseUserId);
                if (responseUser == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Response user does not exist!");
                }

                var commonGroups = _unitOfWork.GroupMemberRepository.GetAll()
                    .Where(gm => gm.AccountId == req.RequestUserId || gm.AccountId == req.ResponseUserId)
                    .GroupBy(gm => gm.GroupId)
                    .Where(g => g.Count() == 2)
                    .ToList();

                if (commonGroups.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Cannot remove friend because both users are in the same family group.");
                }

                var userLink = await _unitOfWork.UserLinkRepository.GetByUserIdsAsync(requestUser.AccountId, responseUser.AccountId);
                if (userLink == null || !userLink.Status.Equals(SD.UserLinkStatus.ACCEPTED, StringComparison.OrdinalIgnoreCase))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid status!");
                }

                var listUserInChat = new List<int> { requestUser.AccountId, responseUser.AccountId };
                var chatRoomId = await _videoCallService.FindChatRoomContainingAllUsers(listUserInChat);

                var roomRef = _firestoreDb.Collection("ChatRooms").Document(chatRoomId);
                await roomRef.DeleteAsync();

                var removeUserLink = await _unitOfWork.UserLinkRepository.RemoveAsync(userLink);

                if (removeUserLink)
                {
                    if (requestUser.RoleId == 2)
                    {
                        var elderlyAccount = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(requestUser.AccountId);

                        var activityDOB = _unitOfWork.ActivityRepository.FindByCondition(a => a.ElderlyId == elderlyAccount.Elderly.ElderlyId).FirstOrDefault();

                        if (activityDOB != null)
                        {
                            var deleteResult = await _unitOfWork.ActivityScheduleRepository.DeleteActivitySchedulesByActivityIdAsync(activityDOB.ActivityId);

                            if (deleteResult < 1)
                            {
                                return new BusinessResult(Const.FAIL_DELETE, Const.FAIL_DELETE_MSG);
                            }

                            activityDOB.Status = SD.GeneralStatus.INACTIVE;
                            await _unitOfWork.ActivityRepository.UpdateAsync(activityDOB);
                        }
                    }

                    if (responseUser.RoleId == 2)
                    {
                        var elderlyAccount = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(responseUser.AccountId);

                        var activityDOB = _unitOfWork.ActivityRepository.FindByCondition(a => a.ElderlyId == elderlyAccount.Elderly.ElderlyId).FirstOrDefault();

                        if (activityDOB != null)
                        {
                            var deleteResult = await _unitOfWork.ActivityScheduleRepository.DeleteActivitySchedulesByActivityIdAsync(activityDOB.ActivityId);

                            if (deleteResult < 1)
                            {
                                return new BusinessResult(Const.FAIL_DELETE, Const.FAIL_DELETE_MSG);
                            }

                            activityDOB.Status = SD.GeneralStatus.INACTIVE;
                            await _unitOfWork.ActivityRepository.UpdateAsync(activityDOB);
                        }
                    }

                    return new BusinessResult(Const.SUCCESS_CREATE, $"Relationship is removed.");
                }

                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllByRequestUserId(int requestUserId)
        {
            try
            {
                var requestUser = _unitOfWork.UserLinkRepository.GetAll().Where(u => u.AccountId1 == requestUserId).FirstOrDefault();

                if (requestUser == null)
                {
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG);
                }

                var userLinks = await _unitOfWork.UserLinkRepository.GetByAccount1Async(requestUserId, SD.UserLinkStatus.PENDING);

                var result = userLinks.Select(ul => new UserLinkDTO
                {
                    RequestUserId = ul.AccountId1,
                    RequestUserName = ul.AccountId1Navigation?.FullName,
                    RequestUserAvatar = ul.AccountId1Navigation?.Avatar,
                    ResponseUserId = ul.AccountId2,
                    ResponseUserName = ul.AccountId2Navigation?.FullName,
                    ResponseUserAvatar = ul.AccountId2Navigation?.Avatar,
                    CreatedAt = (DateTime)ul.CreatedAt,
                    User = new UserInUserLinkDTO
                    {
                        RequestUserId = ul.AccountId1,
                        AccountId = ul.AccountId2Navigation.AccountId,
                        RoleId = ul.AccountId2Navigation.RoleId,
                        Email = ul.AccountId2Navigation.Email,
                        Password = ul.AccountId2Navigation.Password,
                        FullName = ul.AccountId2Navigation.FullName,
                        Avatar = ul.AccountId2Navigation.Avatar,
                        Gender = ul.AccountId2Navigation.Gender,
                        PhoneNumber = ul.AccountId2Navigation.PhoneNumber,
                        DateOfBirth = ul.AccountId2Navigation.DateOfBirth,
                        CreatedDate = ul.AccountId2Navigation.CreatedDate,
                        Status = ul.AccountId2Navigation.Status,
                    }
                }).ToList();

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllByResponseUserId(int responseUserId)
        {
            try
            {
                var requestUser = _unitOfWork.UserLinkRepository.GetAll().Where(u => u.AccountId2 == responseUserId).FirstOrDefault();

                if (requestUser == null)
                {
                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG);
                }


                var userLinks = await _unitOfWork.UserLinkRepository.GetByAccount2Async(responseUserId, SD.UserLinkStatus.PENDING);

                var result = userLinks.Select(ul => new UserLinkDTO
                {
                    RequestUserId = ul.AccountId1,
                    RequestUserName = ul.AccountId1Navigation?.FullName,
                    RequestUserAvatar = ul.AccountId1Navigation?.Avatar,
                    ResponseUserId = ul.AccountId2,
                    ResponseUserName = ul.AccountId2Navigation?.FullName,
                    ResponseUserAvatar = ul.AccountId2Navigation?.Avatar,
                    CreatedAt = (DateTime)ul.CreatedAt,
                    User = new UserInUserLinkDTO
                    {
                        RequestUserId = ul.AccountId1,
                        AccountId = ul.AccountId1Navigation.AccountId,
                        RoleId = ul.AccountId1Navigation.RoleId,
                        Email = ul.AccountId1Navigation.Email,
                        Password = ul.AccountId1Navigation.Password,
                        FullName = ul.AccountId1Navigation.FullName,
                        Avatar = ul.AccountId1Navigation.Avatar,
                        Gender = ul.AccountId1Navigation.Gender,
                        PhoneNumber = ul.AccountId1Navigation.PhoneNumber,
                        DateOfBirth = ul.AccountId1Navigation.DateOfBirth,
                        CreatedDate = ul.AccountId1Navigation.CreatedDate,
                        Status = ul.AccountId1Navigation.Status,
                    }
                }).ToList();

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }
    }
}
