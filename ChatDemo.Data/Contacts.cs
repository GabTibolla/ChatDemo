using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatDemo.Data
{
    public class Contacts
    {
        public int? Id;
        public string? Name { get; set; }
        public string? NumberId { get; set; }
        public string? MyNumberId { get; set; }
        public int? LastMessageId { get; set; }
        public DateTime? LastMessageDate { get; set; }
    }
}
