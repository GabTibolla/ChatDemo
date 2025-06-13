using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatDemo.Data
{
    public class Message
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public DateTime Datetime { get; set; }
        public string FromNumberId { get; set; }
        public string ToNumberId { get; set; }
        public bool Sent { get; set; }
    }
}
