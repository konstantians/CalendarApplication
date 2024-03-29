﻿using DataAccess.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace DataAccess.Logic
{
    /// <summary>
    /// handles the dataAccess for the calendars and calendar categories.
    /// </summary>
    public class CalendarDataAccess : ICalendarDataAccess
    {
        private readonly IConfiguration Configuration;
        private readonly SQLiteConnection connection;
        private readonly IEventDataAccess EventDataAccess;

        /// <summary>
        /// constructor used to instansiate the connection with the databese and it also is used for dependency injection
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="eventDataAccess"></param>
        public CalendarDataAccess(IConfiguration configuration,IEventDataAccess eventDataAccess)
        {
            Configuration = configuration;
            connection = new SQLiteConnection(Configuration.GetConnectionString("Default"));
            EventDataAccess = eventDataAccess;

        }

        /// <summary>
        /// This method returns all the calendars of a specific user from the database and their categories. It also returns the
        /// events of the calendars, which the method also fills/populates.
        /// </summary>
        /// <param name="userUsername">The username of the user who created/owns the calendar</param>
        /// <returns>All the user's calendars</returns>
        public List<CalendarDataModel> GetCalendars(string userUsername)
        {
            connection.Open();
            string sqlQuery = "SELECT * FROM Calendar WHERE UserUsername = @userUsername;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@userUsername", userUsername);
            SQLiteDataReader reader = command.ExecuteReader();

            List<CalendarDataModel> calendars = new List<CalendarDataModel>();

            while (reader.Read())
            {
                CalendarDataModel calendar = new CalendarDataModel();

                calendar.Id = reader.GetInt32(0);
                calendar.Title = reader.GetString(1);
                calendar.ImagePath = !reader.IsDBNull(2) ? reader.GetString(2) : "";
                calendar.Events = ReturnEventsOfCalendar(calendar.Id);

                sqlQuery = "SELECT Name FROM Category " +
                "JOIN Calendar ON Calendar.Id = Category.CalendarId WHERE Calendar.Id = @id;";
                command = new SQLiteCommand(sqlQuery, connection);

                command.Parameters.AddWithValue("@id", calendar.Id);
                SQLiteDataReader reader2 = command.ExecuteReader();

                while (reader2.Read())
                {
                    calendar.Categories.Add(reader2.GetString(0));
                }

                calendars.Add(calendar);
            }
            connection.Close();

            return calendars;
        }

        /// <summary>
        /// This method returns a calendar and its categories using its id from the database. It also returns the
        /// events of the calendar which the method also fills.
        /// </summary>
        /// <param name="id">The id of the calendar</param>
        /// <returns>The specified calendar</returns>
        public CalendarDataModel GetCalendar(int id)
        {
            connection.Open();
            string sqlQuery = "SELECT * FROM Calendar WHERE Id = @id;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@id", id);
            SQLiteDataReader reader = command.ExecuteReader();

            CalendarDataModel calendar = new CalendarDataModel();

            while (reader.Read())
            {
                calendar.Id = reader.GetInt32(0);
                calendar.Title = reader.GetString(1);
                calendar.ImagePath = !reader.IsDBNull(2) ? reader.GetString(2) : "";
                calendar.Events = ReturnEventsOfCalendar(id);

                sqlQuery = "SELECT Name FROM Category " +
                "JOIN Calendar ON Calendar.Id = Category.CalendarId WHERE Calendar.Id = @id;";
                command = new SQLiteCommand(sqlQuery, connection);

                command.Parameters.AddWithValue("@id", id);
                SQLiteDataReader reader2 = command.ExecuteReader();

                while (reader2.Read())
                {
                    calendar.Categories.Add(reader2.GetString(0));
                }
            }
            connection.Close();

            return calendar;
        }

        /// <summary>
        /// This is a helper method that is used by the GetCalendars and GetCalendar methods to connect
        /// the calendar/calendars with its/their events. It also fills these events.
        /// </summary>
        /// <param name="id">The id of the calendar</param>
        /// <returns>The events that exist in the calendar</returns>
        private List<EventDataModel> ReturnEventsOfCalendar(int id)
        {
            string sqlQuery = "SELECT Event.Id, ParticipationInEvent.UserUsername FROM Calendar " +
                "JOIN ParticipationInEvent ON ParticipationInEvent.CalendarId = Calendar.Id " +
                "JOIN Event ON ParticipationInEvent.EventId = Event.Id AND ParticipationInEvent.CalendarId = Calendar.Id " +
                "WHERE Calendar.Id = @id;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@id", id);
            SQLiteDataReader reader = command.ExecuteReader();

            List<EventDataModel> events = new List<EventDataModel>();

            while (reader.Read())
            {

                EventDataModel calendarEvent = EventDataAccess.GetEvent(reader.GetInt32(0), reader.GetString(1));

                events.Add(calendarEvent);
            }

            return events;
        }

        /// <summary>
        /// This method creates a calendar with its specified categories and connects it to the specified user. 
        /// It also returns the id of the newly created calendar. It must be noted that it does not create any 
        /// events and that if you wish to create events you should use the CreateEvent methods which exists
        /// in the EventDataAccess
        /// </summary>
        /// <param name="calendar">The calendar model</param>
        /// <param name="usernameOfUser">The user's username who created/owns the calendar</param>
        /// <returns>The id of the newly created calendar</returns>
        public int CreateCalendar(CalendarDataModel calendar, string usernameOfUser)
        {
            connection.Open();
            string sqlQuery = "INSERT INTO Calendar (Title, UserUsername) " +
                              "VALUES (@title, @userUsername);" +
                              "SELECT last_insert_rowid();";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@title", calendar.Title);
            command.Parameters.AddWithValue("@userUsername", usernameOfUser);
            int calendarId = Convert.ToInt32(command.ExecuteScalar());

            foreach (string category in calendar.Categories)
            {
                CreateCategory(category, calendarId);
            }

            connection.Close();

            return calendarId;
        }

        
        /// <summary>
        /// This method creates a category and connects it to the specified calendar.
        /// </summary>
        /// <param name="categoryName">The category name</param>
        /// <param name="calendarId">The calendar id that created/owns this category</param>
        /// <returns>The id of the newly created category</returns>
        private int CreateCategory(string categoryName, int calendarId)
        {
            string sqlQuery = "INSERT INTO Category (Name, CalendarId) " +
                              "VALUES (@name, @calendarId);" +
                              "SELECT last_insert_rowid();";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@name", categoryName);
            command.Parameters.AddWithValue("@calendarId", calendarId);

            return Convert.ToInt32(command.ExecuteScalar());
        
        }

        /// <summary>
        /// This method updates the calendar's information of the specified calendar. It must be mentioned that the
        /// writeCategory parameter is used to dictate whether or not the method should also update the categories of the
        /// calendar or not. This method does not update the event, in case you need to update the event use the 
        /// UpdateEvent method in the EventDataAccess class.
        /// </summary>
        /// <param name="calendar">The calendar model</param>
        /// <param name="writeCategory">The flag to change the calendar's categories</param>
        public void UpdateCalendar(CalendarDataModel calendar, bool writeCategory)
        {
            connection.Open();
            string sqlQuery = "UPDATE Calendar SET Title = @title " +
                              "WHERE Calendar.Id = @calendarId;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@title", calendar.Title);
            command.Parameters.AddWithValue("@calendarId", calendar.Id);
            command.ExecuteNonQuery();

            connection.Close();

            if (writeCategory)
            {
                //get the calendar from the database
                CalendarDataModel calendarInDatabase = GetCalendar(calendar.Id);

                connection.Open();
                //add the difference
                foreach (string category in calendar.Categories)
                {
                    if (calendarInDatabase.Categories.Contains(category))
                        continue;

                    sqlQuery = "INSERT INTO Category(Name, CalendarId) " +
                               "VALUES(@name, @calendarId);";
                    command = new SQLiteCommand(sqlQuery, connection);

                    command.Parameters.AddWithValue("@name", category);
                    command.Parameters.AddWithValue("@calendarId", calendar.Id);
                    command.ExecuteNonQuery();
                }

                //delete the difference
                foreach (string category in calendarInDatabase.Categories)
                {
                    if (calendar.Categories.Contains(category))
                        continue;

                    sqlQuery = "DELETE FROM Category " +
                               "WHERE name = @name;";
                    command = new SQLiteCommand(sqlQuery, connection);

                    command.Parameters.AddWithValue("@name", category);
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
            
            connection.Close();
        }

        /// <summary>
        /// This method is a very simple one and it is simply used to update the screenshot
        /// of the calendar which happens automatically. It should not be really used match
        /// and it should not be used by the user to update the screenshot of the calendar
        /// manually.
        /// </summary>
        /// <param name="calendarId">The id of the calendar</param>
        /// <param name="newScreenshotPath">The new path of the screenshot</param>
        public void UpdateCalendarScreenshot(int calendarId, string newScreenshotPath)
        {
            connection.Open();
            string sqlQuery = "UPDATE Calendar SET ImagePath = @imagePath " +
                              "WHERE Calendar.Id = @calendarId;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@calendarId", calendarId);
            command.Parameters.AddWithValue("@imagePath", newScreenshotPath);
            command.ExecuteNonQuery();

            connection.Close();
        }

        /// <summary>
        /// This method deletes the specified calendar with all its events and its categories. It also removes any 
        /// indirect connections or direct connections the calendar's event have. Additionally it removes the connection between
        /// the user and the calendar from the database. Finally the methods sends relavant notifications to the users who were
        /// affected by the deletion of the events which resided on the calendar if needed.
        /// </summary>
        /// <param name="id">The calendar's id</param>
        /// <param name="ownerOfCalendar">The owner of the calendar</param>
        public void DeleteCalendar(int id, string ownerOfCalendar)
        {
            connection.Open();

            //gets all the events of the calendar
            List<EventDataModel> events = ReturnEventsOfCalendar(id);

            connection.Close();

            //delete each event that it contains using the deleteEvent method
            foreach (EventDataModel calendarEvent in events)
            {
                EventDataAccess.DeleteEvent(calendarEvent.Id, ownerOfCalendar);
            }

            connection.Open();

            //delete all the categories of the calendar
            //and then delete the calendar itself
            string sqlQuery = "DELETE FROM Category WHERE CalendarId = @id;" +
                              "DELETE FROM Calendar WHERE Id = @id;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}
