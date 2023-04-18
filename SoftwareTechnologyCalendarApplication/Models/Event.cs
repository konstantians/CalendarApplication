using System;
using System.Collections.Generic;
using System.Text;

namespace SoftwareTechnologyCalendarApplication.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartingTime { get; set; }
        public DateTime EndingTime { get; set; }
        public bool AlertStatus { get; set; }
        public List<User> UsersThatParticipateInTheEvent { get; set; } = new List<User>();
    }
}
