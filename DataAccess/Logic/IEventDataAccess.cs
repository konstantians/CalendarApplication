using DataAccess.Models;
using System;
using System.Collections.Generic;

namespace DataAccess.Logic
{
    /// <summary>
    /// interface thas handles the event, comment and notification dataAccess and is used for dependency injection.
    /// </summary>
    public interface IEventDataAccess
    {
        /// <summary>
        /// This method returns all the static information of the events of a specific calendar from the database. It also fills the 
        /// details the information of these events. It must be mentioned that the indirect connection of the users
        /// that the method returns do not have their calendars, events, categories(the complicated information) filled.
        /// In case you want to fill these information you can use the GetUser method in the UserDataAccess class using 
        /// the name of the user you want. Also the dynamic information(information based on the preferences of each user, such as alert status) 
        /// of the events is not returned, so if you want that then you will have to use the GetEvent method, which also takes
        /// as an input the username of the user you want the dynamic information of.
        /// </summary>
        /// <param name="calendarId">The Id of the calendar</param>
        /// <returns>All the static information of the events of a calendar</returns>
        List<EventDataModel> GetEvents(int calendarId);
        /// <summary>
        /// This method returns using the id parameter all the static information for the given event. Static information is
        /// all the information that is shared by everyone who participates in the event, such as username, but not information 
        /// such as the alertStatus, which might different from user to user who participate in the event. So if you also need this
        /// information(dynamic information of the event) you should use the overloaded version, which also takes as parameter a
        /// string username. The reason why this version exists is, because it is faster, so if you do not need that information
        /// you could use this one.
        /// </summary>
        /// <param name="id">The id of the event</param>
        /// <returns>The static/public information of an event</returns>
        EventDataModel GetEvent(int id);
        /// <summary>
        /// This method returns using the id parameter all the static and dynamic information for the given event. Static information is
        /// all the information that is shared by everyone who participates in the event, such as username, but not information 
        /// such as the alertStatus, which might different from user to user who participate in the event. This specific overload of the 
        /// method handles the dynamic information, so if you need this information you should use this one. That being said this version is slower
        /// than the GetEvent which takes as a parameter only the id of the event, so if you do not need the dynamic information of the event, you
        /// could use that.
        /// </summary>
        /// <param name="id">The id of the event</param>
        /// <param name="username">The name of the user we want the non static information for(for example alert status)</param>
        /// <returns>The full(static + dynamic) information of an event for a given user</returns>
        EventDataModel GetEvent(int id, string username);
        /// <summary>
        /// Do not use this method it used only for dataAccess internal reasons and I could not 
        /// make it private/internal :( .
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        EventDataModel GetEventForNotifications(int id);
        /// <summary>
        /// This method creates an event using the given model and connects it to the chosen calendar using the calendarId parameter
        /// and also creates the direct connection with the user that created it. If the EventComments property is
        /// not empty it also creates comments using the property by calling the method CreateComments.
        /// Finally it returns the id of the newly created event.  
        /// </summary>
        /// <param name="calendarEvent">The event's model</param>
        /// <param name="username">The user's username who has created the calendar that contains the event</param>
        /// <param name="calendarId">The calendar's id that contains the event</param>
        /// <returns>The id of the newly created event</returns>
        int CreateEvent(EventDataModel calendarEvent, string username, int calendarId);
        /// <summary>
        /// This method creates an event using the given model and connects it to the chosen calendar using the calendarId parameter
        /// and also creates the direct connection with the user that created it. If the EventComments property is
        /// not empty it also creates comments using the property by calling the method CreateComments. This specific 
        /// overloaded version of the CreateEvent method also sends invitations to the specified users
        /// using the usernamesOfUsers list. Finally it returns the id of the newly created event.  
        /// </summary>
        /// <param name="calendarEvent">The event's information</param>
        /// <param name="usernameOfCreator">the event's creator name</param>
        /// <param name="usernamesOfUsers">The users' usernames who are associated(are invited) with the event</param>
        /// <param name="calendarId">The calendar's id that contains the event</param>
        /// <returns>The id of the newly created event</returns>
        int CreateEvent(EventDataModel calendarEvent, string usernameOfCreator, List<string> usernamesOfUsers, int calendarId);
        /// <summary>
        /// This method creates comments for an event using the comments list parameter. It also sends
        /// notifications to all the other users who participate for the given event that a comment has been
        /// created and the relavent information of that comment.
        /// </summary>
        /// <param name="comments">The texts of the created comments</param>
        /// <param name="eventId">The id of the event for which the comment was created</param>
        /// <param name="creatorOfComment">The name of the creator of the comment</param>
        void CreateComments(List<string> comments, int eventId, string creatorOfComment);
        /// <summary>
        /// This method updates an event using the information that exists on the updated information that exists 
        /// in commentDatamodel parameter. The oldCommentText parameter is necessary for finding out which comment 
        /// was changed.
        /// </summary>
        /// <param name="commentDataModel">The commentDataModel model that contains the updated information of the comment</param>
        /// <param name="oldCommentText">the old text of the comment</param>
        void UpdateComment(CommentDataModel commentDataModel, string oldCommentText);
        /// <summary>
        /// This method deletes a comment using the comments list parameter. It also sends
        /// notifications to all the other users who participate in that event, that the comment has been deleted 
        /// and the relavent information of that action(who deleted, when etc).
        /// </summary>
        /// <param name="commentDataModel">The information of the comment that is needed for locating the comment</param>
        void DeleteComment(CommentDataModel commentDataModel);
        /// <summary>
        /// This method is used to send invitations for the specified event to the specified users.
        /// </summary>
        /// <param name="eventId">The event's id</param>
        /// <param name="usernameOfCreator">The name of the person who created the event</param>
        /// <param name="usernamesOfInvitedUsers">The names of the people who are invited</param>
        void InviteUsersToEvent(int eventId, string usernameOfCreator, List<string> usernamesOfInvitedUsers);
        /// <summary>
        /// This method is used to send an invitation for the specified event to the specified user.
        /// If you need to send multiple invations use the InviteUsersToEvent method instead.
        /// </summary>
        /// <param name="eventId">The event's id</param>
        /// <param name="usernameOfCreator">The name of the person who created the event</param>
        /// <param name="usernameOfInvitedUser">The name of the person who is invited</param>
        void InviteUserToEvent(int eventId, string usernameOfCreator, string usernameOfInvitedUser);
        /// <summary>
        /// This method is used to accept an invitation that came from a different user for a specific event, 
        /// which is identified by the eventId parameter, and to make the indirect connection to the event for the invited user. 
        /// This method also deletes that invitation, so no further action is required. Finally it sends a notification to the 
        /// user who sent the invitation(creator of the event) that the invited user accepted their invitation.
        /// </summary>
        /// <param name="eventId">The id of the event</param>
        /// <param name="usernameOfInvitedUser">The username of the invited user who accepted the invitation</param>
        /// <param name="usernameOfSenderUser">The username of the sender user(the user who sent the notification)</param>
        /// <param name="notificationTime">The date and time of the original invitation, which is used to delete the invitation</param>
        /// <param name="calendarId">The id of the calendar that contains the event</param>
        /// <param name="alertStatus">The alert status that the invited user chose(on or off)</param>
        void AcceptInvitation(int eventId, string usernameOfInvitedUser, string usernameOfSenderUser, DateTime notificationTime, int calendarId, bool alertStatus);
        /// <summary>
        /// This method is used to reject an invitation that came from a different user for a specific event, 
        /// which is identified by the eventId parameter. This method also deletes that invitation, so no further
        /// action is required. Finally it sends a notification to the user who sent the invitation(creator of the event)
        /// that the invited user rejected their invitation.
        /// </summary>
        /// <param name="eventId">The id of the event</param>
        /// <param name="usernameOfInvitedUser">The username of the invited user who rejected the invitation</param>
        /// <param name="usernameOfSenderUser">The username of the sender user(the user who sent the notification)</param>
        /// <param name="notificationTime">The date and time of the original invitation, which is used to delete the invitation</param>
        void RejectInvitation(int eventId, string usernameOfInvitedUser, string usernameOfSenderUser, DateTime notificationTime);
        /// <summary>
        /// This method using the userWhoUpdatedTheEvent parameter figures out on whether the event is native or foreign
        /// If the method finds out that the event was native then it updates all the fields using the information that 
        /// exists in eventDataModel parameter and it also sends notifications to every user that participates in the event
        /// that the event was updated. If the method finds out that the event was foreign it only updates the personal information
        /// of the user such as alertStatus and in this case it does not send any notifications. It must be noted that the method
        /// does not update the comments of the user for the given event, so if you want to update the comments you should use
        /// the updateComment method.
        /// </summary>
        /// <param name="eventDataModel">The event model</param>
        /// <param name="userWhoUpdatedTheEvent">The name of the user who updated the event</param>
        void UpdateEvent(EventDataModel eventDataModel, string userWhoUpdatedTheEvent);
        /// <summary>
        /// This method using the userWhoDeletedTheEvent parameter figures out on whether the event is native or foreign.
        /// If the method finds out that the event was native then it soft deletes the event, which means that it removes
        /// all its connections it has on the database and only keeps the event in a soft deleted state.  It also sends 
        /// a notification to every user who participate in the event that the event is now deleted. Once every notification has been
        /// acknowledged by every user the event becomes hard deleted, which means that is fully removed from the database.
        /// The hard deleted operation is handled, by the DeleteNotification method automatically, so you do not need to worry about that.
        /// If the method finds out that the event was foreign then it just removes the user participation(indirect connection) from that event
        /// and sends to every other user who participates in the event that the user left the event.
        /// </summary>
        /// <param name="eventId">The id of the event</param>
        /// <param name="userWhoDeletedTheEvent">The user who deleted the event</param>
        void DeleteEvent(int eventId, string userWhoDeletedTheEvent);
        /// <summary>
        /// This method is used to send an alert notification status for an event, which is identified by the eventId parameter.
        /// It should be used when an event is about to start and the user has the alert status for that event activated, but this
        /// method does not check for that, so make sure to check on the backend/frontend.
        /// </summary>
        /// <param name="eventId">The id of the event</param>
        /// <param name="username">The username of the user who will receive the alert notification status</param>
        void SendAlertNotification(int eventId, string username);
        /// <summary>
        /// This method is used to delete notifications and it also checks if the event that this notification was for 
        /// is soft deleted. If the event is soft deleted then it will check to see if there are other notifications for 
        /// that event and if no then it will hard delete the event. In any other case it will just stop.
        /// </summary>
        /// <param name="eventId">The id of the event</param>
        /// <param name="userReceiver">The username of the receiver user(the user who received the notification)</param>
        /// <param name="userSender">The username of the sender user(the user who sent the notification)</param>
        /// <param name="notificationTime">The creation date and time of the about to be deleted notification</param>
        void DeleteNotification(int eventId, string userReceiver, string userSender, DateTime notificationTime);
        /// <summary>
        /// This method should only be used when the system needs to understand that the
        /// user has aknowledged the notifications that have been sent to them.
        /// </summary>
        /// <param name="username">The username of the user who saw the notifications</param>
        void UpdateSeenStatusOfNotifications(string username);
    }
}