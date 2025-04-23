using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class GetChatRoomByIdDTO
    {
        public string? RoomId { get; set; }
        public string? RoomName { get; set; }
        public string? RoomAvatar { get; set; }
        public string? CreatedAt { get; set; }
        public bool? IsOnline { get; set; }
        public bool? IsGroupChat { get; set; }
        public bool? IsFriend { get; set; }
        public int? NumberOfMems { get; set; }
        public List<GetUserInRoomChatDetailDTO> Users { get; set; }
    }
}
