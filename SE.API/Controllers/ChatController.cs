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

namespace SE.API.Controllers
{
    [Route("chat-management")]
    [ApiController]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly FirestoreDb _firestoreDb = FirestoreDb.Create("testproject-bc2e2");
        private readonly string _apiKey = "B33004B21C3D49660617A96B47C788";
        private readonly string _secretKey = "560DBDA1555DDDDAD5F6D340E31A18";
        private readonly string _brandName = "LUXDEN";

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatDTO chatDTO)
        {
            DocumentReference chatRef = _firestoreDb.Collection("Chat").Document(chatDTO.ChatRoomID);
            CollectionReference messagesRef = chatRef.Collection("Message");

            var newMessage = new
            {
                SenderName = chatDTO.SenderName,
                Message = chatDTO.Message,
                SentTime = DateTime.UtcNow,
            };

            await messagesRef.AddAsync(newMessage);

            return Ok();
        }

        [HttpGet("{chatroomId}/messages")]
        public async Task<IActionResult> GetAllMessages(string chatroomId)
        {
            CollectionReference messagesRef = _firestoreDb.Collection("Chat").Document(chatroomId).Collection("Message");

            QuerySnapshot snapshot = await messagesRef.OrderBy("SentTime").GetSnapshotAsync();

            List<Message> messages = new List<Message>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                var messageData = document.ToDictionary();
                var message = new Message
                {
                    SenderName = messageData["SenderName"].ToString(),
                    MessageText = messageData["Message"].ToString(),
                    SentTime = messageData["SentTime"].ToString() 
                };
                messages.Add(message);
            }

            return Ok(messages);
        }

        public class Message
        {
            public string SenderName { get; set; }
            public string MessageText { get; set; }
            public string SentTime { get; set; }
        }

        [HttpGet("{phoneNumber}")]
        public async Task<IActionResult> SendSms(string phoneNumber)
        {
            var otp = new Random().Next(100000, 999999);
            string content = $"Mã OTP của bạn là : {otp}";
            content = HttpUtility.UrlEncode(content);

            var url = $"http://api.tinnhanthuonghieu.com/MainService.svc/json/SendMultipleMessage_V4_get?SmsType=2&ApiKey={_apiKey}&SecretKey={_secretKey}&Brandname={_brandName}&Content={content}&Phone={0912393903}";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Failed to send SMS.");
                }
            }
        }
    }
}
