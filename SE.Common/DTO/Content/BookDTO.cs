using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE.Common.DTO.Content
{
    public class BookDTO
    {
        public int BookId { get; set; }

        public int? AccountId { get; set; }

        public string BookName { get; set; }

        public string BookUrl { get; set; }

        public string BookType { get; set; }

        public string Author { get; set; }

        public DateTime? PublishDate { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
