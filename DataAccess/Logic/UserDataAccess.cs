using DataAccess.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace DataAccess.Logic
{
    public class UserDataAccess : IUserDataAccess
    {

        private readonly IConfiguration Configuration;
        private readonly SQLiteConnection connection;
        private readonly ICalendarDataAccess CalendarDataAccess;
        private readonly IEventDataAccess EventDataAccess;

        public UserDataAccess(IConfiguration configuration, ICalendarDataAccess calendarDataAccess, IEventDataAccess eventDataAccess)
        {
            Configuration = configuration;
            connection = new SQLiteConnection(Configuration.GetConnectionString("Default"));
            CalendarDataAccess = calendarDataAccess;
            EventDataAccess = eventDataAccess;

        }

        /// <summary>
        /// This method returns all the users from the database and also all their calendars populated(with their events filled)
        /// and also all the user-event indirect or direct connections.
        /// </summary>
        /// <returns>All the users</returns>
        public List<UserDataModel> GetUsers()
        {
            connection.Open();
            string sqlQuery = "Select * from User";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
            SQLiteDataReader reader = command.ExecuteReader();

            List<UserDataModel> users = new List<UserDataModel>();

            while (reader.Read())
            {
                UserDataModel user = new UserDataModel();

                user.Username = reader.GetString(0);
                user.Password = reader.GetString(1);
                user.Fullname = reader.GetString(2);
                user.Email = reader.GetString(3);
                user.Phone = reader.GetString(4);
                user.Calendars = ReturnCalendarsOfUser(user.Username);

                users.Add(user);
            }
            connection.Close();

            return users;
        }

        /// <summary>
        /// This method returns one using the given username from the database and also all their calendars 
        /// populated(with their events filled) and also all the user-event indirect or direct connections.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <returns>The specified user</returns>
        public UserDataModel GetUser(string username)
        {
            connection.Open();
            string sqlQuery = "Select * from User where Username = @username";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@username", username);
            SQLiteDataReader reader = command.ExecuteReader();

            UserDataModel user = new UserDataModel();

            while (reader.Read())
            {
                user.Username = reader.GetString(0);
                user.Password = reader.GetString(1);
                user.Fullname = reader.GetString(2);
                user.Email = reader.GetString(3);
                user.Phone = reader.GetString(4);
                user.Calendars = ReturnCalendarsOfUser(username);
                user.EventsThatTheUserParticipates = ReturnEventsOfUser(username);

            }
            connection.Close();

            return user;
        }

        /// <summary>
        /// This is a private helper method that is used by the GetUser and GetUsers methods to connect the
        /// user with the calendars. This method calls the GetCalendar method which exists in the CalendarDataAccess
        /// and connects the calendars of the user with their corresponding events.
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <returns>All the user's calendars</returns>
        private List<CalendarDataModel> ReturnCalendarsOfUser(string username)
        {
            string sqlQuery = "Select Id from Calendar join User on " +
                "Calendar.UserUsername = User.Username where Username = @username ";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@username", username);
            SQLiteDataReader reader = command.ExecuteReader();

            List<CalendarDataModel> calendars = new List<CalendarDataModel>();

            while (reader.Read())
            {
                calendars.Add(CalendarDataAccess.GetCalendar(reader.GetInt32(0)));
            }

            return calendars;
        }

        /// <summary>
        /// This is a private helper method that is used by the GetUser and GetUsers methods to connect the
        /// the user with all the events he has a connection with. This connection can bedirect in case he has created those 
        /// events or indirect if he has not created those events and he has been referenced by another user.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>The events that the user is associated with</returns>
        private List<EventDataModel> ReturnEventsOfUser(string username)
        {
            string sqlQuery = "SELECT Event.Id FROM Event " +
                              "JOIN ParticipationInEvent ON ParticipationInEvent.EventId = Event.Id " +
                              "JOIN User ON ParticipationInEvent.UserUsername = User.Username " +
                              "WHERE User.Username = @username";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@username", username);
            SQLiteDataReader reader = command.ExecuteReader();

            List<EventDataModel> events = new List<EventDataModel>();

            while (reader.Read())
            {
                EventDataModel calendarEvent = EventDataAccess.GetEvent(reader.GetInt32(0));
                events.Add(calendarEvent);
            }

            return events;
        }

        

        /// <summary>
        /// This method creates a user. It must be mentioned that this method does not create calendars or events.
        /// In case you want to create those you can by using the createCalendar in the calendarDataAccess class
        /// or the createEvent in the eventDataAccess. The connection with the user ther is automatic by providing
        /// the user's username.
        /// </summary>
        //// <param name="user">The user model</param>
        public void CreateUser(UserDataModel user)
        {
            // check for other users with the same username
            if (GetUser(user.Username).Username != null)
            {
                throw new Exception("There is another user with the same username");
            }

            connection.Open();
            string sqlQuery = "INSERT INTO User (Username, Password, FullName, Email, Phone) " +
                              "VALUES (@username, @password, @fullName, @email, @phone)";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@password", user.Password);
            command.Parameters.AddWithValue("@fullName", user.Fullname);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@phone", user.Phone);
            command.ExecuteNonQuery();
            connection.Close();
        }

        /// <summary>
        /// This method deletes the user and and all their calendars and events. It also removes any indirect connections
        /// That the user has with any event. 
        /// </summary>
        /// <param /name="username">The user's name</param>
        public void DeleteUser(string username)
        {
            //get the user and delete all the calendars(this will also delete all the events and it will take care
            //of the ParticipationInEvent table for these events)
            UserDataModel user = GetUser(username);
            foreach (CalendarDataModel calendar in user.Calendars)
            {
                CalendarDataAccess.DeleteCalendar(calendar.Id);
            }

            connection.Open();

            //now remove the user entries from the participationInEvent table
            //delete the user itself
            string sqlQuery = "Delete From ParticipationInEvent where UserUsername = @username;" +
                              "Delete from User where Username = @username";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@username", username);
            command.ExecuteNonQuery();
            connection.Close();
        }

        /// <summary>
        /// This method updates the user. It must be mentioned that it does not update the calendar or the events
        /// of the user. If you need to update those you will have to call the UpdateCalendar method 
        /// from the CalendarDataAccess class and the UpdateEvent method from the EventDataAccess class.  
        /// </summary>
        /// <param name="user">The user's name</param>
        public void UpdateUser(UserDataModel user)
        {
            connection.Open();
            string sqlQuery = "Update User set Password = @password,FullName = @fullname, " +
                "Email = @email,Phone = @phone " +
                "where Username = @username";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@password", user.Password);
            command.Parameters.AddWithValue("@fullName", user.Fullname);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@phone", user.Phone);
            command.Parameters.AddWithValue("@username", user.Username);
            command.ExecuteNonQuery();
            connection.Close();
        }

        /// <summary>
        /// This method updates the user like the UpdateUser method, but it can also update the user's Username. 
        /// It must be mentioned that it does not update the calendar or the events of the user. 
        /// If you need to update those you will have to call the UpdateCalendar method from the CalendarDataAccess
        /// class and the UpdateEvent method from the EventDataAccess class.  
        /// </summary>
        /// <param name="user"></param>
        /// <param name="oldUsername"></param>
        public void UpdateUserAndUsername(UserDataModel user, string oldUsername)
        {
            connection.Open();
            string sqlQuery = "Update User set Username = @username,Password = @password,FullName = @fullname, " +
                "Email = @email,Phone = @phone " +
                "where Username = @oldUsername";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@password", user.Password);
            command.Parameters.AddWithValue("@fullName", user.Fullname);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@phone", user.Phone);
            command.Parameters.AddWithValue("@oldUsername", oldUsername);
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}
