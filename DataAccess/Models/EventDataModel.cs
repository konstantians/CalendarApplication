using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public class EventDataModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartingTime { get; set; }
        public DateTime EndingTime { get; set; }
        public bool AlertStatus { get; set; }
        public List<UserDataModel> UsersThatParticipateInTheEvent { get; set; } = new List<UserDataModel>();
    }
}
