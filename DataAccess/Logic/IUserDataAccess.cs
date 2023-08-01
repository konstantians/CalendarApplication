using DataAccess.Models;
using System.Collections.Generic;

namespace DataAccess.Logic
{
    /// <summary>
    /// interface thas handles the user dataAccess and is used for dependency injection.
    /// </summary>
    public interface IUserDataAccess
    {
        /// <summary>
        /// This method returns all the users from the database and also all their calendars populated(with their events filled)
        /// and their events populated. Additionally it returns the comments of the users and it also returns all the events they either have created or participate in.
        /// </summary>
        /// <returns>All the users</returns>
        List<UserDataModel> GetUsers();
        /// <summary>
        /// This method returns one user using the given username from the database and also all their calendars populated(with their events filled)
        /// and their events populated. Additionally it returns the comments of the user and it also returns all the events they either have created or participate in.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <returns>The specified user</returns>
        UserDataModel GetUser(string username);
        /// <summary>
        /// This method creates a user. It must be mentioned that this method does not create calendars or events.
        /// In case you want to create those you can by using the createCalendar in the calendarDataAccess class
        /// or the createEvent in the eventDataAccess. The connection with the user there is automatic by providing
        /// the user's username.
        /// </summary>
        /// <param name="user">The user model</param>
        void CreateUser(UserDataModel user);
        /// <summary>
        /// This method updates the user. It must be mentioned that it does not update the calendar or the events
        /// of the user aand it only updates the static information of the user. 
        /// If you need to update those you will have to call the UpdateCalendar method 
        /// from the CalendarDataAccess class and the UpdateEvent method from the EventDataAccess class.  
        /// </summary>
        /// <param name="user">The user's name</param>
        void UpdateUser(UserDataModel user);
        /// <summary>
        /// This method updates the user like the UpdateUser method, but it can also update the user's Username. 
        /// It must be mentioned that it does not update the calendar or the events of the user. 
        /// If you need to update those you will have to call the UpdateCalendar method from the CalendarDataAccess
        /// class and the UpdateEvent method from the EventDataAccess class.  
        /// </summary>
        /// <param name="user"></param>
        /// <param name="oldUsername"></param>
        void UpdateUserAndUsername(UserDataModel user, string oldUsername);
        /// <summary>
        /// This method deletes the user and and all their calendars, comments and events. It also removes any indirect connections
        /// That the user has with any event. Finally it sends relevant notifications to other users who either participated in an event
        /// that now is deleted, because the user does not exist any more and also notifications to those who have/participate in an event that
        /// the deleted user participated in.
        /// </summary>
        /// <param name="username">The user's name</param>
        void DeleteUser(string username);
    }
}