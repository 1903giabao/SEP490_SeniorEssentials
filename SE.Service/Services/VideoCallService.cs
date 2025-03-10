using AutoMapper;
using SE.Common.DTO;
using SE.Common;
using SE.Data.UnitOfWork;
using SE.Service.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SE.Common.Request;
using SE.Data.Models;
using SE.Common.Enums;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Firebase.Auth;

namespace SE.Service.Services
{
    public interface IVideoCallService
    {
        Task<IBusinessResult> VideoCall(VideoCallRequest req);
        Task<string> FindChatRoomContainingAllUsers(List<int> listUserInRoomChat);
    }

    public class VideoCallService : IVideoCallService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly FirestoreDb _firestoreDb;

        public VideoCallService(UnitOfWork unitOfWork, IMapper mapper, FirestoreDb firestoreDb)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firestoreDb = firestoreDb;
            _firestoreDb = firestoreDb;
        }

        public async Task<IBusinessResult> VideoCall(VideoCallRequest req)
        {
            try
            {
                var caller = await _unitOfWork.AccountRepository.GetByIdAsync(req.CallerId);
                if (caller == null)
                {
                    return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, "Caller does not exist!");
                }

                foreach (var receiverId in req.ListReceiverId)
                {
                    var receiver = await _unitOfWork.AccountRepository.GetByIdAsync(receiverId);
                    if (receiver == null)
                    {
                        return new BusinessResult(Const.FAIL_READ, Const.FAIL_READ_MSG, $"Receiver with ID {receiverId} does not exist!");
                    }
                }

                var listUserInRoomChat = new List<int>
                {
                    caller.AccountId,
                };

                listUserInRoomChat.AddRange(req.ListReceiverId);

                var roomChatId = await FindChatRoomContainingAllUsers(listUserInRoomChat);

                var sentTime = DateTime.UtcNow.AddHours(7);

                DocumentReference chatRef = _firestoreDb.Collection("ChatRooms").Document(roomChatId);

                var messagesRef = chatRef.Collection("Messages");

                var message = req.IsVideo ? $"Cuộc gọi Video - {req.Duration}" : $"Cuộc gọi thoại - {req.Duration}";

                var newMessage = new
                {
                    SenderId = req.CallerId,
                    SenderName = caller.FullName,
                    SenderAvatar = caller.Avatar,
                    Message = message,
                    MessageType = req.Status.ToString(),
                    SentDate = sentTime.ToString("dd-MM-yyyy"),
                    SentTime = string.Format("{0:D2}:{1:D2}", (int)sentTime.TimeOfDay.TotalHours, sentTime.TimeOfDay.Minutes),
                    SentDateTime = sentTime.ToString(),
                    IsSeen = false,
                };

                await messagesRef.AddAsync(newMessage);

                await chatRef.UpdateAsync(new Dictionary<string, object>
                    {
                        { "LastMessage", message },
                        { "SentDate", sentTime.ToString("dd-MM-yyyy") },
                        { "SentTime", string.Format("{0:D2}:{1:D2}", (int)sentTime.TimeOfDay.TotalHours, sentTime.TimeOfDay.Minutes) },
                        { "SentDateTime", sentTime.ToString() },
                        { "SenderId", req.CallerId },
                    });

                return new BusinessResult(Const.SUCCESS_CREATE, Const.SUCCESS_CREATE_MSG, newMessage);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.FAIL_CREATE, "An unexpected error occurred: " + ex.Message);
            }
        }

        public async Task<string> FindChatRoomContainingAllUsers(List<int> listUserInRoomChat)
        {
            try
            {
                var userSet = new HashSet<string>(listUserInRoomChat.Select(id => id.ToString()));

                var chatRoomsRef = _firestoreDb.Collection("ChatRooms");
                var chatRoomsSnapshot = await chatRoomsRef.GetSnapshotAsync();

                foreach (var chatRoomDoc in chatRoomsSnapshot.Documents)
                {
                    var memberIds = chatRoomDoc.GetValue<Dictionary<string, object>>("MemberIds");

                    if (memberIds != null)
                    {
                        var roomUserSet = new HashSet<string>(memberIds.Keys);

                        if (roomUserSet.SetEquals(userSet))
                        {
                            return chatRoomDoc.Id;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding chat room: {ex.Message}");
                return null;
            }
        }
    }
}
