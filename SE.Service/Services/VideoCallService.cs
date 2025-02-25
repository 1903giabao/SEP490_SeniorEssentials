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
        Task<IBusinessResult> GetAllVideoCallHistory();
        Task<IBusinessResult> GetVideoCallHistoryById(int vidCallId);
        Task<IBusinessResult> VideoCall(VideoCallRequest req);
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

        public async Task<IBusinessResult> GetAllVideoCallHistory()
        {
            try
            {
                var vidCallHistory = await _unitOfWork.VideoCallRepository.GetAllIncluding();

                var vidCallModel = _mapper.Map<List<VideoCallModel>>(vidCallHistory);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, vidCallModel);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IBusinessResult> GetVideoCallHistoryById(int vidCallId)
        {
            try
            {
                var vidCallHistory = await _unitOfWork.VideoCallRepository.GetByIdIncluding(vidCallId);

                var vidCallModel = _mapper.Map<VideoCallModel>(vidCallHistory);

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, vidCallModel);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
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

                var newMessage = new
                {
                    SenderId = req.CallerId,
                    SenderName = caller.FullName,
                    SenderAvatar = caller.Avatar,
                    Message = req.Duration,
                    MessageType = req.Status.ToString(),
                    SentDate = sentTime.ToString("dd-MM-yyyy"),
                    SentTime = string.Format("{0:D2}:{1:D2}", (int)sentTime.TimeOfDay.TotalHours, sentTime.TimeOfDay.Minutes),
                    SentDateTime = sentTime.ToString(),
                    IsSeen = false,
                };

                await messagesRef.AddAsync(newMessage);

                await chatRef.UpdateAsync(new Dictionary<string, object>
                    {
                        { "LastMessage", "Cuộc gọi" },
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
                // Convert the list of user IDs to a HashSet for easier comparison
                var userSet = new HashSet<string>(listUserInRoomChat.Select(id => id.ToString()));

                // Query the ChatRooms collection
                var chatRoomsRef = _firestoreDb.Collection("ChatRooms");
                var chatRoomsSnapshot = await chatRoomsRef.GetSnapshotAsync();

                foreach (var chatRoomDoc in chatRoomsSnapshot.Documents)
                {
                    // Get the MemberIds dictionary from the chat room document
                    var memberIds = chatRoomDoc.GetValue<Dictionary<string, object>>("MemberIds");

                    if (memberIds != null)
                    {
                        // Convert the keys of the MemberIds dictionary to a HashSet
                        var roomUserSet = new HashSet<string>(memberIds.Keys);

                        // Check if the roomUserSet contains all users in the userSet
                        if (roomUserSet.IsSupersetOf(userSet))
                        {
                            // Return the chat room ID if it contains all users
                            return chatRoomDoc.Id;
                        }
                    }
                }

                // If no chat room contains all users, return null or an empty string
                return null;
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                Console.WriteLine($"Error finding chat room: {ex.Message}");
                return null;
            }
        }
    }
}
