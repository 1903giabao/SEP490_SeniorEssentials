using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class ChatRoomDTO
    {
        public string? RoomId { get; set; }
        public string? RoomName { get; set; }
        public string? RoomAvatar { get; set; }
        public string? CreatedAt { get; set; }
        public bool? IsOnline { get; set; }
        public bool? IsGroupChat { get; set; }
        public bool? IsProfessorChat { get; set; }
        public int? NumberOfMems { get; set; }
        public long? SenderId { get; set; }
        public string? LastMessage { get; set; }
        public string? SentDate { get; set; }
        public string? SentTime { get; set; }
        public string? SentDateTime { get; set; }
        public List<UserInRoomChatDTO> Users { get; set; }
    }


    public class UserInRoomChatDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
