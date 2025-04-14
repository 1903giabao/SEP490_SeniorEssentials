using AutoMapper;
using SE.Common.Request;
using SE.Common;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using SE.Common.Enums;
using SE.Common.Request.SE.Common.Request;
using Google.Cloud.Firestore;
using Firebase.Auth;
using SE.Common.DTO;
using AutoMapper.Execution;
using Org.BouncyCastle.Ocsp;
using SE.Common.Response.Group;
using Google.Cloud.Firestore.V1;
using SE.Common.Response.HealthIndicator;
using Microsoft.Identity.Client;
using System.Text.RegularExpressions;
using CloudinaryDotNet;
using SE.Common.Request.Group;

namespace SE.Service.Services
{
    public interface IGroupService
    {
        Task<IBusinessResult> GetAllElderlyByFamilyMemberId(int accountId);
        Task<IBusinessResult> ChangeGroupName(ChangeGroupNameRequest req);
        Task<IBusinessResult> CreateGroup(CreateGroupRequest request);
        Task<IBusinessResult> GetGroupsByAccountId(int accountId);
        Task<IBusinessResult> RemoveMemberFromGroup(int kickerId, int groupId, int accountId);
        Task<IBusinessResult> RemoveGroup(int groupId);
        Task<IBusinessResult> GetMembersByGroupId(int groupId);
        Task<IBusinessResult> GetAllGroupMembersByUserId(int userId);
        List<(int, int)> GetUniquePairs(List<int> memberIds);
        Task<IBusinessResult> CreateRoomChat(List<GroupMemberRequest> groupMembers, string groupName);
        Task<IBusinessResult> AddMemberToGroup(AddMemberToGroupRequest req);
        Task<IBusinessResult> GetMembersNotInGroupChat(string groupChatId);
        Task<List<int>> GetAllFamilyMembersByElderly(int accountId);
        Task<IBusinessResult> CheckIfElderlyInGroup(int elderly);
        Task<IBusinessResult> GetGroupAndRelationshipInforByElderly(int elderlyId);
        Task<IBusinessResult> GetGroupAndRelationshipInforByFamily(int familyMemberId);
    }

    public class GroupService : IGroupService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly FirestoreDb _firestoreDb;
        private readonly IVideoCallService _videoCallService;
        private readonly INotificationService _notificationService;
        private readonly IUserLinkService _userLinkService;
        public GroupService(UnitOfWork unitOfWork, IMapper mapper, FirestoreDb firestoreDb, IVideoCallService videoCallService, INotificationService notificationService, IUserLinkService userLinkService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firestoreDb = firestoreDb;
            _videoCallService = videoCallService;
            _notificationService = notificationService;
            _userLinkService = userLinkService;
        }
        public async Task<List<int>> GetAllFamilyMembersByElderly(int accountId)
        {
            try
            {
                if (accountId <= 0)
                {
                    return new List<int>();
                }

                var userGroups = _unitOfWork.GroupMemberRepository.GetAll()
                    .Where(gm => gm.AccountId == accountId && gm.Status == SD.GeneralStatus.ACTIVE)
                    .Select(gm => gm.GroupId)
                    .ToList();

                if (!userGroups.Any())
                {
                    return new List<int>();
                }

                var result = new List<int>();

                foreach (var groupId in userGroups)
                {
                    var group = await _unitOfWork.GroupRepository.GetByIdAsync(groupId);

                    var groupMembers = _unitOfWork.GroupMemberRepository.GetAll()
                        .Where(gm => gm.GroupId == groupId &&
                                     gm.Status == SD.GeneralStatus.ACTIVE &&
                                     gm.AccountId != accountId)
                        .Distinct()
                        .Select(gm => gm.AccountId)
                        .ToList();

                    if (groupMembers.Any())
                    {
                        var users = _unitOfWork.AccountRepository.GetAll()
                            .Where(a => groupMembers.Contains(a.AccountId) && a.RoleId == 3)
                            .Select(a => _mapper.Map<UserDTO>(a))
                            .Distinct()
                            .ToList();

                        result.AddRange(users.Select(u => u.AccountId));
                    }
                }

                if (!result.Any())
                {
                    return new List<int>();
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<IBusinessResult> GetAllElderlyByFamilyMemberId(int accountId)
        {
            try
            {
                if (accountId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid account ID.");
                }

                var familyMember = _unitOfWork.AccountRepository.FindByCondition(a => a.AccountId == accountId && a.RoleId == 3 && a.Status.Equals(SD.GeneralStatus.ACTIVE)).FirstOrDefault();

                if (familyMember == null) 
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Family Member does not existed");
                }

                var userGroups = _unitOfWork.GroupMemberRepository.GetAll()
                    .Where(gm => gm.AccountId == accountId && gm.Status == SD.GeneralStatus.ACTIVE)
                    .Select(gm => gm.GroupId)
                    .ToList();

                if (!userGroups.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "User is not a member of any group.");
                }

                var result = new List<GetAllElderlyInGroupResponse>();

                foreach (var groupId in userGroups)
                {
                    var group = await _unitOfWork.GroupRepository.GetByIdAsync(groupId);

                    var elders = await _unitOfWork.GroupMemberRepository.GetElderlyInGroupByGroupIdAsync(group.GroupId, SD.GeneralStatus.ACTIVE);

                    if (elders.Any())
                    {
                        var users = elders.Select(e => _mapper.Map<AccountElderlyDTO>(e.Account)).ToList();

                        result.Add(new GetAllElderlyInGroupResponse
                        {
                            GroupId = groupId,
                            GroupName = group.GroupName,
                            Members = users
                        });
                    }
                }

                if (!result.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "No active members found in any group.");
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> ChangeGroupName(ChangeGroupNameRequest req)
        {
            try
            {
                if (req.GroupId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Invalid group ID.");
                }

                var group = _unitOfWork.GroupRepository.FindByCondition(a => a.GroupId == req.GroupId && a.Status.Equals(SD.GeneralStatus.ACTIVE)).FirstOrDefault();

                if (group == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Group does not existed");
                }

                group.GroupName = req.GroupName;

                var result = await _unitOfWork.GroupRepository.UpdateAsync(group);

                if (result < 1)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG);
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateGroup(CreateGroupRequest request)
        {
            try
            {
                if (request == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Request cannot be null.");
                }

                if (string.IsNullOrWhiteSpace(request.GroupName))
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Group name cannot be empty.");
                }

                if (request.CreatorAccountId <= 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Invalid creator account ID.");
                }

                var checkCreator = await _unitOfWork.AccountRepository.GetByIdAsync(request.CreatorAccountId);

                if (checkCreator.RoleId != 3)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Creator is not Family Member");
                }

                var accountIds = request.Members.Select(m => m.AccountId).ToList();

                var existingGroupIds = _unitOfWork.GroupMemberRepository.GetAll()
                    .Where(gm => accountIds.Contains(gm.AccountId))
                    .GroupBy(gm => gm.GroupId)
                    .Where(g => g.Count() == accountIds.Count)
                    .Select(g => g.Key)
                    .ToList();

                if (existingGroupIds.Any())
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "A group with the same members already exists.");
                }

                var memberIds = request.Members.Select(m => m.AccountId).ToList();

                foreach (var memberId  in memberIds)
                {
                    var elderlyCheck = await _unitOfWork.AccountRepository.GetByIdAsync(memberId);

                    if (elderlyCheck.RoleId == 2) 
                    {
                        var elderly = _unitOfWork.GroupMemberRepository.FindByCondition(gm => gm.AccountId == elderlyCheck.AccountId).FirstOrDefault();

                        if (elderly != null)
                        {
                            return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, $"Người già đã ở trong nhóm gia đình khác!");
                        }
                    }
                }

                var allPairs = GetUniquePairs(memberIds);

                foreach (var pair in allPairs)
                {
                    var relationshipExists = _unitOfWork.UserLinkRepository.GetAll()
                        .Any(ul => (ul.AccountId1 == pair.Item1 && ul.AccountId2 == pair.Item2 && ul.RelationshipType.Equals("Family")) ||
                                   (ul.AccountId1 == pair.Item2 && ul.AccountId2 == pair.Item1) && ul.RelationshipType.Equals("Family"));

                    if (!relationshipExists)
                    {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, $"Members {pair.Item1} and {pair.Item2} is not Family.");
                    }
                }

                var group = _mapper.Map<Data.Models.Group>(request);
                group.CreatedDate = DateTime.UtcNow.AddHours(7);
                group.Status = SD.GeneralStatus.ACTIVE;

                await _unitOfWork.GroupRepository.CreateAsync(group);

                foreach (var member in request.Members)
                {
                    var groupMember = new GroupMember
                    {
                        GroupId = group.GroupId,
                        AccountId = member.AccountId,
                        IsCreator = member.IsCreator,
                        Status = SD.GeneralStatus.ACTIVE
                    };
                    var createRs = await _unitOfWork.GroupMemberRepository.CreateAsync(groupMember);

                    if (createRs < 1)
                    {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG);
                    }
                }

                var roomCreateRs = await CreateRoomChat(request.Members, group.GroupName);

                if (roomCreateRs.Status < 1)
                {
                    return new BusinessResult(Const.FAIL_CREATE, roomCreateRs.Message);
                }

                if (roomCreateRs.Data != null)
                {
                    group.GroupChatId = (string)roomCreateRs.Data;
                    await _unitOfWork.GroupRepository.UpdateAsync(group);
                }

                var listFamilyMember = request.Members.Where(m => m.IsCreator == false).Select(m => m.AccountId).ToList();

                foreach (var member in listFamilyMember)
                {
                    var familyMember = await _unitOfWork.AccountRepository.GetByIdAsync(member);

                    if (!string.IsNullOrEmpty(familyMember.DeviceToken) && familyMember.DeviceToken != "string")
                    {
                        // Send notification
                        await _notificationService.SendNotification(
                            familyMember.DeviceToken,
                            "Thêm Vào Gia Đình",
                            $"Bạn đã được vào nhóm gia đình {group.GroupName}.");

                        var newNotification = new Data.Models.Notification
                        {
                            NotificationType = "Thêm Vào Gia Đình",
                            AccountId = familyMember.AccountId,
                            Status = SD.NotificationStatus.SEND,
                            Title = "Thêm Vào Gia Đình",
                            Message = $"Bạn đã được vào nhóm gia đình {group.GroupName}.",
                            CreatedDate = System.DateTime.UtcNow.AddHours(7),
                        };

                        await _unitOfWork.NotificationRepository.CreateAsync(newNotification);
                    }

                    if (!string.IsNullOrEmpty(familyMember.DeviceToken) && familyMember.DeviceToken != "string")
                    {
                        // Send notification
                        await _notificationService.SendNotification(
                            familyMember.DeviceToken,
                            "Thêm vào nhóm chat",
                            $"Bạn đã được vào nhóm chat {group.GroupName}.");

                        var newNotification = new Data.Models.Notification
                        {
                            NotificationType = "Thêm vào nhóm chat",
                            AccountId = familyMember.AccountId,
                            Status = SD.GeneralStatus.ACTIVE,
                            Title = "Thêm vào nhóm chat",
                            Message = $"Bạn đã được vào nhóm chat {group.GroupName}.",
                            CreatedDate = System.DateTime.UtcNow.AddHours(7),
                        };

                        await _unitOfWork.NotificationRepository.CreateAsync(newNotification);
                    }
                }

                return new BusinessResult(Const.SUCCESS_CREATE, "Group created successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateRoomChat(List<GroupMemberRequest> groupMembers, string groupName)
        {
            try
            {
                if (groupMembers.Count < 2)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "At least two members are required to create chat rooms.");
                }

                List<Task> chatRoomTasks = new List<Task>();

                var currentTime = DateTime.UtcNow.AddHours(7);
                var groupId = Guid.NewGuid().ToString();

                if (groupMembers.Count > 2)
                {                    
                    DocumentReference groupChatRoomRef = _firestoreDb.Collection("ChatRooms").Document(groupId); 

                    var groupChatRoomData = new Dictionary<string, object>
                    {
                        { "CreatedAt", currentTime.ToString("dd-MM-yyyy HH:mm") },
                        { "IsGroupChat", true },
                        { "RoomName", groupName },
                        { "RoomAvatar", "https://icons.veryicon.com/png/o/miscellaneous/standard/avatar-15.png" },
                        { "SenderId", 0 },
                        { "LastMessage", "" },
                        { "SentDate", currentTime.ToString("dd-MM-yyyy") },
                        { "SentTime", currentTime.ToString("HH:mm") },
                        { "SentDateTime", currentTime.ToString("dd-MM-yyyy HH:mm") },
                            {
                                "MemberIds", groupMembers
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

                return new BusinessResult(Const.SUCCESS_CREATE, "Chat rooms created successfully.", groupId);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public List<(int, int)> GetUniquePairs(List<int> memberIds)
        {
            var pairs = new List<(int, int)>();
            for (int i = 0; i < memberIds.Count; i++)
            {
                for (int j = i + 1; j < memberIds.Count; j++)
                {
                    pairs.Add((memberIds[i], memberIds[j]));
                }
            }
            return pairs;
        }

        public async Task<IBusinessResult> AddMemberToGroup(AddMemberToGroupRequest req)
        {
            try
            {
                if (req.GroupId <= 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Invalid group ID.");
                }

                if (req.MemberIds == null || !req.MemberIds.Any())
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Member IDs cannot be null or empty.");
                }

                var group = await _unitOfWork.GroupRepository.GetByIdAsync(req.GroupId);
                if (group == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE, "Group does not exist.");
                }

                foreach (var memberId in req.MemberIds)
                {
                    var elderlyCheck = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(memberId);

                    if (elderlyCheck.RoleId == 2)
                    {
                        var elderly = _unitOfWork.GroupMemberRepository.FindByCondition(gm => gm.AccountId == elderlyCheck.AccountId && gm.Status.Equals(SD.GeneralStatus.ACTIVE)).FirstOrDefault();

                        if (elderly != null)
                        {
                            return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, $"Người già {elderlyCheck.FullName} đã ở trong nhóm gia đình khác!");
                        }
                    }
                }

                var existingMembers = _unitOfWork.GroupMemberRepository.GetAll()
                    .Where(gm => gm.GroupId == req.GroupId)
                    .ToList();

                var duplicateMembers = existingMembers
                    .Where(gm => req.MemberIds.Contains(gm.AccountId))
                    .ToList();

                foreach (var duplicateMember in duplicateMembers)
                {
                    if (duplicateMember.Status == SD.GeneralStatus.INACTIVE)
                    {
                        duplicateMember.Status = SD.GeneralStatus.ACTIVE;
                        var updateRs = await _unitOfWork.GroupMemberRepository.UpdateAsync(duplicateMember);

                        if (updateRs < 1)
                        {
                            return new BusinessResult(Const.FAIL_CREATE, $"Failed to reactivate member {duplicateMember.AccountId}.");
                        }
                    }
                }

                var newMemberIds = req.MemberIds
                    .Except(existingMembers.Select(gm => gm.AccountId))
                    .ToList();

                var allMemberIds = existingMembers.Select(gm => gm.AccountId).Concat(newMemberIds).ToList();
                var allPairs = GetUniquePairs(allMemberIds);

                foreach (var pair in allPairs)
                {
                    var relationshipExists = _unitOfWork.UserLinkRepository.GetAll()
                        .Any(ul => (ul.AccountId1 == pair.Item1 && ul.AccountId2 == pair.Item2 && ul.RelationshipType.Equals("Family")) ||
                                   (ul.AccountId1 == pair.Item2 && ul.AccountId2 == pair.Item1 && ul.RelationshipType.Equals("Family")));

                    if (!relationshipExists)
                    {
                        var userLink = new UserLink
                        {
                            AccountId1 = pair.Item1,
                            AccountId2 = pair.Item2,
                            CreatedAt = DateTime.UtcNow.AddHours(7),
                            UpdatedAt = DateTime.UtcNow.AddHours(7),
                            Status = "Accepted",
                            RelationshipType = "Family"
                        };

                        var createRs = await _unitOfWork.UserLinkRepository.CreateAsync(userLink);

                        if (createRs < 1)
                        {
                            return new BusinessResult(Const.FAIL_CREATE, $"Failed to create user link.");
                        }
                    }
                }

                foreach (var memberId in newMemberIds)
                {
                    var groupMember = new GroupMember
                    {
                        GroupId = req.GroupId,
                        AccountId = memberId,
                        IsCreator = false,
                        Status = SD.GeneralStatus.ACTIVE
                    };

                    var createRs = await _unitOfWork.GroupMemberRepository.CreateAsync(groupMember);

                    if (createRs < 1)
                    {
                        return new BusinessResult(Const.FAIL_CREATE, "Failed to add member to the group.");
                    }
                }

                var isCreator = _unitOfWork.GroupMemberRepository.GetAll().Where(gm => gm.GroupId == group.GroupId && gm.IsCreator == true).Select(gm => gm.AccountId).FirstOrDefault();

                var listGroupMembers = new List<GroupMemberRequest>();

                foreach (var memberId in allMemberIds)
                {
                    listGroupMembers.Add(new GroupMemberRequest
                    {
                        AccountId = memberId,
                        IsCreator = memberId == isCreator ? true : false,
                    });
                }

                var roomCreateRs = await _userLinkService.CreatePairRoomChat(listGroupMembers);

                if (roomCreateRs.Status < 1)
                {
                    return new BusinessResult(Const.FAIL_CREATE, roomCreateRs.Message);
                }

                var allGroupMembers = existingMembers.Select(m => m.AccountId).ToList();

                var roomChatId = group.GroupChatId;

                var groupRef = _firestoreDb.Collection("ChatRooms").Document(roomChatId);
                var groupDoc = await groupRef.GetSnapshotAsync();

                if (groupDoc.Exists)
                {
                    var currentMembers = groupDoc.GetValue<Dictionary<string, object>>("MemberIds") ?? new Dictionary<string, object>();

                    foreach (var member in req.MemberIds)
                    {
                        var memberIdStr = member.ToString();

                        if (currentMembers.ContainsKey(memberIdStr))
                        {
                            return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Member is already in this group!");
                        }

                        currentMembers.Add(memberIdStr, true);
                        var membersRef = groupRef.Collection("Members").Document(memberIdStr);
                        await membersRef.SetAsync(new { IsCreator = false });
                    }

                    var updateData = new Dictionary<string, object>
                    {
                        { "MemberIds", currentMembers }
                    };

                    await groupRef.UpdateAsync(updateData);
                }
                else
                {
                    var currentTime = DateTime.UtcNow.AddHours(7);

                    var groupChatRoomData = new Dictionary<string, object>
                    {
                        { "CreatedAt", currentTime.ToString("dd-MM-yyyy HH:mm") },
                        { "IsGroupChat", true },
                        { "RoomName", group.GroupName },
                        { "RoomAvatar", "https://icons.veryicon.com/png/o/miscellaneous/standard/avatar-15.png" },
                        { "SenderId", 0 },
                        { "LastMessage", "" },
                        { "SentDate", currentTime.ToString("dd-MM-yyyy") },
                        { "SentTime", currentTime.ToString("HH:mm") },
                        { "SentDateTime", currentTime.ToString("dd-MM-yyyy HH:mm") },
                            {
                                "MemberIds", allMemberIds
                                    .ToDictionary(m => m.ToString(), m => (object)true)
                            }
                    };

                    await groupRef.SetAsync(groupChatRoomData);

                    foreach (var member in listGroupMembers)
                    {
                        await groupRef.Collection("Members").Document(member.AccountId.ToString()).SetAsync(new { IsCreator = member.IsCreator });
                    }
                }

                var onlineMembersRef = _firestoreDb.Collection("OnlineMembers");

                foreach (var member in req.MemberIds)
                {
                    var onlineMemberData = new Dictionary<string, object>
                            {
                                { "IsOnline", true }
                            };

                    await onlineMembersRef.Document(member.ToString()).SetAsync(onlineMemberData);
                }

                foreach (var member in newMemberIds)
                {
                    var familyMember = await _unitOfWork.AccountRepository.GetByIdAsync(member);

                    if (!string.IsNullOrEmpty(familyMember.DeviceToken) && familyMember.DeviceToken != "string")
                    {
                        // Send notification
                        await _notificationService.SendNotification(
                            familyMember.DeviceToken,
                            "Thêm Vào Gia Đình",
                            $"Bạn đã được vào nhóm gia đình {group.GroupName}.");

                        var newNotification = new Data.Models.Notification
                        {
                            NotificationType = "Thêm Vào Gia Đình",
                            AccountId = familyMember.AccountId,
                            Status = SD.NotificationStatus.SEND,
                            Title = "Thêm Vào Gia Đình",
                            Message = $"Bạn đã được vào nhóm gia đình {group.GroupName}.",
                            CreatedDate = System.DateTime.UtcNow.AddHours(7),
                        };

                        await _unitOfWork.NotificationRepository.CreateAsync(newNotification);
                    }
                }

                return new BusinessResult(Const.SUCCESS_CREATE, "Members added to the group successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetGroupsByAccountId(int accountId)
        {
            try
            {
                var isExisted= _unitOfWork.AccountRepository.GetById(accountId);

                if (isExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, "Account not found.");
                }

                var groupMembers = await _unitOfWork.GroupMemberRepository
                    .GetGroupMembersByAccountIdAsync(accountId);

                if (groupMembers == null || !groupMembers.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "No groups found for the given account ID.");
                }

                var result = groupMembers.Select(gm => new GroupMemberDTO
                {
                    GroupId = gm.GroupId,
                    AccountId = gm.AccountId,
                    FullName = gm.Account.FullName,
                    Avatar = gm.Account.Avatar,
                    IsCreator = gm.IsCreator,
                    GroupName = gm.Group.GroupName
                }).ToList();

                return new BusinessResult(Const.SUCCESS_READ, "Groups retrieved successfully.", result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> RemoveMemberFromGroup(int kickerId,int groupId, int accountId)
        {
            try
            {
                var groupMember = await _unitOfWork.GroupMemberRepository
                    .GetByGroupIdAndAccountIdAsync(groupId, accountId);

                if (groupMember == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Group member not found.");
                }

                var isMemberCreator = _unitOfWork.GroupMemberRepository.FindByCondition(gm => gm.AccountId == accountId && gm.GroupId == groupId).Select(gm => gm.IsCreator).FirstOrDefault();

                if (isMemberCreator)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Creator cannot be kicked/out!");
                }

                groupMember.Status = SD.GeneralStatus.INACTIVE;

                if (kickerId != accountId)
                {
                    var isKickerCreator = _unitOfWork.GroupMemberRepository.FindByCondition(gm => gm.AccountId == kickerId && gm.GroupId == groupId).Select(gm => gm.IsCreator).FirstOrDefault();

                    if (!isKickerCreator)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Only the group creator can perform this action!");
                    }
                }

                var allGroupMembers = _unitOfWork.GroupMemberRepository.FindByCondition(gm => gm.GroupId == groupId).Select(gm => gm.AccountId).ToList();

                var roomChatId = await _videoCallService.FindChatRoomContainingAllUsers(allGroupMembers, true);

                if (roomChatId != null)
                {
                    var groupRef = _firestoreDb.Collection("ChatRooms").Document(roomChatId);
                    var groupDoc = await groupRef.GetSnapshotAsync();

                    if (!groupDoc.Exists)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Group chat does not exist!");
                    }

                    var currentMembers = groupDoc.GetValue<Dictionary<string, object>>("MemberIds") ?? new Dictionary<string, object>();
                    var memberIdStr = accountId.ToString();

                    if (!currentMembers.ContainsKey(memberIdStr))
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Member is not part of this group!");
                    }

                    currentMembers.Remove(memberIdStr);

                    var updateData = new Dictionary<string, object>
                    {
                        { "MemberIds", currentMembers }
                    };

                    await groupRef.UpdateAsync(updateData);

                    var membersRef = groupRef.Collection("Members").Document(memberIdStr);
                    await membersRef.DeleteAsync();
                }

                var existingMembers = _unitOfWork.GroupMemberRepository.GetAll()
                            .Where(gm => gm.GroupId == groupId && gm.Status.Equals(SD.GeneralStatus.ACTIVE) && gm.AccountId != accountId && gm.AccountId != kickerId)
                            .Select(gm => gm.AccountId)
                            .ToList();

                foreach (var familyMember in existingMembers)
                {
                    var relationshipExists = _unitOfWork.UserLinkRepository.GetAll()
                        .Where(ul => (ul.AccountId1 == familyMember && ul.AccountId2 == accountId && ul.RelationshipType.Equals("Family")) ||
                                   (ul.AccountId1 == accountId && ul.AccountId2 == familyMember && ul.RelationshipType.Equals("Family"))).FirstOrDefault();

                    if (relationshipExists != null)
                    {
                        var deleteRs = await _unitOfWork.UserLinkRepository.RemoveAsync(relationshipExists);

                        if (!deleteRs)
                        {
                            return new BusinessResult(Const.FAIL_CREATE, $"Failed to delete user link.");
                        }
                    }
                }

                await _unitOfWork.GroupMemberRepository.UpdateAsync(groupMember);

                return new BusinessResult(Const.SUCCESS_UPDATE, "Member removed from group successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> RemoveGroup(int groupId)
        {
            try
            {
                var groupMember = await _unitOfWork.GroupRepository.GetByIdAsync(groupId);

                if (groupMember == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Group not found.");
                }

                groupMember.Status = SD.GeneralStatus.INACTIVE;

                await _unitOfWork.GroupRepository.UpdateAsync(groupMember);

                return new BusinessResult(Const.SUCCESS_UPDATE, "Removed group successfully.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }


        public async Task<IBusinessResult> GetMembersByGroupId(int groupId)
        {
            try
            {
                var groupMembers = await _unitOfWork.GroupMemberRepository.GetByGroupIdAsync(groupId);

                if (groupMembers == null || !groupMembers.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "No members found for the given group ID.");
                }

                var rs = _mapper.Map<List<GroupMemberDTO>>(groupMembers);

                return new BusinessResult(Const.SUCCESS_READ, "Group members retrieved successfully.", rs);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetMembersNotInGroupChat(string groupChatId)
        {
            try
            {
                DocumentReference roomChatRef = _firestoreDb.Collection("ChatRooms").Document(groupChatId);
                DocumentSnapshot documentSnapshot = await roomChatRef.GetSnapshotAsync();

                if (!documentSnapshot.Exists)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Group chat does not exist.");
                }

                var memberIds = documentSnapshot.GetValue<Dictionary<string, object>>("MemberIds")?.Keys
                    .Select(int.Parse)
                    .ToList();

                if (memberIds == null || !memberIds.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "No members found in the group chat.");
                }

                var familyGroups = _unitOfWork.GroupMemberRepository.GetAll()
                    .Where(gm => gm.Status == SD.GeneralStatus.ACTIVE)
                    .GroupBy(gm => gm.GroupId)
                    .Where(g => memberIds.All(id => g.Any(gm => gm.AccountId == id)))
                    .Select(g => g.Key)
                    .ToList();

                if (!familyGroups.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "No valid family groups found where all group chat members are present.");
                }

                var result = new List<GetAllGroupMembersDTO>();

                foreach (var groupId in familyGroups)
                {
                    var group = await _unitOfWork.GroupRepository.GetByIdAsync(groupId);

                    var groupMembers = _unitOfWork.GroupMemberRepository.GetAll()
                        .Where(gm => gm.GroupId == groupId &&
                                     gm.Status == SD.GeneralStatus.ACTIVE)
                    .Select(gm => gm.AccountId)
                    .Distinct()
                    .ToList();

                    if (groupMembers.Any())
                    {

                        var membersNotInGroupChat = groupMembers
                            .Where(memberId => !memberIds.Contains(memberId))
                            .ToList();

                        var users = _unitOfWork.AccountRepository.GetAll()
                            .Where(a => membersNotInGroupChat.Contains(a.AccountId))
                            .Select(a => _mapper.Map<UserDTO>(a))
                            .ToList();

                        result.Add(new GetAllGroupMembersDTO
                        {
                            GroupId = groupId,
                            GroupName = group.GroupName,
                            Members = users
                        });
                    }
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllGroupMembersByUserId(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, "Invalid user ID.");
                }

                var userGroups = _unitOfWork.GroupMemberRepository.GetAll()
                    .Where(gm => gm.AccountId == userId && gm.Status == SD.GeneralStatus.ACTIVE)
                    .Select(gm => gm.GroupId)
                    .ToList();

                if (!userGroups.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "User is not a member of any group.");
                }

                var result = new List<GetAllGroupMembersDTO>();

                foreach (var groupId in userGroups)
                {
                    var group = await _unitOfWork.GroupRepository.GetByIdAsync(groupId);

                    var groupMembers = _unitOfWork.GroupMemberRepository.GetAll()
                        .Where(gm => gm.GroupId == groupId &&
                                     gm.Status == SD.GeneralStatus.ACTIVE &&
                                     gm.AccountId != userId) 
                        .Select(gm => gm.AccountId)
                        .ToList();

                    if (groupMembers.Any())
                    {
                        var users = _unitOfWork.AccountRepository.GetAll()
                            .Where(a => groupMembers.Contains(a.AccountId))
                            .Select(a => _mapper.Map<UserDTO>(a))
                            .ToList();

                        result.Add(new GetAllGroupMembersDTO
                        {
                            GroupId = groupId,
                            GroupName = group.GroupName,
                            Members = users
                        });
                    }
                }

                if (!result.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, "No active members found in any group.");
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }


        public async Task<IBusinessResult> CheckIfElderlyInGroup(int elderly)
        {
            try
            {
                var elderlyAccount = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(elderly);

                if (elderlyAccount == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var group = await _unitOfWork.GroupMemberRepository.GetGroupOfElderly(elderly);

                if (group == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG);
                }

                var groupMember = await _unitOfWork.GroupMemberRepository.GetFamilyMemberInGroup(group.GroupId);

                if (!groupMember.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG);
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetGroupAndRelationshipInforByElderly(int elderlyId)
        {
            try
            {
                var elderlyAccount = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(elderlyId);

                if (elderlyAccount == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var requestUser = await _unitOfWork.UserLinkRepository.GetByAccount1Async(elderlyId, SD.UserLinkStatus.PENDING);
                var requestUserAccount = requestUser.Where(r => r.AccountId2 != elderlyAccount.AccountId && r.RelationshipType.Equals("Family")).Select(r => r.AccountId2Navigation).ToList();
                var mapRequestUser = _mapper.Map<List<UserDTO>>(requestUserAccount);                
                
                var responseUser = await _unitOfWork.UserLinkRepository.GetByAccount2Async(elderlyId, SD.UserLinkStatus.PENDING);
                var responseUserAccount = responseUser.Where(r => r.AccountId1 != elderlyAccount.AccountId && r.RelationshipType.Equals("Family")).Select(r => r.AccountId1Navigation).ToList();
                var mapResponseUser = _mapper.Map<List<UserDTO>>(responseUserAccount);

                var group = await _unitOfWork.GroupMemberRepository.GetGroupOfElderly(elderlyAccount.AccountId);

                var groupMember = await _unitOfWork.GroupMemberRepository.GetByGroupIdAsync(group.GroupId);

                var userInGroup = groupMember.Select(gm => gm.Account).ToList();

                var mapUserInGroup = _mapper.Map<List<UserDTO>>(userInGroup);

                var familyInGroup = groupMember.Where(gm => gm.Account.AccountId != elderlyAccount.AccountId).Select(gm => gm.Account).ToList();

                var familyInGroupIds = familyInGroup.Select(a => a.AccountId).ToList();

                var allElderlyUserLinks = await _unitOfWork.UserLinkRepository.GetByUserIdAsync(elderlyAccount.AccountId, SD.UserLinkStatus.ACCEPTED);

                var familyNotInGroup = allElderlyUserLinks
                    .SelectMany(link => new[] { link.AccountId1, link.AccountId2 })
                    .Distinct()
                    .Where(accountId => !familyInGroupIds.Contains(accountId) && accountId != elderlyAccount.AccountId)
                    .ToList();

                var accountFamilyNotInGroup = _unitOfWork.AccountRepository.GetAll().Where(a => familyNotInGroup.Contains(a.AccountId)).ToList();

                var mapFamilyNotInGroup = _mapper.Map<List<UserDTO>>(accountFamilyNotInGroup);

                if (allElderlyUserLinks == null)
                {
                    allElderlyUserLinks = new List<UserLink>();
                }

                var result = new GetGroupAndRelationshipInforByElderly
                {
                    RequestUsers = mapRequestUser,
                    ResponseUsers = mapResponseUser,
                    FamilyNotInGroup = mapFamilyNotInGroup,
                    GroupInfor = new GroupInfor
                    {
                        GroupId = group.GroupId,
                        GroupName = group.GroupName,
                        UsersInGroup = mapUserInGroup
                    }
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetGroupAndRelationshipInforByFamily(int familyMemberId)
        {
            try
            {
                var familyMemberAccount = await _unitOfWork.AccountRepository.GetFamilyMemberByAccountIDAsync(familyMemberId);

                if (familyMemberAccount == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Elderly does not exist!");
                }

                var requestUser = await _unitOfWork.UserLinkRepository.GetByAccount1Async(familyMemberAccount.AccountId, SD.UserLinkStatus.PENDING);
                var requestUserAccount = requestUser.Where(r => r.AccountId2 != familyMemberAccount.AccountId && r.RelationshipType.Equals("Family")).Select(r => r.AccountId2Navigation).ToList();
                var mapRequestUser = _mapper.Map<List<UserDTO>>(requestUserAccount);

                var responseUser = await _unitOfWork.UserLinkRepository.GetByAccount2Async(familyMemberAccount.AccountId, SD.UserLinkStatus.PENDING);
                var responseUserAccount = responseUser.Where(r => r.AccountId1 != familyMemberAccount.AccountId && r.RelationshipType.Equals("Family")).Select(r => r.AccountId1Navigation).ToList();
                var mapResponseUser = _mapper.Map<List<UserDTO>>(responseUserAccount);

                var groups = await _unitOfWork.GroupMemberRepository.GetGroupOfFamilyMember(familyMemberAccount.AccountId);

                var listGroupInfor = new List<GroupInfor>();

                var listTotalUserNotInGroup = new List<UserDTO>();
                var allUserIdsInAnyGroup = new List<int>();

                foreach (var group in groups)
                {
                    var groupMember = await _unitOfWork.GroupMemberRepository.GetByGroupIdAsync(group.GroupId);

                    var userInGroup = groupMember.Select(gm => gm.Account).ToList();

                    allUserIdsInAnyGroup.AddRange(userInGroup.Select(uig => uig.AccountId));

                    var mapUserInGroup = _mapper.Map<List<UserDTO>>(userInGroup);

                    listGroupInfor.Add(new GroupInfor
                    {
                        GroupId = group.GroupId,
                        GroupName = group.GroupName,
                        UsersInGroup = mapUserInGroup
                    });

                    var familyInGroup = groupMember.Where(gm => gm.Account.AccountId != familyMemberAccount.AccountId).Select(gm => gm.Account).ToList();

                    var familyInGroupIds = familyInGroup.Select(a => a.AccountId).ToList();

                    var allElderlyUserLinks = await _unitOfWork.UserLinkRepository.GetByUserIdAsync(familyMemberAccount.AccountId, SD.UserLinkStatus.ACCEPTED);

                    var familyNotInGroup = allElderlyUserLinks
                        .SelectMany(link => new[] { link.AccountId1, link.AccountId2 })
                        .Distinct()
                        .Where(accountId => !familyInGroupIds.Contains(accountId) && accountId != familyMemberAccount.AccountId)
                        .ToList();

                    var accountFamilyNotInGroup = _unitOfWork.AccountRepository.GetAll().Where(a => familyNotInGroup.Contains(a.AccountId)).ToList();

                    var mapFamilyNotInGroup = _mapper.Map<List<UserDTO>>(accountFamilyNotInGroup);

                    if (allElderlyUserLinks == null)
                    {
                        allElderlyUserLinks = new List<UserLink>();
                    }

                    listTotalUserNotInGroup.AddRange(mapFamilyNotInGroup);
                }

                listTotalUserNotInGroup = listTotalUserNotInGroup
                    .Where(user => !allUserIdsInAnyGroup.Contains(user.AccountId))
                    .DistinctBy(a => a.AccountId)  // Ensure no duplicates
                    .ToList();

                var result = new GetGroupAndRelationshipInforByFamilyMember
                {
                    RequestUsers = mapRequestUser,
                    ResponseUsers = mapResponseUser,
                    FamilyNotInGroup = listTotalUserNotInGroup,
                    GroupInfors = listGroupInfor
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }
    }
}
