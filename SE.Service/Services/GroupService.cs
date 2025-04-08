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

namespace SE.Service.Services
{
    public interface IGroupService
    {
        Task<IBusinessResult> GetAllElderlyByFamilyMemberId(int accountId);
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

    }

    public class GroupService : IGroupService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly FirestoreDb _firestoreDb;
        private readonly IVideoCallService _videoCallService;
        public GroupService(UnitOfWork unitOfWork, IMapper mapper, FirestoreDb firestoreDb, IVideoCallService videoCallService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firestoreDb = firestoreDb;
            _videoCallService = videoCallService;
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

                var group = _mapper.Map<Group>(request);
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

                if (groupMembers.Count > 2)
                {
                    DocumentReference groupChatRoomRef = _firestoreDb.Collection("ChatRooms").Document(); 

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

                return new BusinessResult(Const.SUCCESS_CREATE, "Chat rooms created successfully.");
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
                    var elderlyCheck = await _unitOfWork.AccountRepository.GetByIdAsync(memberId);

                    if (elderlyCheck.RoleId == 2)
                    {
                        var elderly = _unitOfWork.GroupMemberRepository.FindByCondition(gm => gm.AccountId == elderlyCheck.AccountId && gm.Status.Equals(SD.GeneralStatus.ACTIVE)).FirstOrDefault();

                        if (elderly != null)
                        {
                            return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, $"Người già đã ở trong nhóm gia đình khác!");
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
                            Status = SD.GeneralStatus.ACTIVE,
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

                var roomChatId = await _videoCallService.FindChatRoomContainingAllUsers(allGroupMembers);

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
    }
}
