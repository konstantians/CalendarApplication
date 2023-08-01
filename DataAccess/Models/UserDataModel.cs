using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    /// <summary>
    /// The User model that is used by the dataAccess.
    /// </summary>
    public class UserDataModel
    {
        /// <summary>
        /// The username of the user.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// The password of the user.
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// The fullname of the user(firstname and lastname).
        /// </summary>
        public string Fullname { get; set; }
        /// <summary>
        /// The date of birth of the user.
        /// </summary>
        public DateTime DateOfBirth { get; set; }
        /// <summary>
        /// The email of the user.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// The phone number of the user(without country code).
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// The list that contains the calendars that the user has. 
        /// </summary>
        public List<CalendarDataModel> Calendars { get; set; } = new List<CalendarDataModel>();
        /// <summary>
        /// The list that contains the events that the user participates in(native and foreign events). 
        /// </summary>
        public List<EventDataModel> EventsThatTheUserParticipates { get; set; } = new List<EventDataModel>();
        /// <summary>
        /// The list that contains the notifications that the user has(created from the events that the user participates in).
        /// </summary>
        public List<NotificationDataModel> Notifications { get; set; } = new List<NotificationDataModel>();
        /// <summary>
        /// The list that contains all the comments of the user.
        /// </summary>
        public List<CommentDataModel> Comments { get; set; } = new List<CommentDataModel>();
    }
}
