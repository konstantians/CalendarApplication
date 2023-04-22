using DataAccess.Models;
using System.Collections.Generic;

namespace DataAccess.Logic
{
    public interface ICalendarDataAccess
    {
        CalendarDataModel GetCalendar(int id);
        List<CalendarDataModel> GetCalendars(string userUsername);
        int CreateCalendar(CalendarDataModel calendar, string usernameOfUser);
        void UpdateCalendar(CalendarDataModel calendar, bool writeCategory);
        void DeleteCalendar(int id);
        
    }
}