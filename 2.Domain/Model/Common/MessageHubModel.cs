using System;
using System.Collections.Generic;
using System.Text;

namespace PT.Domain.Model
{
    public class MessageHub
    {
        public enum MessageHubType
        {
            Contact = 0,
            BookAnAppointment = 1,
        }
        public string Content { get; set; }
        public MessageHubType Type { get; set; }
        public DateTime Date { get; set; }
    }
}
