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
        public async Task<IActionResult> CreateGroupChat([FromBody] CreateGroupChatRequest req)
        {
            var result = await _chatService.CreateGroupChat(req);
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
        public async Task<IActionResult> ChangeStatus([FromQuery] int userId)
        {
            var result = await _chatService.ChangeStatus(userId);
            return Ok(result);
        }
    }
}
