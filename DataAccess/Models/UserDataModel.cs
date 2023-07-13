using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public class UserDataModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<CalendarDataModel> Calendars { get; set; } = new List<CalendarDataModel>();
        public List<EventDataModel> EventsThatTheUserParticipates { get; set; } = new List<EventDataModel>();
        public List<NotificationDataModel> Notifications { get; set; } = new List<NotificationDataModel>();
        public List<CommentDataModel> Comments { get; set; } = new List<CommentDataModel>();
    }
}
