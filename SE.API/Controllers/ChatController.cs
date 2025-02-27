using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Mvc;
using PusherServer;
using SE.Common.DTO;
using SE.Service.Services;
using System.Net;
using System;
using System.Threading.Tasks;
using System.Web;
using SE.Common.Request;
using SE.Service.Base;
using SE.Service.Helper;
using Firebase.Auth;

namespace SE.API.Controllers
{
    [Route("chat-management")]
    [ApiController]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("{userId}/room-chat")]
        public async Task<IActionResult> GetAllRoomChat([FromRoute] int userId)
        {
            var result = await _chatService.GetAllRoomChat(userId);
            return Ok(result);
        }        
        
        [HttpGet("status-in-room-chat")]
        public async Task<IActionResult> GetStatusInRoomChat([FromQuery] string roomId, [FromQuery] long currentUserId)
        {
            var result = await _chatService.GetStatusInRoomChat(roomId, currentUserId);
            return Ok(result);
        }


        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromForm] SendMessageRequest req)
        {
            var result = await _chatService.SendMessage(req);
            return Ok(result);
        }

        [HttpPost("reply-message")]
        public async Task<IActionResult> ReplyMessage([FromForm] ReplyMessageRequest req)
        {
            var result = await _chatService.ReplyMessage(req);
            return Ok(result);
        }

        [HttpPost("group-chat")]
        public async Task<IActionResult> CreateGroupChat([FromForm] CreateGroupChatRequest req)
        {
            var result = await _chatService.CreateGroupChat(req);
            return Ok(result);
        }

        [HttpPut("group-chat/group-name")]
        public async Task<IActionResult> UpdateGroupName([FromQuery] string groupId, [FromQuery] string groupName)
        {
            var result = await _chatService.UpdateGroupName(groupId, groupName);
            return Ok(result);
        }        
        
        [HttpPut("group-chat/group-avatar")]
        public async Task<IActionResult> UpdateGroupAvatar([FromQuery] string groupId, IFormFile groupAvatar)
        {
            var result = await _chatService.UpdateGroupAvatar(groupId, groupAvatar);
            return Ok(result);
        }

        [HttpGet("{roomId}/messages")]
        public async Task<IActionResult> GetAllMessages(string roomId)
        {
            var result = await _chatService.GetAllMessages(roomId);
            return Ok(result);
        }

        [HttpPut("seen")]
        public async Task<IActionResult> MarkMessagesAsSeen([FromQuery] string roomId, [FromQuery] long currentUserId)
        {
            var result = await _chatService.MarkMessagesAsSeen(roomId, currentUserId);
            return Ok(result);
        }

        [HttpPut("change-status")]
        public async Task<IActionResult> ChangeStatus([FromQuery] int userId, [FromQuery] bool isOnline)
        {
            var result = await _chatService.ChangeStatus(userId, isOnline);
            return Ok(result);
        }        
        
        [HttpPut("kick-member")]
        public async Task<IActionResult> RemoveMemberFromGroup([FromQuery] int kickerId, [FromQuery] string groupId, [FromQuery] int userId)
        {
            var result = await _chatService.RemoveMemberFromGroup(kickerId, groupId, userId);
            return Ok(result);
        }        
        
        [HttpGet("room-chat/{roomChatId}/{userId}")]
        public async Task<IActionResult> GetRoomChatByRoomChatId(string roomChatId, int userId)
        {
            var result = await _chatService.GetRoomChatByRoomChatId(roomChatId, userId);
            return Ok(result);
        }
    }
}
