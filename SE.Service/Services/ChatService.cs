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
using Firebase.Auth;
using SE.Service.Helper;

namespace SE.Service.Services
{
    public interface IChatService
    {
        Task<IBusinessResult> SendMessage(SendMessageRequest req);
        Task<IBusinessResult> ReplyMessage(ReplyMessageRequest req);
        Task<IBusinessResult> GetAllMessages(string roomId);
        Task<IBusinessResult> GetAllRoomChat(int userId);
        Task<IBusinessResult> MarkMessagesAsSeen(string roomId, long currentUserId);
        Task<IBusinessResult> ChangeStatus(int userId);
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

                var urlLink = ("", "");

                if (req.MessageType.Equals("Image"))
                {
                    urlLink = await CloudinaryHelper.UploadImageAsync(req.FileMessage);
                }
                else if (req.MessageType.Equals("Video"))
                {
                    urlLink = await CloudinaryHelper.UploadVideoAsync(req.FileMessage);
                }
                else if (req.MessageType.Equals("Audio"))
                {
                    urlLink = await CloudinaryHelper.UploadAudioAsync(req.FileMessage);
                }

                if (req.MessageType.Equals("Text") || req.MessageType.Equals("Gif")) 
                {
                    var newMessage = new
                    {
                        SenderId = req.SenderId,
                        SenderName = user.FullName,
                        Message = req.Message,
                        MessageType = req.MessageType,
                        SentTime = DateTime.UtcNow,
                        IsSeen = false,
                    };

                    await messagesRef.AddAsync(newMessage);

                    await chatRef.UpdateAsync(new Dictionary<string, object>
                    {
                        { "LastMessage", req.Message },
                        { "SentTime", DateTime.UtcNow },
                        { "SentTime", req.SenderId },
                    });

                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, newMessage);
                }
                else
                {
                    var newMessage = new
                    {
                        SenderId = req.SenderId,
                        SenderName = user.FullName,
                        Message = urlLink.Item2,
                        MessageType = req.MessageType,
                        SentTime = DateTime.UtcNow,
                        IsSeen = false,
                    };

                    await messagesRef.AddAsync(newMessage);

                    await chatRef.UpdateAsync(new Dictionary<string, object>
                    {
                        { "LastMessage", req.Message },
                        { "SentTime", DateTime.UtcNow },
                        { "SentTime", req.SenderId },
                    });

                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, newMessage);
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> ReplyMessage(ReplyMessageRequest req)
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

                DocumentReference messageInRoom = chatRef.Collection("Messages").Document(req.RepliedMessageId);
                DocumentSnapshot messageSnapshot = await userInRoom.GetSnapshotAsync();

                if (!messageSnapshot.Exists)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Message is not in this room chat!");
                }

                CollectionReference messagesRef = chatRef.Collection("Messages");

                var urlLink = ("", "");

                if (req.FileMessage != null)
                {
                    if (req.MessageType.Equals("Image"))
                    {
                        urlLink = await CloudinaryHelper.UploadImageAsync(req.FileMessage);
                    }
                    else if (req.MessageType.Equals("Video"))
                    {
                        urlLink = await CloudinaryHelper.UploadVideoAsync(req.FileMessage);
                    }
                    else if (req.MessageType.Equals("Audio"))
                    {
                        urlLink = await CloudinaryHelper.UploadAudioAsync(req.FileMessage);
                    }
                }

                if (req.MessageType.Equals("Text") || req.MessageType.Equals("Gif"))
                {
                    var newMessage = new
                    {
                        SenderId = req.SenderId,
                        SenderName = user.FullName,
                        RepliedMessageId = req.RepliedMessageId,
                        RepliedMessage = req.RepliedMessage,
                        RepliedMessageType = req.RepliedMessageType,
                        ReplyTo = req.ReplyTo,
                        Message = req.Message,
                        MessageType = req.MessageType,
                        SentTime = DateTime.UtcNow,
                        IsSeen = false,
                    };

                    await messagesRef.AddAsync(newMessage);

                    await chatRef.UpdateAsync(new Dictionary<string, object>
                    {
                        { "LastMessage", req.Message },
                        { "SentTime", DateTime.UtcNow },
                        { "SenderID", req.SenderId },
                    });

                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, newMessage);
                }
                else
                {
                    var newMessage = new
                    {
                        SenderId = req.SenderId,
                        SenderName = user.FullName,
                        RepliedMessageId = req.RepliedMessageId,
                        RepliedMessage = req.RepliedMessage,
                        RepliedMessageType = req.RepliedMessageType,
                        Message = urlLink.Item2,
                        MessageType = req.MessageType,
                        SentTime = DateTime.UtcNow,
                        IsSeen = false,
                    };

                    await messagesRef.AddAsync(newMessage);

                    await chatRef.UpdateAsync(new Dictionary<string, object>
                    {
                        { "LastMessage", req.Message },
                        { "SentTime", DateTime.UtcNow },
                        { "SenderID", req.SenderId },
                    });

                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, newMessage);
                }
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
                        ReplyTo = messageData.ContainsKey("ReplyTo") ? messageData["ReplyTo"].ToString() : string.Empty,
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
                    var roomName = string.Empty; 
                    var roomAvatar = string.Empty;
                    var isOnline = false;
                    List<string> memberIdsList = new List<string>();

                    if (data.TryGetValue("MemberIds", out var memberIdsObj) &&
                        memberIdsObj is IDictionary<string, object> memberIds)
                    {
                        numberOfMems = memberIds.Count;

                        if (numberOfMems == 2)
                        {
                            foreach (var member in memberIds)
                            {
                                if (!member.Key.Equals(userId.ToString()))
                                {
                                    var getUser = await _unitOfWork.AccountRepository.GetByIdAsync(int.Parse(member.Key));

                                    if (getUser == null) 
                                    {
                                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                                    }

                                    DocumentReference onlineRef = _firestoreDb.Collection("OnlineMembers").Document(member.Key);

                                    DocumentSnapshot onlineSnapshot = await onlineRef.GetSnapshotAsync();

                                    if (!onlineSnapshot.Exists)
                                    {
                                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not belong to this room chat!");
                                    }

                                    var onlineData = onlineSnapshot.ToDictionary();

                                    if (onlineData.TryGetValue("IsOnline", out var isOnlineCheck))
                                    {
                                        isOnline = (bool)isOnlineCheck;
                                    }                            

                                    roomName = getUser.FullName;
                                    roomAvatar = getUser.Avatar;
                                }
                            }
                        }
                    }

                    chatRooms.Add(new ChatRoomDTO
                    {
                        RoomId = document.Id,
                        CreatedAt = data["CreatedAt"]?.ToString(),
                        IsOnline = isOnline,
                        IsGroupChat = data["IsGroupChat"] as bool? ?? false,
                        RoomName = numberOfMems == 2 ? roomName : data["RoomName"].ToString(),
                        RoomAvatar = numberOfMems == 2 ? roomAvatar : data["RoomAvatar"].ToString(),
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

        public async Task<IBusinessResult> ChangeStatus(int userId)
        {
            try
            {
                var isExisted = await _unitOfWork.AccountRepository.GetByIdAsync(userId);
                if (isExisted == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                }

                var userGroups = _unitOfWork.GroupMemberRepository
                    .GetAll().Where(gm => gm.AccountId == userId).ToList();

                if (!userGroups.Any())
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, $"User {userId} is not in any group!");
                }

                CollectionReference onlineMembersRef = _firestoreDb.Collection("OnlineMembers");
                
                DocumentSnapshot userSnapshot = await onlineMembersRef.Document(userId.ToString()).GetSnapshotAsync();

                bool newStatus;

                if (userSnapshot.Exists)
                {
                    bool currentStatus = userSnapshot.TryGetValue("IsOnline", out bool isOnline) && isOnline;
                    newStatus = !currentStatus;

                    await onlineMembersRef.Document(userId.ToString()).UpdateAsync(new Dictionary<string, object>
                    {
                        { "IsOnline", newStatus }
                    });
                }
                else
                {
                    newStatus = true;
                    await onlineMembersRef.Document(userId.ToString()).SetAsync(new Dictionary<string, object>
                    {
                        { "IsOnline", newStatus }
                    });
                }

                return new BusinessResult(Const.SUCCESS_UPDATE, $"User  status updated to {(newStatus ? "Online" : "Offline")}.");
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
                    int creatorCount = req.Members.Count(m => m.IsCreator);
                    if (creatorCount != 1)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "There must be exactly one creator!");
                    }

                    if (req.Members.Count <= 2)
                    {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Group must be > 2 members");
                    }

                    req.GroupId = Guid.NewGuid().ToString();
                    var newGroupRef = _firestoreDb.Collection("ChatRooms").Document(req.GroupId);
                    var newGroupData = new Dictionary<string, object>
                    {
                            { "CreatedAt", DateTime.UtcNow },
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

                    var onlineMembersRef = _firestoreDb.Collection("OnlineMembers");

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

                            var onlineMemberData = new Dictionary<string, object>
                            {
                                { "IsOnline", true }
                            };

                            await onlineMembersRef.Document(user.AccountId.ToString()).SetAsync(onlineMemberData);
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
