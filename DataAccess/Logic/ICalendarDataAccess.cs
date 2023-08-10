using DataAccess.Models;
using System.Collections.Generic;

namespace DataAccess.Logic
{
    /// <summary>
    /// interface thas handles the calander and calendar categories dataAccess and is used for dependency injection.
    /// </summary>
    public interface ICalendarDataAccess
    {
        /// <summary>
        /// This method returns all the calendars of a specific user from the database and their categories. It also returns the
        /// events of the calendars, which the method also fills/populates.
        /// </summary>
        /// <param name="userUsername">The username of the user who created/owns the calendar</param>
        /// <returns>All the user's calendars</returns>
        List<CalendarDataModel> GetCalendars(string userUsername);
        /// <summary>
        /// This method returns a calendar and its categories using its id from the database. It also returns the
        /// events of the calendar which the method also fills.
        /// </summary>
        /// <param name="id">The id of the calendar</param>
        /// <returns>The specified calendar</returns>
        CalendarDataModel GetCalendar(int id);
        /// <summary>
        /// This method creates a calendar with its specified categories and connects it to the specified user. 
        /// It also returns the id of the newly created calendar. It must be noted that it does not create any 
        /// events and that if you wish to create events you should use the CreateEvent methods which exists
        /// in the EventDataAccess
        /// </summary>
        /// <param name="calendar">The calendar model</param>
        /// <param name="usernameOfUser">The user's username who created/owns the calendar</param>
        /// <returns>The id of the newly created calendar</returns>
        int CreateCalendar(CalendarDataModel calendar, string usernameOfUser);
        /// <summary>
        /// This method updates the calendar's information of the specified calendar. It must be mentioned that the
        /// writeCategory parameter is used to dictate whether or not the method should also update the categories of the
        /// calendar or not. This method does not update the event, in case you need to update the event use the 
        /// UpdateEvent method in the EventDataAccess class.
        /// </summary>
        /// <param name="calendar">The calendar model</param>
        /// <param name="writeCategory">The flag to change the calendar's categories</param>
        void UpdateCalendar(CalendarDataModel calendar, bool writeCategory);
        /// <summary>
        /// This method is a very simple one and it is simply used to update the screenshot
        /// of the calendar which happens automatically. It should not be really used match
        /// and it should not be used by the user to update the screenshot of the calendar
        /// manually.
        /// </summary>
        /// <param name="calendarId">The id of the calendar</param>
        /// <param name="newScreenshotPath">The new path of the screenshot</param>
        void UpdateCalendarScreenshot(int calendarId, string newScreenshotPath);
        /// <summary>
        /// This method deletes the specified calendar with all its events and its categories. It also removes any 
        /// indirect connections or direct connections the calendar's event have. Additionally it removes the connection between
        /// the user and the calendar from the database. Finally the methods sends relavant notifications to the users who were
        /// affected by the deletion of the events which resided on the calendar if needed.
        /// </summary>
        /// <param name="id">The calendar's id</param>
        /// <param name="ownerOfCalendar">The owner of the calendar</param>
        void DeleteCalendar(int id, string ownerOfCalendar);
        


    }
}