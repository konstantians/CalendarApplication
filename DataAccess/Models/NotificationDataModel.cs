using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    /// <summary>
    /// The Notification model that is used by the dataAccess.
    /// </summary>
    public class NotificationDataModel
    {
        /// <summary>
        /// The event that this notification referes to.
        /// </summary>
        public EventDataModel EventOfNotification { get; set; } = new EventDataModel();
        /// <summary>
        /// The date and time at which this notification was created.
        /// </summary>
        public DateTime NotificationTime { get; set; }
        /// <summary>
        /// The boolean that marks if the notification was an invitation. Only one of the booleans can be true
        /// at the same time for each notification.
        /// </summary>
        public bool InvitationPending { get; set; }
        /// <summary>
        /// The boolean that marks that the invitation was accepted. Only one of the booleans can be true
        /// at the same time for each notification.
        /// </summary>
        public bool EventAccepted { get; set; }
        /// <summary>
        /// The boolean that marks that the invitation was rejected. Only one of the booleans can be true
        /// at the same time for each notification.
        /// </summary>
        public bool EventRejected { get; set; }
        /// <summary>
        /// The boolean that marks that the event was updated. Only one of the booleans can be true
        /// at the same time for each notification.
        /// </summary>
        public bool EventChanged { get; set; }
        /// <summary>
        /// The boolean that marks that a comment was added to the event. Only one of the booleans can be true
        /// at the same time for each notification.
        /// </summary>
        public bool CommentAdded { get; set; }
        /// <summary>
        /// The boolean that marks that a comment was deleted from the event. Only one of the booleans can be true
        /// at the same time for each notification.
        /// </summary>
        public bool CommentDeleted { get; set; }
        /// <summary>
        /// The boolean that marks that an was deleted. Only one of the booleans can be true
        /// at the same time for each notification.
        /// </summary>
        public bool EventDeleted { get; set; }
        /// <summary>
        /// The boolean that is used to showcase that this is a notification that was created from the alert status(the user 
        /// chose for it to be open) and it signifies that the event is about to begin. Only one of the booleans can be true
        /// at the same time for each notification.
        /// </summary>
        public bool AlertNotification { get; set; }
        /// <summary>
        /// The boolean that is used to showcase that the notification has been seen by the user
        /// </summary>
        public bool HasBeenSeen { get; set; }

        /// <summary>
        /// default constructor
        /// </summary>
        public NotificationDataModel(){}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="notificationTime"></param>
        /// <param name="invitationPending"></param>
        /// <param name="eventAccepted"></param>
        /// <param name="eventRejected"></param>
        /// <param name="eventChanged"></param>
        /// <param name="commentAdded"></param>
        /// <param name="commentDeleted"></param>
        /// <param name="eventDeleted"></param>
        /// <param name="alertNotification"></param>
        public NotificationDataModel(int id, DateTime notificationTime, bool invitationPending,
            bool eventAccepted, bool eventRejected, bool eventChanged, bool commentAdded,
            bool commentDeleted, bool eventDeleted, bool alertNotification)
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
            AlertNotification = alertNotification;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="notificationTime"></param>
        /// <param name="invitationPending"></param>
        /// <param name="eventAccepted"></param>
        /// <param name="eventRejected"></param>
        /// <param name="eventChanged"></param>
        /// <param name="commentAdded"></param>
        /// <param name="commentDeleted"></param>
        /// <param name="eventDeleted"></param>
        /// <param name="alertNotification"></param>
        /// <param name="hasBeenSeen"></param>
        public NotificationDataModel(int id, DateTime notificationTime, bool invitationPending,
            bool eventAccepted, bool eventRejected, bool eventChanged, bool commentAdded, 
            bool commentDeleted, bool eventDeleted, bool alertNotification, bool hasBeenSeen)
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
            AlertNotification = alertNotification;
            HasBeenSeen = hasBeenSeen;
        }
    }
}
