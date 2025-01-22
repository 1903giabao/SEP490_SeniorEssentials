using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO
{
    public class GameModel
    {
        public int Id { get; set; }
        public int ContentProviderId { get; set; }
        public string GameName { get; set; }
        public string GameDescription { get; set; }
        public string Status {  get; set; }
    }
}
