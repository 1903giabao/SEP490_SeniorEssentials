using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO.Emergency
{
    public class GetListEmergencyConfirmationByFamilyMemberDTO
    {
        public int ElderlyId { get; set; }
        public List<GetEmergencyConfirmationDTO> GetEmergencyConfirmationDTOs { get; set; } = new List<GetEmergencyConfirmationDTO>();
    }
}
