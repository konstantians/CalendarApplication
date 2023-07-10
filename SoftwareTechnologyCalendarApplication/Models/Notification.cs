using System;
using DataAccess.Models;

namespace SoftwareTechnologyCalendarApplication.Models
{
    public class Notification
    {
        public Event EventOfNotification { get; set; }
        public DateTime NotificationTime { get; set; }
        public bool InvitationPending { get; set; }
        public bool EventAccepted { get; set; }
        public bool EventRejected { get; set; }
        public bool EventChanged { get; set; }
        public bool CommentAdded { get; set; }
        public bool CommentDeleted { get; set; }
        public bool EventDeleted { get; set; }
        

        public Notification(){}

        public Notification(NotificationDataModel notificationDataModel)
        {
            EventOfNotification = new Event(notificationDataModel.EventOfNotification);
            NotificationTime = notificationDataModel.NotificationTime;
            InvitationPending = notificationDataModel.InvitationPending;
            EventAccepted = notificationDataModel.EventAccepted;
            EventRejected = notificationDataModel.EventRejected;
            EventChanged = notificationDataModel.EventChanged;
            CommentAdded = notificationDataModel.CommentAdded;
            CommentDeleted = notificationDataModel.CommentDeleted;
            EventDeleted = notificationDataModel.EventDeleted;
        }
    }
}
