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
using System.Reflection.Metadata;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using SE.Common.Enums;

namespace SE.Service.Services
{
    public interface IChatService
    {
        Task<IBusinessResult> SendMessage(SendMessageRequest req);
        Task<IBusinessResult> ReplyMessage(ReplyMessageRequest req);
        Task<IBusinessResult> GetAllMessages(string roomId);
        Task<IBusinessResult> GetAllRoomChat(int userId);
        Task<IBusinessResult> GetStatusInRoomChat(string roomId, long currentUserId);
        Task<IBusinessResult> MarkMessagesAsSeen(string roomId, long currentUserId);
        Task<IBusinessResult> ChangeStatus(int userId, bool isOnline);
        Task<IBusinessResult> CreateGroupChat(CreateGroupChatRequest req);
        Task<IBusinessResult> UpdateGroupName(string groupId, string newGroupName);
        Task<IBusinessResult> UpdateGroupAvatar(string groupId, IFormFile newGroupAvatar);
        Task<IBusinessResult> RemoveMemberFromGroup(int kickerId, string groupId, int memberId);
        Task<IBusinessResult> GetRoomChatByRoomChatId(string roomChatId, int userId);
    }

    public class ChatService : IChatService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly FirestoreDb _firestoreDb;
        private readonly IGroupService _groupService;

        public ChatService(UnitOfWork unitOfWork, IMapper mapper, FirestoreDb firestoreDb, IGroupService groupService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firestoreDb = firestoreDb;
            _groupService = groupService;
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

                var sentTime = DateTime.UtcNow.AddHours(7);

                if (req.MessageType.Equals("Text") || req.MessageType.Equals("Gif")) 
                {
                    var newMessage = new
                    {
                        SenderId = req.SenderId,
                        SenderName = user.FullName,
                        SenderAvatar = user.Avatar,
                        Message = req.Message,
                        MessageType = req.MessageType,
                        SentDate = sentTime.ToString("dd-MM-yyyy"),
                        SentTime = string.Format("{0:D2}:{1:D2}", (int)sentTime.TimeOfDay.TotalHours, sentTime.TimeOfDay.Minutes),
                        SentDateTime = sentTime.ToString(),
                        IsSeen = false,
                    };

                    await messagesRef.AddAsync(newMessage);

                    await chatRef.UpdateAsync(new Dictionary<string, object>
                    {
                        { "LastMessage", newMessage.Message },
                        { "SentDate", sentTime.ToString("dd-MM-yyyy") },
                        { "SentTime", string.Format("{0:D2}:{1:D2}", (int)sentTime.TimeOfDay.TotalHours, sentTime.TimeOfDay.Minutes) },
                        { "SentDateTime", sentTime.ToString() },
                        { "SenderId", req.SenderId },
                    });

                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, newMessage);
                }
                else
                {
                    var newMessage = new
                    {
                        SenderId = req.SenderId,
                        SenderName = user.FullName,
                        SenderAvatar = user.Avatar,
                        Message = urlLink.Item2,
                        MessageType = req.MessageType,
                        SentDate = sentTime.ToString("dd-MM-yyyy"),
                        SentTime = string.Format("{0:D2}:{1:D2}", (int)sentTime.TimeOfDay.TotalHours, sentTime.TimeOfDay.Minutes),
                        SentDateTime = sentTime.ToString(),
                        IsSeen = false,
                    };

                    await messagesRef.AddAsync(newMessage);

                    await chatRef.UpdateAsync(new Dictionary<string, object>
                    {
                        { "LastMessage", newMessage.Message },
                        { "SentDate", sentTime.ToString("dd-MM-yyyy") },
                        { "SentTime", string.Format("{0:D2}:{1:D2}",(int) sentTime.TimeOfDay.TotalHours, sentTime.TimeOfDay.Minutes) },
                        { "SentDateTime", sentTime.ToString() },
                        { "SenderId", req.SenderId },
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
                DocumentSnapshot messageSnapshot = await messageInRoom.GetSnapshotAsync();

                if (!messageSnapshot.Exists)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Message is not in this room chat!");
                }

                var repliedMessageData = messageSnapshot.ToDictionary();

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

                var sentTime = DateTime.UtcNow.AddHours(7);

                if (req.MessageType.Equals("Text") || req.MessageType.Equals("Gif"))
                {
                    var newMessage = new
                    {
                        SenderId = req.SenderId,
                        SenderName = user.FullName,
                        SenderAvatar = user.Avatar,
                        RepliedMessageId = req.RepliedMessageId,
                        RepliedMessage = repliedMessageData.ContainsKey("Message") ? repliedMessageData["Message"].ToString() : string.Empty,
                        RepliedMessageType = repliedMessageData.ContainsKey("MessageType") ? repliedMessageData["MessageType"].ToString() : string.Empty,
                        ReplyTo = repliedMessageData.ContainsKey("SenderName") ? repliedMessageData["SenderName"].ToString() : string.Empty,
                        Message = req.Message,
                        MessageType = req.MessageType,
                        SentDate = sentTime.ToString("dd-MM-yyyy"),
                        SentTime = string.Format("{0:D2}:{1:D2}", (int)sentTime.TimeOfDay.TotalHours, sentTime.TimeOfDay.Minutes),
                        SentDateTime = sentTime.ToString(),
                        IsSeen = false,
                    };

                    await messagesRef.AddAsync(newMessage);

                    await chatRef.UpdateAsync(new Dictionary<string, object>
                    {
                        { "LastMessage", newMessage.Message },
                        { "SentDate", sentTime.ToString("dd-MM-yyyy") },
                        { "SentTime", string.Format("{0:D2}:{1:D2}", (int)sentTime.TimeOfDay.TotalHours, sentTime.TimeOfDay.Minutes) },
                        { "SentDateTime", sentTime.ToString() },
                        { "SenderId", req.SenderId },
                    });

                    return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, newMessage);
                }
                else
                {
                    var newMessage = new
                    {
                        SenderId = req.SenderId,
                        SenderName = user.FullName,
                        SenderAvatar = user.Avatar,
                        RepliedMessageId = req.RepliedMessageId,
                        RepliedMessage = repliedMessageData.ContainsKey("Message") ? repliedMessageData["Message"].ToString() : string.Empty,
                        RepliedMessageType = repliedMessageData.ContainsKey("MessageType") ? repliedMessageData["MessageType"].ToString() : string.Empty,
                        ReplyTo = repliedMessageData.ContainsKey("SenderName") ? repliedMessageData["SenderName"].ToString() : string.Empty,
                        Message = urlLink.Item2,
                        MessageType = req.MessageType,
                        SentDate = sentTime.ToString("dd-MM-yyyy"),
                        SentTime = string.Format("{0:D2}:{1:D2}", (int)sentTime.TimeOfDay.TotalHours, sentTime.TimeOfDay.Minutes),
                        SentDateTime = sentTime.ToString(),
                        IsSeen = false,
                    };

                    await messagesRef.AddAsync(newMessage);

                    await chatRef.UpdateAsync(new Dictionary<string, object>
                    {
                        { "LastMessage", newMessage.Message },
                        { "SentDate", sentTime.ToString("dd-MM-yyyy") },
                        { "SentTime", string.Format("{0:D2}:{1:D2}",(int) sentTime.TimeOfDay.TotalHours, sentTime.TimeOfDay.Minutes) },
                        { "SentDateTime", sentTime.ToString() },
                        { "SenderId", req.SenderId },
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

                QuerySnapshot snapshot = await messagesRef.OrderBy("SentDateTime").GetSnapshotAsync();

                List<MessageDTO> messages = new List<MessageDTO>();

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    var messageData = document.ToDictionary();
                    var message = new MessageDTO
                    {
                        SenderId = messageData.ContainsKey("SenderId") ? (long)messageData["SenderId"] : 0,
                        SenderName = messageData.ContainsKey("SenderName") ? messageData["SenderName"].ToString() : string.Empty,
                        SenderAvatar = messageData.ContainsKey("SenderAvatar") ? messageData["SenderAvatar"].ToString() : string.Empty,
                        MessageId = document.Id,
                        Message = messageData.ContainsKey("Message") ? messageData["Message"].ToString() : string.Empty,
                        MessageType = messageData.ContainsKey("MessageType") ? messageData["MessageType"].ToString() : string.Empty,
                        SentDate = messageData.ContainsKey("SentDate") ? messageData["SentDate"].ToString() : string.Empty,
                        SentTime = messageData.ContainsKey("SentTime") ? messageData["SentTime"].ToString() : string.Empty,
                        SentDateTime = messageData.ContainsKey("SentDateTime") ? messageData["SentDateTime"].ToString() : string.Empty,
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
                    var listUser = new List<UserInRoomChatDTO>();
                    bool groupChat = false;

                    if (data.TryGetValue("MemberIds", out var memberIdsObj) &&
                        memberIdsObj is IDictionary<string, object> memberIds)
                    {
                        numberOfMems = memberIds.Count;
                        bool isGroupChat = data["IsGroupChat"] as bool? ?? false;
                        groupChat = isGroupChat;

                        foreach (var member in memberIds)
                        {
                            var getUser = await _unitOfWork.AccountRepository.GetByIdAsync(int.Parse(member.Key));
                            if (getUser == null)
                            {
                                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                            }

                            listUser.Add(new UserInRoomChatDTO
                            {
                                Id = int.Parse(member.Key),
                                Name = getUser.FullName
                            });

                            if (!isGroupChat && numberOfMems == 2)
                            {
                                if (!member.Key.Equals(userId.ToString()))
                                {
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
                            RoomName = !groupChat && numberOfMems == 2 ? roomName : data["RoomName"].ToString(),
                            RoomAvatar = !groupChat && numberOfMems == 2 ? roomAvatar : data["RoomAvatar"].ToString(),
                            SenderId = data["SenderId"] as long?,
                            LastMessage = data["LastMessage"]?.ToString(),
                            SentDate = data["SentDate"]?.ToString(),
                            SentTime = data["SentTime"]?.ToString(),
                            SentDateTime = data["SentDateTime"]?.ToString(),
                            NumberOfMems = numberOfMems,
                            Users = listUser
                        });
                    }

                    var orderedChatRooms = chatRooms
                            .OrderByDescending(chatRoom =>
                                DateTime.TryParse(chatRoom.SentDateTime, out DateTime sentDateTime) ? sentDateTime : DateTime.MinValue).ToList();

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, orderedChatRooms);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<IBusinessResult> GetStatusInRoomChat(string roomId, long currentUserId)
        {
            try
            {
                DocumentReference roomChatRef = _firestoreDb.Collection("ChatRooms").Document(roomId);

                DocumentSnapshot documentSnapshot = await roomChatRef.GetSnapshotAsync();

                var data = documentSnapshot.ToDictionary();

                if (data.TryGetValue("MemberIds", out var memberIdsObj) &&
                    memberIdsObj is IDictionary<string, object> memberIds && memberIds.ContainsKey(currentUserId.ToString()))
                {
                    var numberOfMems = memberIds.Count;

                    if (numberOfMems == 2)
                    {
                        foreach (var member in memberIds)
                        {
                            if (!member.Key.Equals(currentUserId.ToString()))
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
                                    return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, new { IsOnline = (bool)isOnlineCheck });
                                }
                            }
                        }
                    }
                    else
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "This is a group chat!");
                    }
                }
                else
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not belong to this room chat!");
                }

                return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Something Wrong!");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, "An unexpected error occurred: " + ex.Message);
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

                    if (messageData.TryGetValue("SenderId", out var SenderId) && (long)SenderId != currentUserId)
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

        public async Task<IBusinessResult> ChangeStatus(int userId, bool isOnline)
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

                await onlineMembersRef.Document(userId.ToString()).SetAsync(new Dictionary<string, object>
                {
                    { "IsOnline", isOnline }
                }, SetOptions.MergeAll);

                return new BusinessResult(Const.SUCCESS_UPDATE, $"User status updated to {(isOnline ? "Online" : "Offline")}.");
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

                var memberIds = req.Members.Select(m => m.AccountId).ToList();
                var allPairs = _groupService.GetUniquePairs(memberIds);

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

                foreach (var user in req.Members)
                {
                    var isExisted = await _unitOfWork.AccountRepository.GetByIdAsync(user.AccountId);
                    if (isExisted == null)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                    }

                    var userGroups = _unitOfWork.GroupMemberRepository
                        .GetAll().Where(gm => gm.AccountId == user.AccountId && gm.Status.Equals(SD.GeneralStatus.ACTIVE)).ToList();

                    if (!userGroups.Any())
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, $"User {user.AccountId} is not in any family group!");
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
                            return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Not all users are in the same family group!");
                        }
                    }
                }

                if (string.IsNullOrEmpty(req.GroupId))
                {
                    var urlLink = ("", "");

                    if (req.GroupAvatar != null)
                    {
                        urlLink = await CloudinaryHelper.UploadImageAsync(req.GroupAvatar);
                    }

                    var creator = req.Members.Where(m => m.IsCreator).FirstOrDefault();

                    var creatorCheck = await _unitOfWork.AccountRepository.GetByIdAsync(creator.AccountId);

                    if (creatorCheck.RoleId != 3)
                    {
                        return new BusinessResult(Const.FAIL_CREATE, Const.FAIL_CREATE_MSG, "Creator is not Family Member");
                    }

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
                            { "CreatedAt", DateTime.UtcNow.AddHours(7).ToString("dd-MM-yyyy HH:mm") },
                            { "IsGroupChat", true },
                            { "RoomName", req.GroupName },
                            { "RoomAvatar", req.GroupAvatar == null ? "https://icons.veryicon.com/png/o/miscellaneous/standard/avatar-15.png" : urlLink.Item2},
                            { "SenderId", 0 },
                            { "LastMessage", "" },
                            { "SentDate",  null },
                            { "SentTime",  null },
                            { "SentDateTime",  null },
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

        public async Task<IBusinessResult> UpdateGroupName(string groupId, string newGroupName)
        {
            try
            {
                if (string.IsNullOrEmpty(groupId))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Group ID cannot be empty!");
                }

                if (string.IsNullOrEmpty(newGroupName))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "New group name cannot be empty!");
                }

                var groupRef = _firestoreDb.Collection("ChatRooms").Document(groupId);
                var groupDoc = await groupRef.GetSnapshotAsync();

                if (!groupDoc.Exists)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Group chat does not exist!");
                }

                var updateData = new Dictionary<string, object>
                {
                    { "RoomName", newGroupName }
                };

                await groupRef.UpdateAsync(updateData);

                return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateGroupAvatar(string groupId, IFormFile newGroupAvatar)
        {
            try
            {
                if (string.IsNullOrEmpty(groupId))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Group ID cannot be empty!");
                }

                if (newGroupAvatar == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "New group avatar cannot be empty!");
                }

                var groupRef = _firestoreDb.Collection("ChatRooms").Document(groupId);
                var groupDoc = await groupRef.GetSnapshotAsync();

                if (!groupDoc.Exists)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Group chat does not exist!");
                }

                var urlLink = ("", "");

                urlLink = await CloudinaryHelper.UploadImageAsync(newGroupAvatar);

                var updateData = new Dictionary<string, object>
                {
                    { "RoomAvatar", urlLink.Item2 }
                };

                await groupRef.UpdateAsync(updateData);

                return new BusinessResult(Const.SUCCESS_UPDATE, Const.SUCCESS_UPDATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_UPDATE, ex.Message);
            }
        }

        public async Task<IBusinessResult> RemoveMemberFromGroup(int kickerId, string groupId, int memberId)
        {
            try
            {
                if (string.IsNullOrEmpty(groupId))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Group ID cannot be empty!");
                }

                if (memberId <= 0)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Member ID is invalid!");
                }

                var groupRef = _firestoreDb.Collection("ChatRooms").Document(groupId);
                var groupDoc = await groupRef.GetSnapshotAsync();

                if (!groupDoc.Exists)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Group chat does not exist!");
                }

                var memberIdStr = memberId.ToString();
                var membersRef = groupRef.Collection("Members").Document(memberIdStr);
                DocumentSnapshot membersSnapshot = await membersRef.GetSnapshotAsync();

                if (!membersSnapshot.Exists)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Member is not a member of the group!");
                }

                if (!membersSnapshot.ToDictionary().TryGetValue("IsCreator", out var isCreatorObj1))
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "The 'IsCreator' field is missing!");
                }

                if (!(isCreatorObj1 is bool isCreator1) || isCreator1)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Creator cannot be kicked/out!");
                }

                if (kickerId != memberId)
                {
                    var kickersRef = groupRef.Collection("Members").Document(kickerId.ToString());

                    DocumentSnapshot kickerSnapshot = await kickersRef.GetSnapshotAsync();

                    if (!kickerSnapshot.Exists)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Kicker is not a member of the group!");
                    }

                    if (!kickerSnapshot.ToDictionary().TryGetValue("IsCreator", out var isCreatorObj))
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "The 'IsCreator' field is missing!");
                    }

                    if (!(isCreatorObj is bool isCreator) || !isCreator)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Only the group creator can perform this action!");
                    }
                }

                var currentMembers = groupDoc.GetValue<Dictionary<string, object>>("MemberIds") ?? new Dictionary<string, object>();

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


                await membersRef.DeleteAsync();

                return new BusinessResult(Const.SUCCESS_DELETE, Const.SUCCESS_DELETE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_DELETE, ex.Message);
            }
        }

        public async Task<IBusinessResult> GetRoomChatByRoomChatId(string roomChatId, int userId)
        {
            try
            {
                DocumentReference chatRoomRef = _firestoreDb.Collection("ChatRooms").Document(roomChatId);
                DocumentSnapshot chatRoomSnapshot = await chatRoomRef.GetSnapshotAsync();

                if (!chatRoomSnapshot.Exists)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Chat room does not exist!");
                }

                var data = chatRoomSnapshot.ToDictionary();

                int numberOfMems = 0;
                var roomName = string.Empty;
                var roomAvatar = string.Empty;
                var isOnline = false;
                var listUser = new List<GetUserInRoomChatDetailDTO>();
                bool isGroupChat = data["IsGroupChat"] as bool? ?? false;

                if (data.TryGetValue("MemberIds", out var memberIdsObj) &&
                    memberIdsObj is IDictionary<string, object> memberIds)
                {
                    numberOfMems = memberIds.Count;

                    foreach (var member in memberIds)
                    {
                        var getUser = await _unitOfWork.AccountRepository.GetByIdAsync(int.Parse(member.Key));
                        if (getUser == null)
                        {
                            return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "User does not exist!");
                        }

                        var membersRef = chatRoomRef.Collection("Members").Document(getUser.AccountId.ToString());

                        DocumentSnapshot membersSnapshot = await membersRef.GetSnapshotAsync();

                        var memberData = membersSnapshot.ToDictionary();

                        var isCreator = memberData["IsCreator"] as bool? ?? false;

                        var userMap = _mapper.Map<GetUserInRoomChatDetailDTO>(getUser);

                        userMap.IsCreator = isCreator;

                        listUser.Add(userMap);

                        if (!isGroupChat && numberOfMems == 2)
                        {
                            if (!member.Key.Equals(userId.ToString()))
                            {
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

                var chatRoom = new GetChatRoomByIdDTO
                {
                    RoomId = chatRoomSnapshot.Id,
                    CreatedAt = data["CreatedAt"]?.ToString(),
                    IsOnline = isOnline,
                    IsGroupChat = isGroupChat,
                    RoomName = !isGroupChat && numberOfMems == 2 ? roomName : data["RoomName"].ToString(),
                    RoomAvatar = !isGroupChat && numberOfMems == 2 ? roomAvatar : data["RoomAvatar"].ToString(),
                    NumberOfMems = numberOfMems,
                    Users = listUser
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, chatRoom);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_READ, "An unexpected error occurred: " + ex.Message);
            }
        }
    }
}
