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
using System.Linq;

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
        Task<IBusinessResult> GetFamilyNotInGroup(int familyMemberId);
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
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "ID tài khoản không hợp lệ.");
                }

                var familyMember = _unitOfWork.AccountRepository.FindByCondition(a => a.AccountId == accountId && a.RoleId == 3 && a.Status.Equals(SD.GeneralStatus.ACTIVE)).FirstOrDefault();

                if (familyMember == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Thành viên gia đình không tồn tại");
                }

                var userGroups = _unitOfWork.GroupMemberRepository.GetAll()
                    .Where(gm => gm.AccountId == accountId && gm.Status == SD.GeneralStatus.ACTIVE)
                    .Select(gm => gm.GroupId)
                    .ToList();

                if (!userGroups.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Người dùng không thuộc nhóm nào.");
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
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Không tìm thấy thành viên hoạt động trong nhóm.");
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Lỗi không mong muốn: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> ChangeGroupName(ChangeGroupNameRequest req)
        {
            try
            {
                if (req.GroupId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "ID nhóm không hợp lệ.");
                }

                var group = _unitOfWork.GroupRepository.FindByCondition(a => a.GroupId == req.GroupId && a.Status.Equals(SD.GeneralStatus.ACTIVE)).FirstOrDefault();

                if (group == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Nhóm không tồn tại");
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
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Lỗi không mong muốn: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateGroup(CreateGroupRequest request)
        {
            try
            {
                if (request == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Yêu cầu không được để trống.");
                }

                if (string.IsNullOrWhiteSpace(request.GroupName))
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Tên nhóm không được để trống.");
                }

                if (request.CreatorAccountId <= 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "ID người tạo không hợp lệ.");
                }

                var checkCreator = await _unitOfWork.AccountRepository.GetByIdAsync(request.CreatorAccountId);

                if (checkCreator.RoleId != 3)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Người tạo không phải thành viên gia đình");
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
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Nhóm với các thành viên này đã tồn tại.");
                }

                var memberIds = request.Members.Select(m => m.AccountId).ToList();

                var numberOfElderly = 0;

                foreach (var memberId in memberIds)
                {
                    var elderlyCheck = await _unitOfWork.AccountRepository.GetByIdAsync(memberId);

                    if (elderlyCheck.RoleId == 2)
                    {
                        var elderly = _unitOfWork.GroupMemberRepository.FindByCondition(gm => gm.AccountId == elderlyCheck.AccountId).FirstOrDefault();

                        if (elderly != null)
                        {
                            return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, $"Người già đã ở trong nhóm gia đình khác!");
                        }

                        numberOfElderly++;
                    }
                }

                if (numberOfElderly == 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, $"Phải có ít nhất một người già!");
                }

                var allPairs = GetUniquePairs(memberIds);

                foreach (var pair in allPairs)
                {
                    var relationshipExists = _unitOfWork.UserLinkRepository.GetAll()
                        .Any(ul => (ul.AccountId1 == pair.Item1 && ul.AccountId2 == pair.Item2 && ul.RelationshipType.Equals("Family")) ||
                                   (ul.AccountId1 == pair.Item2 && ul.AccountId2 == pair.Item1) && ul.RelationshipType.Equals("Family"));

                    if (!relationshipExists)
                    {
                        var fullName1 = _unitOfWork.AccountRepository.GetById(pair.Item1);
                        var fullName2 = _unitOfWork.AccountRepository.GetById(pair.Item2);

                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, $"Thành viên {fullName1.FullName} và {fullName2.FullName} không có mối quan hệ gia đình.");
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
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Lỗi khi tạo!");
                    }
                } 

                var roomCreateRs = await CreateRoomChat(request.Members, group.GroupName);

                if (roomCreateRs.Status < 1)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, roomCreateRs.Message);
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

                    if (request.Members.Count == 2)
                    {
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
                }

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, "Tạo nhóm thành công.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Lỗi không mong muốn: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateRoomChat(List<GroupMemberRequest> groupMembers, string groupName)
        {
            try
            {
                if (groupMembers.Count < 2)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Cần ít nhất hai thành viên để tạo phòng chat.");
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
                        { "IsProfessorChat", false },
                        { "IsDisabled", false },
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

                return new BusinessResult(Const.SUCCESS_CREATE, "Tạo phòng chat thành công.", groupId);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Lỗi không mong muốn: " + ex.Message);
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
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "ID nhóm không hợp lệ.");
                }

                if (req.MemberIds == null || !req.MemberIds.Any())
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Danh sách thành viên không được để trống.");
                }

                var group = await _unitOfWork.GroupRepository.GetByIdAsync(req.GroupId);
                if (group == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Nhóm không tồn tại.");
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

                if (existingMembers.Any())
                {
                    var existingMemberIds = existingMembers.Select(gm => gm.AccountId).ToList();

                    foreach (var memberId in req.MemberIds)
                    {
                        if (existingMemberIds.Contains(memberId))
                        {
                            return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, $"Người dùng đã ở trong nhóm.");
                        }
                    }
                }

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
                            return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, $"Không thể kích hoạt lại thành viên {duplicateMember.AccountId}.");
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
                            return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, $"Không thể tạo liên kết người dùng.");
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
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Không thể thêm thành viên vào nhóm.");
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
                            return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Thành viên đã có trong nhóm này!");
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
                        { "IsProfessorChat", false },
                        { "IsDisabled", false },
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

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, "Thêm thành viên vào nhóm thành công.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Lỗi không mong muốn: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetGroupsByAccountId(int accountId)
        {
            try
            {
                var isExisted = _unitOfWork.AccountRepository.GetById(accountId);

                if (isExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Không tìm thấy tài khoản.");
                }

                var groupMembers = await _unitOfWork.GroupMemberRepository
                    .GetGroupMembersByAccountIdAsync(accountId);

                if (groupMembers == null || !groupMembers.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Không tìm thấy nhóm nào cho ID tài khoản này.");
                }

                var rs = _mapper.Map<List<GroupMemberDTO>>(groupMembers);

                return new BusinessResult(Const.SUCCESS_READ, "Lấy thông tin nhóm thành công.", rs);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Lỗi không mong muốn: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> RemoveMemberFromGroup(int kickerId, int groupId, int accountId)
        {
            try
            {
                var groupMember = await _unitOfWork.GroupMemberRepository
                    .GetByGroupIdAndAccountIdAsync(groupId, accountId);

                if (groupMember == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Không tìm thấy thành viên nhóm.");
                }

                var isMemberCreator = _unitOfWork.GroupMemberRepository.FindByCondition(gm => gm.AccountId == accountId && gm.GroupId == groupId).Select(gm => gm.IsCreator).FirstOrDefault();

                if (isMemberCreator)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Không thể xóa người tạo nhóm!");
                }

                groupMember.Status = SD.GeneralStatus.INACTIVE;

                if (kickerId != accountId)
                {
                    var isKickerCreator = _unitOfWork.GroupMemberRepository.FindByCondition(gm => gm.AccountId == kickerId && gm.GroupId == groupId).Select(gm => gm.IsCreator).FirstOrDefault();

                    if (!isKickerCreator)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Chỉ người tạo nhóm mới có quyền thực hiện hành động này!");
                    }
                }

                var allGroupMembers = _unitOfWork.GroupMemberRepository.FindByCondition(gm => gm.GroupId == groupId).Select(gm => gm.AccountId).ToList();

                int countRole2 = 0;

                foreach (var groupMemberId in allGroupMembers)
                {
                    var account = await _unitOfWork.AccountRepository.GetAccountAsync(groupMemberId);

                    if (account.RoleId == 2)
                    {
                        countRole2++;
                    }
                }

                var kickedAccount = await _unitOfWork.AccountRepository.GetAccountAsync(accountId);

                if (countRole2 == 1)
                {
                    if (kickerId == kickedAccount.AccountId && kickedAccount.RoleId == 2)
                    {
                        await RemoveGroup(groupId);
                        return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG, "Xóa thành viên khỏi nhóm thành công.");
                    }
                    else if (kickedAccount.RoleId == 2)
                    {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Không thể xóa người già này vì phải có ít nhất một người già trong nhóm!");
                    }
                }
                else if (countRole2 < 1)
                {
                    await RemoveGroup(groupId);
                    return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG, "Xóa thành viên khỏi nhóm thành công.");
                }

                var group = await _unitOfWork.GroupRepository.GetByIdAsync(groupId);
                if (group == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Nhóm không tồn tại.");
                }

                var roomChatId = group.GroupChatId;

                if (roomChatId != null)
                {
                    var groupRef = _firestoreDb.Collection("ChatRooms").Document(roomChatId);
                    var groupDoc = await groupRef.GetSnapshotAsync();

                    if (!groupDoc.Exists)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Nhóm chat không tồn tại!");
                    }

                    var currentMembers = groupDoc.GetValue<Dictionary<string, object>>("MemberIds") ?? new Dictionary<string, object>();
                    var memberIdStr = accountId.ToString();

                    if (!currentMembers.ContainsKey(memberIdStr))
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Thành viên không thuộc nhóm này!");
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
                            return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, $"Không thể xóa liên kết người dùng.");
                        }
                    }
                }

                await _unitOfWork.GroupMemberRepository.RemoveAsync(groupMember);

                return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG, "Xóa thành viên khỏi nhóm thành công.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Lỗi không mong muốn: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> RemoveGroup(int groupId)
        {
            try
            {
                var group = await _unitOfWork.GroupRepository.GetByIdAsync(groupId);

                if (group == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Không tìm thấy nhóm.");
                }

                var removeAllGroupMember = await _unitOfWork.GroupMemberRepository.RemoveAllGroupMember(group.GroupId);

                var removeGroup = await _unitOfWork.GroupRepository.RemoveAsync(group);

                if (!removeGroup)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG, "Xóa nhóm thất bại.");
                }

                var roomChatId = group.GroupChatId;
                var groupRef = _firestoreDb.Collection("ChatRooms").Document(roomChatId);
                await groupRef.DeleteAsync();

                return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG, "Xóa nhóm thành công.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, Const.FAIL_UPDATE_MSG, "Lỗi không mong muốn: " + ex.Message);
            }
        }


        public async Task<IBusinessResult> GetMembersByGroupId(int groupId)
        {
            try
            {
                var groupMembers = await _unitOfWork.GroupMemberRepository.GetByGroupIdAsync(groupId);

                if (groupMembers == null || !groupMembers.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Không tìm thấy thành viên nào trong nhóm.");
                }

                var rs = _mapper.Map<List<GroupMemberDTO>>(groupMembers);

                return new BusinessResult(Const.SUCCESS_READ, "Lấy thông tin thành viên nhóm thành công.", rs);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Lỗi không mong muốn: " + ex.Message);
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
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Nhóm chat không tồn tại.");
                }

                var memberIds = documentSnapshot.GetValue<Dictionary<string, object>>("MemberIds")?.Keys
                    .Select(int.Parse)
                    .ToList();

                if (memberIds == null || !memberIds.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Không tìm thấy thành viên nào trong nhóm chat.");
                }

                var familyGroups = _unitOfWork.GroupMemberRepository.GetAll()
                    .Where(gm => gm.Status == SD.GeneralStatus.ACTIVE)
                    .GroupBy(gm => gm.GroupId)
                    .Where(g => memberIds.All(id => g.Any(gm => gm.AccountId == id)))
                    .Select(g => g.Key)
                    .ToList();

                if (!familyGroups.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Không tìm thấy nhóm gia đình hợp lệ nào.");
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
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Lỗi không mong muốn: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllGroupMembersByUserId(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "ID người dùng không hợp lệ.");
                }

                var userGroups = _unitOfWork.GroupMemberRepository.GetAll()
                    .Where(gm => gm.AccountId == userId && gm.Status == SD.GeneralStatus.ACTIVE)
                    .Select(gm => gm.GroupId)
                    .ToList();

                if (!userGroups.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Người dùng không thuộc nhóm nào.");
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
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Không tìm thấy thành viên hoạt động trong nhóm.");
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Lỗi không mong muốn: " + ex.Message);
            }
        }


        public async Task<IBusinessResult> CheckIfElderlyInGroup(int elderly)
        {
            try
            {
                var elderlyAccount = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(elderly);

                if (elderlyAccount == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Người già không tồn tại!");
                }

                var group = await _unitOfWork.GroupMemberRepository.GetGroupOfElderly(elderly);

                if (group == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Nhóm không tồn tại!");
                }

                var groupMember = await _unitOfWork.GroupMemberRepository.GetFamilyMemberInGroup(group.GroupId);

                if (!groupMember.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Lỗi khi tìm nhóm!");
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Lỗi không mong muốn: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetGroupAndRelationshipInforByElderly(int elderlyId)
        {
            try
            {
                var elderlyAccount = await _unitOfWork.AccountRepository.GetElderlyByAccountIDAsync(elderlyId);

                if (elderlyAccount == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Người già không tồn tại!");
                }

                var requestUser = await _unitOfWork.UserLinkRepository.GetByAccount1Async(elderlyId, SD.UserLinkStatus.PENDING);
                var requestUserAccount = requestUser.Where(r => r.AccountId2 != elderlyAccount.AccountId && r.RelationshipType.Equals("Family")).Select(r => r.AccountId2Navigation).ToList();
                var mapRequestUser = _mapper.Map<List<UserDTO>>(requestUserAccount);

                var responseUser = await _unitOfWork.UserLinkRepository.GetByAccount2Async(elderlyId, SD.UserLinkStatus.PENDING);
                var responseUserAccount = responseUser.Where(r => r.AccountId1 != elderlyAccount.AccountId && r.RelationshipType.Equals("Family")).Select(r => r.AccountId1Navigation).ToList();
                var mapResponseUser = _mapper.Map<List<UserDTO>>(responseUserAccount);

                var group = await _unitOfWork.GroupMemberRepository.GetGroupOfElderly(elderlyAccount.AccountId);

                var allElderlyUserLinks = await _unitOfWork.UserLinkRepository.GetByUserIdAsync(elderlyAccount.AccountId, SD.UserLinkStatus.ACCEPTED);

                if (group == null)
                {
                    var allUserLinks = allElderlyUserLinks.SelectMany(link => new[] { link.AccountId1, link.AccountId2 }).Where(u => u != elderlyAccount.AccountId).Distinct().ToList();

                    var allUserLinkAccount = _unitOfWork.AccountRepository.GetAll().Where(a => allUserLinks.Contains(a.AccountId)).ToList();

                    var allUserLinkAccountMap = _mapper.Map<List<UserDTO>>(allUserLinkAccount);

                    var result1 = new GetGroupAndRelationshipInforByElderly
                    {
                        RequestUsers = mapRequestUser,
                        ResponseUsers = mapResponseUser,
                        FamilyNotInGroup = allUserLinkAccountMap,
                        GroupInfor = null
                    };

                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, result1);
                }

                var groupMember = await _unitOfWork.GroupMemberRepository.GetByGroupIdAsync(group.GroupId);

                var userInGroup = groupMember.Select(gm => gm.Account).ToList();

                var mapUserInGroup = _mapper.Map<List<UserDTO>>(userInGroup);

                var familyInGroup = groupMember.Where(gm => gm.Account.AccountId != elderlyAccount.AccountId).Select(gm => gm.Account).ToList();

                var familyInGroupIds = familyInGroup.Select(a => a.AccountId).ToList();


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
                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Lỗi không mong muốn: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetGroupAndRelationshipInforByFamily(int familyMemberId)
        {
            try
            {
                var familyMemberAccount = await _unitOfWork.AccountRepository.GetFamilyMemberByAccountIDAsync(familyMemberId);

                if (familyMemberAccount == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Thành viên gia đình không tồn tại!");
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

                var allElderlyUserLinks = await _unitOfWork.UserLinkRepository.GetByUserIdAsync(familyMemberAccount.AccountId, SD.UserLinkStatus.ACCEPTED);

                if (groups.Any())
                {
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

                }
                else
                {
                    var allElderlyUserLinksIds = allElderlyUserLinks.SelectMany(link => new[] { link.AccountId1, link.AccountId2 }).Where(u => u != familyMemberId).Distinct().ToList();

                    var familyNotInGroupIds = new List<int>();

                    foreach (var us in allElderlyUserLinksIds)
                    {
                        var account = await _unitOfWork.AccountRepository.GetAccountAsync(us);

                        var groupOfElderly = await _unitOfWork.GroupMemberRepository.GetGroupOfElderly(us);

                        if (account != null && account.RoleId == 2 && groupOfElderly == null)
                        {
                            familyNotInGroupIds.Add(us);
                        }
                        else if (account != null && account.RoleId == 3)
                        {
                            familyNotInGroupIds.Add(us);
                        }
                    }

                    var accountFamilyNotInGroup = _unitOfWork.AccountRepository.GetAll().Where(a => familyNotInGroupIds.Contains(a.AccountId)).ToList();

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
                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Lỗi không mong muốn: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetFamilyNotInGroup(int familyMemberId)
        {
            try
            {
                var familyMemberAccount = await _unitOfWork.AccountRepository.GetFamilyMemberByAccountIDAsync(familyMemberId);

                if (familyMemberAccount == null || familyMemberAccount.RoleId != 3)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Thành viên gia đình không tồn tại!");
                }

                var groups = await _unitOfWork.GroupMemberRepository.GetGroupOfFamilyMember(familyMemberAccount.AccountId);

                var listTotalUserNotInGroup = new List<UserDTO>();
                var allUserIdsInAnyGroup = new List<int>();

                var allElderlyUserLinks = await _unitOfWork.UserLinkRepository.GetByUserIdAsync(familyMemberAccount.AccountId, SD.UserLinkStatus.ACCEPTED);

                if (groups.Any())
                {
                    foreach (var group in groups)
                    {
                        var groupMember = await _unitOfWork.GroupMemberRepository.GetByGroupIdAsync(group.GroupId);

                        var userInGroup = groupMember.Select(gm => gm.Account).ToList();

                        allUserIdsInAnyGroup.AddRange(userInGroup.Select(uig => uig.AccountId));

                        var mapUserInGroup = _mapper.Map<List<UserDTO>>(userInGroup);

                        var familyInGroup = groupMember.Where(gm => gm.Account.AccountId != familyMemberAccount.AccountId).Select(gm => gm.Account).ToList();

                        var familyInGroupIds = familyInGroup.Select(a => a.AccountId).ToList();

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
                }
                else
                {
                    var allElderlyUserLinksIds = allElderlyUserLinks.SelectMany(link => new[] { link.AccountId1, link.AccountId2 }).Where(u => u != familyMemberId).Distinct().ToList();

                    var familyNotInGroupIds = new List<int>();

                    foreach (var us in allElderlyUserLinksIds)
                    {
                        var account = await _unitOfWork.AccountRepository.GetAccountAsync(us);

                        var groupOfElderly = await _unitOfWork.GroupMemberRepository.GetGroupOfElderly(us);

                        if (account != null && account.RoleId == 2 && groupOfElderly == null)
                        {
                            familyNotInGroupIds.Add(us);
                        }
                        else if (account != null && account.RoleId == 3)
                        {
                            familyNotInGroupIds.Add(us);
                        }
                    }

                    var accountFamilyNotInGroup = _unitOfWork.AccountRepository.GetAll().Where(a => familyNotInGroupIds.Contains(a.AccountId)).ToList();

                    var mapFamilyNotInGroup = _mapper.Map<List<UserDTO>>(accountFamilyNotInGroup);

                    if (allElderlyUserLinks == null)
                    {
                        allElderlyUserLinks = new List<UserLink>();
                    }

                    listTotalUserNotInGroup.AddRange(mapFamilyNotInGroup);
                }

                listTotalUserNotInGroup = listTotalUserNotInGroup
                    .Where(user => !(user.RoleId == 2 && allUserIdsInAnyGroup.Contains(user.AccountId)))
                    .DistinctBy(a => a.AccountId)
                    .ToList();

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, listTotalUserNotInGroup);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Lỗi không mong muốn: " + ex.Message);
            }
        }
    }
}