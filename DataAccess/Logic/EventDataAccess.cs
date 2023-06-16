using DataAccess.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Text;

namespace DataAccess.Logic
{
    public class EventDataAccess : IEventDataAccess
    {
        private readonly IConfiguration Configuration;
        private readonly SQLiteConnection connection;
        public EventDataAccess(IConfiguration configuration)
        {
            Configuration = configuration;
            connection = new SQLiteConnection(Configuration.GetConnectionString("Default"));

        }

        /// <summary>
        /// This method returns all the events of a specific calendar from the database. It also fills the 
        /// details the information of these events. It must be mentioned that the indirect connection of the users
        /// that the method returns do not have their calendars, events, categories(the complicated information) filled.
        /// In case you want to fill these information you can use the GetUser method in the UserDataAccess class using 
        /// the name of the user you want.
        /// </summary>
        /// <param name="calendarId">The Id of the calendar</param>
        /// <returns>All the events of a calendar</returns>
        public List<EventDataModel> GetEvents(int calendarId)
        {
            connection.Open();
            string sqlQuery = "Select * from Event where Event.CalendarId = @calendarId";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@calendarId", calendarId);
            SQLiteDataReader reader = command.ExecuteReader();

            List<EventDataModel> events = new List<EventDataModel>();

            while (reader.Read())
            {
                EventDataModel calendarEvent = new EventDataModel();

                calendarEvent.Id = reader.GetInt32(0);
                calendarEvent.Title = reader.GetString(1);
                calendarEvent.Description = reader.GetString(2);
                calendarEvent.StartingTime = DateTime.Parse(reader.GetString(3));
                calendarEvent.EndingTime = DateTime.Parse(reader.GetString(4));
                calendarEvent.AlertStatus = Convert.ToBoolean(reader.GetInt32(5));
                calendarEvent.UsersThatParticipateInTheEvent = ReturnUsersOfEvent(calendarEvent.Id);

                events.Add(calendarEvent);
            }
            connection.Close();

            return events;
        }

        /// <summary>
        /// This method returns the information of the specified event using the id parameter. It also fills the 
        /// details the information of this event. It must be mentioned that the indirect connection of the users
        /// that the method returns do not have their calendars, events, categories(the complicated information) filled.
        /// In case you want to fill these information you can use the GetUser method in the UserDataAccess class using 
        /// the name of the user you want.
        /// </summary>
        /// <param name="id">The specified event's id</param>
        /// <returns>The specified event</returns>
        public EventDataModel GetEvent(int id)
        {
            connection.Open();
            string sqlQuery = "Select * from Event where Id = @id";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@id", id);
            SQLiteDataReader reader = command.ExecuteReader();

            EventDataModel calendarEvent = new EventDataModel();

            while (reader.Read())
            {
                calendarEvent.Id = reader.GetInt32(0);
                calendarEvent.Title = reader.GetString(1);
                calendarEvent.Description = reader.GetString(2);
                calendarEvent.StartingTime = DateTime.Parse(reader.GetString(3));
                calendarEvent.EndingTime = DateTime.Parse(reader.GetString(4));
                calendarEvent.AlertStatus = Convert.ToBoolean(reader.GetInt32(5));
                calendarEvent.UsersThatParticipateInTheEvent = ReturnUsersOfEvent(calendarEvent.Id);
            }
            connection.Close();

            return calendarEvent;
        }

        /// <summary>
        /// This is a helper method that is used by the GetEvents and GetEvent methods to connect
        /// the specified event with the users it is asocciated with directly or indirectly. 
        /// These users do not have their calendar property filled. In case you want to also fill this information
        /// you can use the GetUser method in the UserDataAccess class using the name of the user you want.
        /// </summary>
        /// <param name="id">The event's id</param>
        /// <returns>The user's who are associated directly or indirectly with the specified event</returns>
        List<UserDataModel> ReturnUsersOfEvent(int id)
        {
            //TODO refactor this mess
            //filter the users that have this event
            string sqlQuery = "Select Username from User " +
                "join ParticipationInEvent on ParticipationInEvent.UserUsername = User.Username " +
                "join Event on ParticipationInEvent.EventId = Event.Id " +
                "where Event.Id = @id ";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@id", id);
            SQLiteDataReader reader = command.ExecuteReader();

            List<UserDataModel> users = new List<UserDataModel>();

            while (reader.Read())
            {
                //get all the users from user table
                sqlQuery = "Select * from User where Username = @username";
                command = new SQLiteCommand(sqlQuery, connection);

                command.Parameters.AddWithValue("@username", reader.GetString(0));
                SQLiteDataReader reader2 = command.ExecuteReader();

                UserDataModel user = new UserDataModel();
                
                //foreach user get basic details
                while (reader2.Read())
                {
                    user.Username = reader2.GetString(0);
                    user.Password = reader2.GetString(1);
                    user.Fullname = reader2.GetString(2);
                    user.Email = reader2.GetString(3);
                    user.Phone = reader2.GetString(4);
                }

                users.Add(user);
            }

            return users;
        }

        /// <summary>
        /// This method creates an event using the given information and connects it to the calendar id that is contained
        /// in and also creates the direct connection with the user that created it. It also returns the id of the 
        /// newly created event.
        /// </summary>
        /// <param name="calendarEvent">The event's information</param>
        /// <param name="username">The user's username who has created the calendar that contains the event</param>
        /// <param name="calendarId">The calendar's id that contains the event</param>
        /// <returns>The id of the newly created event</returns>
        public int CreateEvent(EventDataModel calendarEvent, string username, int calendarId)
        {
            connection.Open();
            string sqlQuery = "insert into Event (Title,Description,StartingTime,EndingTime,AlertStatus,CalendarId) " +
                              "values (@title,@description,@startingTime,@endingTime,@alertStatus,@calendarId);" +
                              "SELECT last_insert_rowid()";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@title", calendarEvent.Title);
            command.Parameters.AddWithValue("@description", calendarEvent.Description);
            command.Parameters.AddWithValue("@startingTime", calendarEvent.StartingTime.ToString());
            command.Parameters.AddWithValue("@endingTime", calendarEvent.EndingTime.ToString());
            command.Parameters.AddWithValue("@alertStatus", Convert.ToInt32(calendarEvent.AlertStatus));
            command.Parameters.AddWithValue("@calendarId", calendarId);

            //get the eventId
            int eventId = Convert.ToInt32(command.ExecuteScalar());

            //add an entry for the user that created that event
            sqlQuery = "insert into ParticipationInEvent (EventId,UserUsername)" +
                        "values (@eventId,@userUsername)";
            command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@eventId", eventId);
            command.Parameters.AddWithValue("@userUsername", username);
            command.ExecuteNonQuery();

            connection.Close();

            return eventId;
        }

        /// <summary>
        /// This method creates an event using the given information and connects it to the calendar id that is contained
        /// in and also creates the direct connection with the user that created it and the indirect connections
        /// with the users that event connects with. It also returns the id of the newly created event.
        /// </summary>
        /// <param name="calendarEvent">The event's information</param>
        /// <param name="usernamesOfUsers">The users' usernames who are associated with the event</param>
        /// <param name="calendarId">The calendar's id that contains the event</param>
        /// <returns></returns>
        public int CreateEvent(EventDataModel calendarEvent, List<string> usernamesOfUsers, int calendarId)
        {
            connection.Open();
            string sqlQuery = "insert into Event (Title,Description,StartingTime,EndingTime,AlertStatus,CalendarId) " +
                              "values (@title,@description,@startingTime,@endingTime,@alertStatus,@calendarId);" +
                              "SELECT last_insert_rowid()";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@title", calendarEvent.Title);
            command.Parameters.AddWithValue("@description", calendarEvent.Description);
            command.Parameters.AddWithValue("@startingTime", calendarEvent.StartingTime.ToString());
            command.Parameters.AddWithValue("@endingTime", calendarEvent.EndingTime.ToString());
            command.Parameters.AddWithValue("@alertStatus", Convert.ToInt32(calendarEvent.AlertStatus));
            command.Parameters.AddWithValue("@calendarId", calendarId);

            //get the eventId
            int eventId = Convert.ToInt32(command.ExecuteScalar());

            //add the users that participate and the event in intermediate table(many-to-many relationship)
            foreach (string username in usernamesOfUsers)
            {
                sqlQuery = "insert into ParticipationInEvent (EventId,UserUsername)" +
                           "values (@eventId,@userUsername)";
                command = new SQLiteCommand(sqlQuery, connection);

                command.Parameters.AddWithValue("@eventId", eventId);
                command.Parameters.AddWithValue("@userUsername", username);
                command.ExecuteNonQuery();
            }

            connection.Close();

            return eventId;
        }

        /// <summary>
        /// This method is used to create the connection between the users that participate in that event with the event.
        /// The reason why this method exists is to be used after the creation of the event, which means that
        /// the user can add users to the event at a later time.
        /// </summary>
        /// <param name="eventId">The event's id</param>
        /// <param name="usernamesOfUsers">The users' usernames with which the indirect connection will be created</param>
        /// <param name="calendarId">The calendar's id that contains the event</param>
        public void AddUsersToEvent(int eventId, List<string> usernamesOfUsers, int calendarId)
        {
            connection.Open();

            //add the users that participate and the event in intermediate table(many-to-many relationship)
            foreach (string username in usernamesOfUsers)
            {
                string sqlQuery = "insert into ParticipationInEvent (EventId,UserUsername)" +
                           "values (@eventId,@userUsername)";
                SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

                command.Parameters.AddWithValue("@eventId", eventId);
                command.Parameters.AddWithValue("@userUsername", username);
                command.ExecuteNonQuery();
            }

            connection.Close();
        }

        /// <summary>
        /// This method is used to create the connection between a user that participate in that event with the event.
        /// The reason why this method exists is to be used after the creation of the event, which means that
        /// the user can add a user to the event at a later time.
        /// </summary
        /// <param name="eventId">The event's id</param>
        /// <param name="username">The user's username with which the indirect connection will be created</param>
        /// <param name="calendarId">The calendar's id that contains the event</param>
        public void AddUserToEvent(int eventId, string username, int calendarId)
        {
            connection.Open();

            //add the user that participate and the event in intermediate table(many-to-many relationship)
            string sqlQuery = "insert into ParticipationInEvent (EventId,UserUsername)" +
                        "values (@eventId,@userUsername)";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@eventId", eventId);
            command.Parameters.AddWithValue("@userUsername", username);
            command.ExecuteNonQuery();

            connection.Close();
        }

        /// <summary>
        /// This methods update the information of the event. This method can not update the information of the users
        /// that the event is directly or indirectly connected to. In case you need to update their information use the 
        /// GetUser method from the UserDataAccess class to get the user/users and then update them using the UpdateUser
        /// method. The same proccess aplies for the calendar of the users who are associated with that event.
        /// </summary>
        /// <param name="calendarEvent">The event's information</param>
        public void UpdateEvent(EventDataModel calendarEvent)
        {
            connection.Open();
            string sqlQuery = "Update Event set Title = @title, Description = @description," +
                "StartingTime = @startingTime, EndingTime = @endingTime, AlertStatus = @alertStatus " +
                "where Id = @id";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@title", calendarEvent.Title);
            command.Parameters.AddWithValue("@description", calendarEvent.Description);
            command.Parameters.AddWithValue("@startingTime", calendarEvent.StartingTime.ToString());
            command.Parameters.AddWithValue("@endingTime", calendarEvent.EndingTime.ToString());
            command.Parameters.AddWithValue("@alertStatus", Convert.ToInt32(calendarEvent.AlertStatus));
            command.Parameters.AddWithValue("@id", calendarEvent.Id);

            command.ExecuteNonQuery();

            connection.Close();
        }
        //public void UpdateEvent(EventDataModel calendarEvent)
        //{
        //    connection.Open();
        //    string sqlQuery = $"Update Event set Title = {calendarEvent.Title}, Description = {calendarEvent.Description}, StartingTime = {calendarEvent.StartingTime}, EndingTime = {calendarEvent.EndingTime}, AlertStatus = {calendarEvent.AlertStatus} where Id = {calendarEvent.Id}";
        //    SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
        //
        //    command.ExecuteNonQuery();
        //
        //    connection.Close();
        //}

        /// <summary>
        /// This method deletes the specified event and removes all the direct and indirect connections it has with 
        /// any user/users. It also removes its connection with the calendar that contained the event. 
        /// </summary>
        /// <param name="id">The event's id</param>
        public void DeleteEvent(int id)
        {
            connection.Open();
            string sqlQuery = "Delete From ParticipationInEvent where EventId = @id;" +
                              "Delete from Event where Id = @id";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@id", id);

            command.ExecuteNonQuery();

            connection.Close();
        }
    }
}
