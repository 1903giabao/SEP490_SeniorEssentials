using AutoMapper;
using SE.Data.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using SE.Common.Request;
using SE.Service.Base;
using SE.Common;
using Microsoft.AspNetCore.Mvc;
using SE.Common.DTO;
using Org.BouncyCastle.Ocsp;
using AutoMapper.Execution;
using SE.Data.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;

namespace SE.Service.Services
{
    public interface IChatService
    {
        Task<IBusinessResult> SendMessage(SendMessageRequest req);
        Task<IBusinessResult> GetAllMessages(string roomId);
        Task<IBusinessResult> GetAllRoomChat(int userId);
        Task<IBusinessResult> MarkMessagesAsSeen(string roomId, long currentUserId);
        Task<IBusinessResult> ChangeStatus(string roomId);
        Task<IBusinessResult> CreateGroupChat(CreateGroupChatRequest req);
    }

    public class ChatService : IChatService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly FirestoreDb _firestoreDb;
        public ChatService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firestoreDb = FirestoreDb.Create("testproject-bc2e2");
        }

        public async Task<IBusinessResult> SendMessage(SendMessageRequest req)
        {
            try
            {
                var user = await _unitOfWork.AccountRepository.GetByIdAsync(req.SenderId);

                if (user == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                }

                DocumentReference chatRef = _firestoreDb.Collection("ChatRooms").Document(req.RoomId);

                DocumentReference userInRoom = chatRef.Collection("Members").Document(req.SenderId.ToString());
                DocumentSnapshot snapshot = await userInRoom.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not belong to this room chat!");
                }

                CollectionReference messagesRef = chatRef.Collection("Messages");

                var newMessage = new
                {
                    SenderId = req.SenderId,
                    SenderName = user.FullName,
                    Message = req.Message,
                    MessageType = req.MessageType,
                    SentTime = DateTime.UtcNow,
                    IsSeen = false,
                    RepliedMessage = req.RepliedMessage,
                    RepliedTo = req.RepliedTo,
                    RepliedMessageType = req.RepliedMessageType,
                };

                await messagesRef.AddAsync(newMessage);

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, newMessage);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllMessages(string roomId)
        {
            try
            {
                CollectionReference messagesRef = _firestoreDb.Collection("ChatRooms").Document(roomId).Collection("Messages");

                QuerySnapshot snapshot = await messagesRef.OrderBy("SentTime").GetSnapshotAsync();

                List<MessageDTO> messages = new List<MessageDTO>();

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    var messageData = document.ToDictionary();
                    var message = new MessageDTO
                    {
                        SenderName = messageData.ContainsKey("SenderName") ? messageData["SenderName"].ToString() : string.Empty,
                        MessageId = document.Id,
                        Message = messageData.ContainsKey("Message") ? messageData["Message"].ToString() : string.Empty,
                        MessageType = messageData.ContainsKey("MessageType") ? messageData["MessageType"].ToString() : string.Empty,
                        SentTime = messageData.ContainsKey("SentTime") ? messageData["SentTime"].ToString() : string.Empty,
                        IsSeen = messageData.ContainsKey("IsSeen") && (bool)messageData["IsSeen"],
                        RepliedMessage = messageData.ContainsKey("RepliedMessage") ? messageData["RepliedMessage"].ToString() : string.Empty,
                        RepliedTo = messageData.ContainsKey("RepliedTo") ? messageData["RepliedTo"].ToString() : string.Empty,
                        RepliedMessageType = messageData.ContainsKey("RepliedMessageType") ? messageData["RepliedMessageType"].ToString() : string.Empty,
                    };
                    messages.Add(message);
                }

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, messages);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetAllRoomChat(int userId)
        {
            try
            {
                var user = await _unitOfWork.AccountRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                }

                var chatRoomsQuery = _firestoreDb.Collection("ChatRooms")
                    .WhereEqualTo($"MemberIds.{userId}", true);

                QuerySnapshot snapshot = await chatRoomsQuery.GetSnapshotAsync();

                var chatRooms = new List<ChatRoomDTO>();
                foreach (var document in snapshot.Documents)
                {
                    var data = document.ToDictionary();

                    int numberOfMems = 0;
                    var RoomName = string.Empty; 
                    var RoomAvatar = string.Empty;
                    List<string> memberIdsList = new List<string>();

                    if (data.TryGetValue("MemberIds", out var memberIdsObj) &&
                        memberIdsObj is IDictionary<string, object> memberIds)
                    {
                        numberOfMems = memberIds.Count;

                        if (numberOfMems == 2)
                        {
                            foreach (var key in memberIds.Keys)
                            {
                                if (!key.Equals(userId.ToString()))
                                {
                                    var getUser = await _unitOfWork.AccountRepository.GetByIdAsync(int.Parse(key));

                                    if (getUser == null) 
                                    {
                                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                                    }

                                    RoomName = getUser.FullName;
                                    RoomAvatar = getUser.Avatar;
                                }
                            }
                        }
                    }

                    chatRooms.Add(new ChatRoomDTO
                    {
                        RoomId = document.Id,
                        CreatedAt = data["CreatedAt"]?.ToString(),
                        IsOnline = data["IsOnline"] as bool? ?? false,
                        IsGroupChat = data["IsGroupChat"] as bool? ?? false,
                        RoomName = numberOfMems == 2 ? RoomName : data["RoomName"].ToString(),
                        RoomAvatar = numberOfMems == 2 ? RoomAvatar : data["RoomAvatar"].ToString(),
                        SenderId = data["SenderID"] as int?,
                        LastMessage = data["LastMessage"]?.ToString(),
                        SentTime = data["SentTime"]?.ToString(),
                        NumberOfMems = numberOfMems
                    });
                }

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, chatRooms);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> MarkMessagesAsSeen(string roomId, long currentUserId)
        {
            try
            {
                CollectionReference messagesRef = _firestoreDb.Collection("ChatRooms").Document(roomId).Collection("Messages");

                QuerySnapshot snapshot = await messagesRef.GetSnapshotAsync();

                List<Task> updateTasks = new List<Task>();

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    var messageData = document.ToDictionary();

                    if (messageData.TryGetValue("SenderId", out var senderId) && (long)senderId != currentUserId)
                    {
                        var updateData = new Dictionary<string, object>
                        {
                            { "IsSeen", true }
                        };

                        updateTasks.Add(document.Reference.UpdateAsync(updateData));
                    }
                }

                await Task.WhenAll(updateTasks);

                return new BusinessResult(Const.SUCCESS_UPDATE, "All messages marked as seen.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> ChangeStatus(string roomId)
        {
            try
            {
                DocumentReference roomRef = _firestoreDb.Collection("ChatRooms").Document(roomId);

                DocumentSnapshot roomSnapshot = await roomRef.GetSnapshotAsync();

                if (!roomSnapshot.Exists)
                {
                    return new BusinessResult(Const.FAIL_UPDATE, "Chat room does not exist.");
                }

                bool currentStatus = roomSnapshot.TryGetValue("IsOnline", out bool isOnline) && isOnline;

                bool newStatus = !currentStatus;

                var updateData = new Dictionary<string, object>
                {
                    { "IsOnline", newStatus }
                };

                await roomRef.UpdateAsync(updateData);

                return new BusinessResult(Const.SUCCESS_UPDATE, $"Chat room status updated to {(newStatus ? "Online" : "Offline")}.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, ex.Message);
            }
        }

        public async Task<IBusinessResult> CreateGroupChat(CreateGroupChatRequest req)
        {
            try
            {
                var userGroupIds = new Dictionary<int, HashSet<int>>();

                foreach (var user in req.Members)
                {
                    var isExisted = await _unitOfWork.AccountRepository.GetByIdAsync(user.AccountId);
                    if (isExisted == null)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                    }

                    var userGroups = _unitOfWork.GroupMemberRepository
                        .GetAll().Where(gm => gm.AccountId == user.AccountId).ToList();

                    if (!userGroups.Any())
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, $"User {user.AccountId} is not in any group!");
                    }

                    userGroupIds[user.AccountId] = new HashSet<int>(userGroups.Select(gm => gm.GroupId));
                }

                if (userGroupIds.Count > 0)
                {
                    var firstUserGroupIds = userGroupIds.Values.First();

                    foreach (var groupIds in userGroupIds.Values.Skip(1))
                    {
                        if (!firstUserGroupIds.Overlaps(groupIds))
                        {
                            return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Not all users are in the same group!");
                        }
                    }
                }

                if (string.IsNullOrEmpty(req.GroupId))
                {
                    if (req.Members.Count <= 2)
                    {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Group must be > 2 members");
                    }

                    req.GroupId = Guid.NewGuid().ToString();
                    var newGroupRef = _firestoreDb.Collection("ChatRooms").Document(req.GroupId);
                    var newGroupData = new Dictionary<string, object>
                    {
                            { "CreatedAt", DateTime.UtcNow },
                            { "IsOnline", false },
                            { "IsGroupChat", true },
                            { "RoomName", req.GroupName },
                            { "RoomAvatar", req.GroupAvatar },
                            { "SenderID", 0 },
                            { "LastMessage", "" },
                            { "SentTime",  null },
                            { "MemberIds", new Dictionary<string, object>() },
                    };

                    await newGroupRef.SetAsync(newGroupData);
                }
                else
                {
                    foreach (var member in req.Members)
                    {
                        member.IsCreator = false;
                    }
                }

                var groupRef = _firestoreDb.Collection("ChatRooms").Document(req.GroupId);
                var groupDoc = await groupRef.GetSnapshotAsync();
                if (groupDoc.Exists)
                {
                    var currentMembers = groupDoc.GetValue<Dictionary<string, object>>("MemberIds") ?? new Dictionary<string, object>();
                    var currentMembersList = currentMembers.Keys.ToList(); 
                    
                    var memberIdsList = new List<string>();
                    var membersRef = groupRef.Collection("Members");
                    var membersSnapshot = await membersRef.GetSnapshotAsync();

                    foreach (var user in req.Members)
                    {
                        if (!currentMembersList.Contains(user.AccountId.ToString()))
                        {
                            currentMembers[user.AccountId.ToString()] = true;

                            var memberData = new Dictionary<string, object>
                            {
                                { "IsCreator", user.IsCreator }
                            };

                            await membersRef.Document(user.AccountId.ToString()).SetAsync(memberData);
                        }
                    }

                    var updateData = new Dictionary<string, object>
                        {
                            { "MemberIds", currentMembers }
                        };

                    await groupRef.UpdateAsync(updateData);
                }
                else
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Group chat does not exist!");
                }

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, ex.Message);
            }
        }
    }
}
