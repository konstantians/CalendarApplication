using DataAccess.Models;
using System;
using System.Collections.Generic;

namespace DataAccess.Logic
{
    public interface IEventDataAccess
    {
        List<EventDataModel> GetEvents(int calendarId);
        EventDataModel GetEvent(int id);
        EventDataModel GetEventForNotifications(int id);
        int CreateEvent(EventDataModel calendarEvent, string username, int calendarId);
        int CreateEvent(EventDataModel calendarEvent, string usernameOfCreator, List<string> usernamesOfUsers, int calendarId);
        void CreateComments(List<string> comments, int eventId, string creatorOfComment);
        void DeleteComment(CommentDataModel commentDataModel);
        void InviteUserToEvent(int eventId, string usernameOfCreator, string foreignUsername);
        void InviteUsersToEvent(int eventId, string usernameOfCreator, List<string> usernamesOfUsers);
        void AcceptInvitation(int eventId, string usernameOfInvitedUser, DateTime notificationTime, int calendarId, bool alertStatus);
        void RejectInvitation(int eventId, string usernameOfInvitedUser, DateTime notificationTime);
        void UpdateEvent(EventDataModel calendarEvent, string username);
        void DeleteEvent(int id, string userWhoDeletedTheEvent);
        void DeleteNotification(int eventId, string userUsername, DateTime notificationTime);
    }
}