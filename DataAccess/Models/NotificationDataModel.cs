using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public class NotificationDataModel
    {
        public EventDataModel EventOfNotification { get; set; } = new EventDataModel();
        public DateTime NotificationTime { get; set; }
        public bool InvitationPending { get; set; }
        public bool EventAccepted { get; set; }
        public bool EventRejected { get; set; }
        public bool EventChanged { get; set; }
        public bool CommentAdded { get; set; }
        public bool CommentDeleted { get; set; }
        public bool EventDeleted { get; set; }

        //default constructor
        public NotificationDataModel(){}

        public NotificationDataModel(int id, DateTime notificationTime, bool invitationPending,
            bool eventAccepted, bool eventRejected, bool eventChanged, bool commentAdded, bool commentDeleted, bool eventDeleted)
        {
            EventOfNotification.Id = id;
            NotificationTime = notificationTime;
            InvitationPending = invitationPending;
            EventAccepted = eventAccepted;
            EventRejected = eventRejected;
            EventChanged = eventChanged;
            CommentAdded = commentAdded;
            CommentDeleted = commentDeleted;
            EventDeleted = eventDeleted;
        }
    }
}
