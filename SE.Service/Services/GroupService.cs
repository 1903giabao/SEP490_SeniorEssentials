using AutoMapper;
using SE.Common.Request;
using SE.Common;
using SE.Data.Models;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using SE.Common.Enums;
using SE.Common.Request.SE.Common.Request;
using Google.Cloud.Firestore;

namespace SE.Service.Services
{
    public interface IGroupService
    {
        Task<IBusinessResult> CreateGroup(CreateGroupRequest request);
        Task<IBusinessResult> GetGroupsByAccountId(int accountId);
        Task<IBusinessResult> RemoveMemberFromGroup(int groupId, int accountId);
        Task<IBusinessResult> RemoveGroup(int groupId);
        Task<IBusinessResult> GetMembersByGroupId(int groupId);
    }

    public class GroupService : IGroupService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly FirestoreDb _firestoreDb;
        public GroupService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firestoreDb = FirestoreDb.Create("testproject-bc2e2");
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

                var group = _mapper.Map<Group>(request);
                group.CreatedDate = DateTime.UtcNow;
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

                for (int i = 0; i < groupMembers.Count; i++)
                {
                    for (int j = i + 1; j < groupMembers.Count; j++)
                    {
                        var member1 = groupMembers[i];
                        var member2 = groupMembers[j];

                        DocumentReference pairChatRoomRef = _firestoreDb.Collection("ChatRooms").Document(); 

                        var pairChatRoomData = new Dictionary<string, object>
                        {
                            { "CreatedAt", DateTime.UtcNow },
                            { "IsOnline", false }, 
                            { "IsGroupChat", false },
                            { "RoomName", groupName },
                            { "RoomAvatar", "" },
                            { "SenderID", 0 },
                            { "LastMessage", "" },
                            { "SentTime",  null },
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

                if (groupMembers.Count > 2)
                {
                    DocumentReference groupChatRoomRef = _firestoreDb.Collection("ChatRooms").Document(); 

                    var groupChatRoomData = new Dictionary<string, object>
                    {
                        { "CreatedAt", DateTime.UtcNow },
                        { "IsOnline", false },
                        { "IsGroupChat", true },
                        { "RoomName", groupName },
                        { "RoomAvatar", "" },
                        { "SenderID", 0 },
                        { "LastMessage", "" },
                        { "SentTime", null },
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

                return new BusinessResult(Const.SUCCESS_CREATE, "Chat rooms created successfully.");
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

        public async Task<IBusinessResult> RemoveMemberFromGroup(int groupId, int accountId)
        {
            try
            {
                var groupMember = await _unitOfWork.GroupMemberRepository
                    .GetByGroupIdAndAccountIdAsync(groupId, accountId);

                if (groupMember == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Group member not found.");
                }

                groupMember.Status = SD.GeneralStatus.INACTIVE;

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


    }
}
