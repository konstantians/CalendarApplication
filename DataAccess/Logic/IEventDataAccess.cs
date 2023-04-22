using DataAccess.Models;
using System.Collections.Generic;

namespace DataAccess.Logic
{
    public interface IEventDataAccess
    {
        List<EventDataModel> GetEvents(int calendarId);
        EventDataModel GetEvent(int id);
        int CreateEvent(EventDataModel calendarEvent, string username, int calendarId);
        int CreateEvent(EventDataModel calendarEvent, List<string> usernamesOfUsers, int calendarId);
        void AddUserToEvent(int eventId, string username, int calendarId);
        void AddUsersToEvent(int eventId, List<string> usernamesOfUsers, int calendarId);
        void UpdateEvent(EventDataModel calendarEvent);
        void DeleteEvent(int id);
    }
}