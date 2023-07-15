using DataAccess.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

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

        //TODO maybe check it out more
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
            string sqlQuery = "SELECT Id, Title, Description, StartingTime, EndingTime, EventCreator, AlertStatus FROM Event " +
                "JOIN ParticipationInEvent ON Event.Id = ParticipationInEvent.EventId " +
                "WHERE Event.CalendarId = @calendarId AND SoftDeleted = 0;";
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
                calendarEvent.EventCreatorName = reader.GetString(5);
                calendarEvent.AlertStatus = Convert.ToBoolean(reader.GetInt32(6));
                calendarEvent.UsersThatParticipateInTheEvent = ReturnUsersOfEvent(calendarEvent.Id);
                calendarEvent.EventComments = ReturnCommentsOfEvent(calendarEvent.Id);

                events.Add(calendarEvent);
            }
            connection.Close();

            return events;
        }

        /// <summary>
        /// This method returns the information of the specified event of the given calendar using their ids parameters. It also fills the 
        /// details the information of this event(only the static details of the event).It must be mentioned that the indirect connection of the users
        /// that the method returns do not have their calendars, events, categories(the complicated information) filled.
        /// In case you want to fill these information you can use the GetUser method in the UserDataAccess class using 
        /// the name of the user you want.
        /// </summary>
        /// <param name="id">The specified event's id</param>
        /// <returns>The specified event of the calendar</returns>
        public EventDataModel GetEvent(int id)
        {
            connection.Open();
            string sqlQuery = "SELECT Id, Title, Description, StartingTime, EndingTime, EventCreator FROM Event " +
                "WHERE Event.Id = @id AND SoftDeleted = 0;";
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
                calendarEvent.EventCreatorName = reader.GetString(5);
                calendarEvent.UsersThatParticipateInTheEvent = ReturnUsersOfEvent(calendarEvent.Id);
                calendarEvent.EventComments = ReturnCommentsOfEvent(calendarEvent.Id);
            }
            connection.Close();

            return calendarEvent;
        }

        /// <summary>
        /// This method returns the information of the specified event of the given calendar using their ids parameters. It also fills the 
        /// details the information of this event(also the dynamic information of the event). It must be mentioned that the indirect connection of the users
        /// that the method returns do not have their calendars, events, categories(the complicated information) filled.
        /// In case you want to fill these information you can use the GetUser method in the UserDataAccess class using 
        /// the name of the user you want.
        /// </summary>
        /// <param name="id">The specified event's id</param>
        /// <param name="username">The user who we want the their dynamic details</param>
        /// <returns>The specified event of the calendar</returns>
        public EventDataModel GetEvent(int id, string username)
        {
            connection.Open();
            string sqlQuery = "SELECT Id, Title, Description, StartingTime, EndingTime, EventCreator, AlertStatus FROM Event " +
                "JOIN ParticipationInEvent ON Event.Id = ParticipationInEvent.EventId " +
                "WHERE Event.Id = @id AND ParticipationInEvent.UserUsername = @username AND SoftDeleted = 0;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@username", username);
            SQLiteDataReader reader = command.ExecuteReader();

            EventDataModel calendarEvent = new EventDataModel();

            while (reader.Read())
            {
                calendarEvent.Id = reader.GetInt32(0);
                calendarEvent.Title = reader.GetString(1);
                calendarEvent.Description = reader.GetString(2);
                calendarEvent.StartingTime = DateTime.Parse(reader.GetString(3));
                calendarEvent.EndingTime = DateTime.Parse(reader.GetString(4));
                calendarEvent.EventCreatorName = reader.GetString(5);
                calendarEvent.AlertStatus = Convert.ToBoolean(reader.GetInt32(6));
                calendarEvent.UsersThatParticipateInTheEvent = ReturnUsersOfEvent(calendarEvent.Id);
                calendarEvent.EventComments = ReturnCommentsOfEvent(calendarEvent.Id);
            }
            connection.Close();

            return calendarEvent;
        }

        /// <summary>
        /// returns the events like GetEvent ignoring the soft delete
        /// </summary>
        /// <param name="id">The specified event's id</param>
        /// <returns>The specified event of the calendar</returns>
        public EventDataModel GetEventForNotifications(int id)
        {
            //if it a soft deleted event then the GetEvent method should return null
            if (GetEvent(id).Title == null)
            {
                connection.Open();
                string sqlQuery = "SELECT Id, Title, Description, StartingTime, EndingTime, EventCreator FROM Event WHERE Event.Id = @id;";
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
                    calendarEvent.EventCreatorName = reader.GetString(5);
                }
                connection.Close();

                return calendarEvent;
            }
            //otherwise use the typical GetEvent method
            else
            {
                return GetEvent(id);
            }
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
            //filter the users that have this event
            string sqlQuery = "SELECT Username, Password, Fullname, Email, Phone FROM User " +
                "JOIN ParticipationInEvent ON ParticipationInEvent.UserUsername = User.Username " +
                "JOIN Event ON ParticipationInEvent.EventId = Event.Id " +
                "WHERE Event.Id = @id;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@id", id);
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

                users.Add(user);
            }

            return users;
        }

        List<CommentDataModel> ReturnCommentsOfEvent(int id)
        {
            //filter the users that have this event
            string sqlQuery = "SELECT Title, Description, StartingTime, EndingTime, UserUsername, CommentText, CommentDate FROM Event " +
                "JOIN Comment ON Event.Id = Comment.EventId " +
                "WHERE Event.Id = @id;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@id", id);
            SQLiteDataReader reader = command.ExecuteReader();

            List<CommentDataModel> comments = new List<CommentDataModel>();

            while (reader.Read())
            {
                CommentDataModel comment = new CommentDataModel();
                comment.EventOfComment = new EventDataModel(id, reader.GetString(0), reader.GetString(1),
                    DateTime.Parse(reader.GetString(2)), DateTime.Parse(reader.GetString(3)), reader.GetString(4));
                comment.UserWhoMadeTheComment = ReturnUserOfComment(reader.GetString(4));
                comment.CommentText = reader.GetString(5);
                comment.CommentDate = DateTime.Parse(reader.GetString(6));

                comments.Add(comment);
            }

            return comments;
        }

        UserDataModel ReturnUserOfComment(string userUsername)
        {
            string sqlQuery = "SELECT Username, Password, FullName, Email, Phone From User WHERE User.Username = @username;";

            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@username", userUsername);

            SQLiteDataReader reader = command.ExecuteReader();

            UserDataModel user = new UserDataModel();

            while (reader.Read())
            {
                user.Username = reader.GetString(0);
                user.Password = reader.GetString(1);
                user.Fullname = reader.GetString(2);
                user.Email = reader.GetString(3);
                user.Phone = reader.GetString(4);
            }

            return user;
        }



        /// <summary>
        /// This method creates an event using the given information and connects it to the calendar id that is contained
        /// in and also creates the direct connection with the user that created it. It also returns the id of the 
        /// newly created event. Make sure that the 
        /// </summary>
        /// <param name="calendarEvent">The event's information</param>
        /// <param name="username">The user's username who has created the calendar that contains the event</param>
        /// <param name="calendarId">The calendar's id that contains the event</param>
        /// <returns>The id of the newly created event</returns>
        public int CreateEvent(EventDataModel calendarEvent, string username, int calendarId)
        {
            connection.Open();
            string sqlQuery = "INSERT INTO Event (Title, Description, StartingTime, EndingTime, SoftDeleted, CalendarId, EventCreator) " +
                              "VALUES (@title, @description, @startingTime, @endingTime, @softDeleted, @calendarId, @eventCreator);" +
                              "SELECT last_insert_rowid();";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@title", calendarEvent.Title);
            command.Parameters.AddWithValue("@description", calendarEvent.Description);
            command.Parameters.AddWithValue("@startingTime", calendarEvent.StartingTime.ToString());
            command.Parameters.AddWithValue("@endingTime", calendarEvent.EndingTime.ToString());
            command.Parameters.AddWithValue("@softDeleted", Convert.ToInt32(false));
            command.Parameters.AddWithValue("@calendarId", calendarId);
            command.Parameters.AddWithValue("@eventCreator", username);

            //get the eventId
            int eventId = Convert.ToInt32(command.ExecuteScalar());

            //add an entry for the user that created that event
            sqlQuery = "INSERT INTO ParticipationInEvent (EventId, UserUsername, CalendarId, AlertStatus) " +
                       "VALUES (@eventId, @userUsername, @calendarId, @alertStatus);";
            command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@eventId", eventId);
            command.Parameters.AddWithValue("@userUsername", username);
            command.Parameters.AddWithValue("@calendarId", calendarId);
            command.Parameters.AddWithValue("@alertStatus", Convert.ToInt32(calendarEvent.AlertStatus));
            command.ExecuteNonQuery();

            connection.Close();

            List<string> comments = new List<string>();
            foreach (CommentDataModel eventComment in calendarEvent.EventComments)
            {
                comments.Add(eventComment.CommentText);
            }
            CreateComments(comments, eventId, username);

            return eventId;
        }

        /// <summary>
        /// This method creates an event using the given information and connects it to the calendar id that is contained
        /// in and also creates the direct connection with the user that created it and the indirect connections
        /// with the users that event connects with. It also returns the id of the newly created event.
        /// </summary>
        /// <param name="calendarEvent">The event's information</param>
        /// <param name="usernameOfCreator">the event's creator name</param>
        /// <param name="usernamesOfUsers">The users' usernames who are associated(are invited) with the event</param>
        /// <param name="calendarId">The calendar's id that contains the event</param>
        /// <returns></returns>
        public int CreateEvent(EventDataModel calendarEvent, string usernameOfCreator, List<string> usernamesOfUsers, int calendarId)
        {
            connection.Open();
            string sqlQuery = "INSERT INTO Event (Title, Description, StartingTime, EndingTime, SoftDeleted, CalendarId, EventCreator) " +
                              "VALUES (@title, @description, @startingTime, @endingTime, @softDeleted, @calendarId, @eventCreator);" +
                              "SELECT last_insert_rowid();";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@title", calendarEvent.Title);
            command.Parameters.AddWithValue("@description", calendarEvent.Description);
            command.Parameters.AddWithValue("@startingTime", calendarEvent.StartingTime.ToString());
            command.Parameters.AddWithValue("@endingTime", calendarEvent.EndingTime.ToString());
            command.Parameters.AddWithValue("@softDeleted", Convert.ToInt32(false));
            command.Parameters.AddWithValue("@calendarId", calendarId);
            command.Parameters.AddWithValue("@eventCreator", usernameOfCreator);

            //get the eventId
            int eventId = Convert.ToInt32(command.ExecuteScalar());

            connection.Close();

            connection.Open();

            //add an entry for the user that created that event
            sqlQuery = "INSERT INTO ParticipationInEvent (EventId, UserUsername, CalendarId, AlertStatus) " +
                       "VALUES (@eventId,@userUsername, @calendarId, @alertStatus);";
            command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@eventId", eventId);
            command.Parameters.AddWithValue("@userUsername", usernameOfCreator);
            command.Parameters.AddWithValue("@calendarId", calendarId);
            command.Parameters.AddWithValue("@alertStatus", Convert.ToInt32(calendarEvent.AlertStatus));
            command.ExecuteNonQuery();

            connection.Close();

            List<string> comments = new List<string>();
            foreach (CommentDataModel eventComment in calendarEvent.EventComments)
            {
                comments.Add(eventComment.CommentText);
            }
            CreateComments(comments, eventId, usernameOfCreator);

            //send notifications to everyone who was invited
            InviteUsersToEvent(eventId, usernameOfCreator, usernamesOfUsers);

            return eventId;
        }

        public void CreateComments(List<string> comments, int eventId, string creatorOfComment)
        {
            string sqlQuery;
            SQLiteCommand command;

            connection.Open();
            foreach (string comment in comments)
            {
                sqlQuery = "INSERT INTO Comment (EventId, UserUsername, CommentText, CommentDate) " +
                           "VALUES (@eventId, @userUsername, @commentText, @commentDate);";

                command = new SQLiteCommand(sqlQuery, connection);

                command.Parameters.AddWithValue("@eventId", eventId);
                command.Parameters.AddWithValue("@userUsername", creatorOfComment);
                command.Parameters.AddWithValue("@commentText", comment);
                command.Parameters.AddWithValue("@commentDate", DateTime.Now.ToString());
                command.ExecuteNonQuery();
            }

            //find everyone who participates in the event and is not the person who created the comment
            sqlQuery = "SELECT UserUsername FROM ParticipationInEvent WHERE ParticipationInEvent.EventId = @eventId AND ParticipationInEvent.UserUsername != @userUsername;";

            command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@eventId", eventId);
            command.Parameters.AddWithValue("@userUsername", creatorOfComment);

            SQLiteDataReader reader = command.ExecuteReader();

            List<string> membersOfEvent = new List<string>();

            while (reader.Read())
            {
                membersOfEvent.Add(reader.GetString(0));
            }

            connection.Close();

            int count = 0;
            foreach (string comment in comments)
            {
                CreateNotifications(new NotificationDataModel(eventId, DateTime.Now,
                    false, false, false, false, true, false, false, false), membersOfEvent, count);
                count++;
            }
        }

        public void DeleteComment(CommentDataModel commentDataModel)
        {
            connection.Open();
            string sqlQuery = "DELETE FROM Comment WHERE EventId = @eventId AND UserUsername = @userUsername AND CommentText = @commentText AND CommentDate = @commentDate;";

            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@eventId", commentDataModel.EventOfComment.Id);
            command.Parameters.AddWithValue("@userUsername", commentDataModel.UserWhoMadeTheComment.Username);
            command.Parameters.AddWithValue("@commentText", commentDataModel.CommentText);
            command.Parameters.AddWithValue("@commentDate", commentDataModel.CommentDate);
            command.ExecuteNonQuery();

            //find everyone who participates in the event and is not the person who created the comment
            sqlQuery = "SELECT UserUsername FROM ParticipationInEvent WHERE ParticipationInEvent.EventId = @eventId AND ParticipationInEvent.UserUsername != @userUsername;";

            command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@eventId", commentDataModel.EventOfComment.Id);
            command.Parameters.AddWithValue("@userUsername", commentDataModel.UserWhoMadeTheComment.Username);

            SQLiteDataReader reader = command.ExecuteReader();

            List<string> membersOfEvent = new List<string>();

            while (reader.Read())
            {
                membersOfEvent.Add(reader.GetString(0));
            }

            connection.Close();

            CreateNotifications(new NotificationDataModel(commentDataModel.EventOfComment.Id, DateTime.Now,
                false, false, false, false, false, true, false, false), membersOfEvent);
        }

        private void CreateNotifications(NotificationDataModel notificationDataModel, List<string> receiversOfNotification, int count = 0)
        {
            connection.Open();

            //create a notification for each one of them
            foreach (string memberOfEvent in receiversOfNotification)
            {

                string sqlQuery = "INSERT INTO Notification (EventId, UserUsername, NotificationTime, InvitationPending, EventAccepted, " +
                    "EventRejected, EventChanged, CommentAdded, CommentDeleted, EventDeleted, AlertNotification) " +
                           "VALUES (@eventId, @userUsername, @notificationTime, @invitationPending ,@eventAccepted, " +
                           "@eventRejected, @eventChanged, @commentAdded, @commentDeleted, @eventDeleted, @alertNotification);";
                SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

                command.Parameters.AddWithValue("@eventId", notificationDataModel.EventOfNotification.Id);
                command.Parameters.AddWithValue("@userUsername", memberOfEvent);
                command.Parameters.AddWithValue("@notificationTime", notificationDataModel.NotificationTime.AddSeconds(count).ToString());
                command.Parameters.AddWithValue("@invitationPending", Convert.ToInt32(notificationDataModel.InvitationPending));
                command.Parameters.AddWithValue("@eventAccepted", Convert.ToInt32(notificationDataModel.EventAccepted));
                command.Parameters.AddWithValue("@eventRejected", Convert.ToInt32(notificationDataModel.EventRejected));
                command.Parameters.AddWithValue("@eventChanged", Convert.ToInt32(notificationDataModel.EventChanged));
                command.Parameters.AddWithValue("@commentAdded", Convert.ToInt32(notificationDataModel.CommentAdded));
                command.Parameters.AddWithValue("@commentDeleted", Convert.ToInt32(notificationDataModel.CommentDeleted));
                command.Parameters.AddWithValue("@eventDeleted", Convert.ToInt32(notificationDataModel.EventDeleted));
                command.Parameters.AddWithValue("@alertNotification", Convert.ToInt32(notificationDataModel.AlertNotification));

                command.ExecuteNonQuery();
            }

            connection.Close();
        }

        //TODO thing about the usernameOfCreator
        /// <summary>
        /// This method is used to create the connection between the users that participate in that event with the event.
        /// The reason why this method exists is to be used after the creation of the event, which means that
        /// the user can add users to the event at a later time.
        /// </summary>
        /// <param name="eventId">The event's id</param>
        /// <param name="usernamesOfUsers">The users' usernames with which the indirect connection will be created</param>
        /// <param name="calendarId">The calendar's id that contains the event</param>
        public void InviteUsersToEvent(int eventId, string usernameOfCreator, List<string> usernamesOfUsers)
        {
            CreateNotifications(new NotificationDataModel(eventId, DateTime.Now, true, false, false, false, false, false, false, false), usernamesOfUsers);
        }

        public void InviteUserToEvent(int eventId, string usernameOfCreator, string usernameOfInvitedUser)
        {
            CreateNotifications(new NotificationDataModel(eventId, DateTime.Now, true, false, false, false, false, false, false, false), new List<string> { usernameOfInvitedUser });
        }

        public void AcceptInvitation(int eventId, string usernameOfInvitedUser, DateTime notificationTime, int calendarId, bool alertStatus)
        {
            //delete the pending notification
            DeleteNotification(eventId, usernameOfInvitedUser, notificationTime);

            connection.Open();

            //add the user that participate and the event in intermediate table(many-to-many relationship)
            string sqlQuery = "INSERT INTO ParticipationInEvent(EventId, UserUsername, CalendarId, AlertStatus) " +
                "VALUES(@eventId, @userUsername, @calendarId, @alertStatus);";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@eventId", eventId);
            command.Parameters.AddWithValue("@userUsername", usernameOfInvitedUser);
            command.Parameters.AddWithValue("@calendarId", calendarId);
            command.Parameters.AddWithValue("@alertStatus", Convert.ToInt32(alertStatus));
            command.ExecuteNonQuery();

            connection.Close();

            //send notification to the creator of the event
            //find the creator of the event
            connection.Open();
            sqlQuery = "SELECT EventCreator FROM Event WHERE Event.Id = @eventId;";
            command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@eventId", eventId);
            SQLiteDataReader reader = command.ExecuteReader();
            string creatorName = "";

            while (reader.Read())
            {
                creatorName = reader.GetString(0);
            }

            connection.Close();

            //send accepted notification to the creator of the event
            CreateNotifications(new NotificationDataModel(eventId, DateTime.Now, false, true, false, false, false, false, false, false), new List<string> { creatorName });
        }

        public void RejectInvitation(int eventId, string usernameOfInvitedUser, DateTime notificationTime)
        {
            //delete the pending notification
            DeleteNotification(eventId, usernameOfInvitedUser, notificationTime);

            //send notification to the creator of the event
            //find the creator of the event
            connection.Open();
            string sqlQuery = "SELECT EventCreator FROM Event WHERE Event.Id = @eventId;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@eventId", eventId);
            SQLiteDataReader reader = command.ExecuteReader();
            string creatorName = "";

            while (reader.Read())
            {
                creatorName = reader.GetString(0);
            }

            connection.Close();
            //send rejected notification to the creator of the event
            CreateNotifications(new NotificationDataModel(eventId, DateTime.Now, false, false, true, false, false, false, false, false), new List<string> { creatorName });
        }

        public void UpdateEvent(EventDataModel eventDataModel, string userWhoUpdatedTheEvent)
        {
            if (GetEvent(eventDataModel.Id).EventCreatorName == userWhoUpdatedTheEvent)
            {
                UpdateNativeEvent(eventDataModel, userWhoUpdatedTheEvent);
            }
            else
            {
                UpdateForeignEvent(eventDataModel, userWhoUpdatedTheEvent);
            }
        }

        /// <summary>
        /// This methods update the information of the event. This method can not update the information of the users
        /// that the event is directly or indirectly connected to. In case you need to update their information use the 
        /// GetUser method from the UserDataAccess class to get the user/users and then update them using the UpdateUser
        /// method. The same proccess aplies for the calendar of the users who are associated with that event.
        /// </summary>
        /// <param name="calendarEvent">The event's information</param>
        /// <param>
        public void UpdateNativeEvent(EventDataModel calendarEvent, string username)
        {
            connection.Open();
            string sqlQuery = "UPDATE Event SET Title = @title, Description = @description, " +
                "StartingTime = @startingTime, EndingTime = @endingTime " +
                "WHERE Id = @id;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@title", calendarEvent.Title);
            command.Parameters.AddWithValue("@description", calendarEvent.Description);
            command.Parameters.AddWithValue("@startingTime", calendarEvent.StartingTime.ToString());
            command.Parameters.AddWithValue("@endingTime", calendarEvent.EndingTime.ToString());
            command.Parameters.AddWithValue("@id", calendarEvent.Id);

            command.ExecuteNonQuery();

            connection.Close();

            connection.Open();
            sqlQuery = "UPDATE ParticipationInEvent SET alertStatus = @alertStatus " +
                "WHERE EventId = @eventId AND UserUsername = @userUsername;";
            command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@alertStatus", Convert.ToInt32(calendarEvent.AlertStatus));
            command.Parameters.AddWithValue("@eventId", calendarEvent.Id);
            command.Parameters.AddWithValue("@userUsername", username);

            command.ExecuteNonQuery();

            connection.Close();

            //Send notification to everyone who was not the user(the creator of the event) who updated the event
            //find the users
            connection.Open();
            sqlQuery = "SELECT UserUsername FROM ParticipationInEvent " +
                "WHERE ParticipationInEvent.UserUsername != @userUsername AND ParticipationInEvent.EventId = @eventId;";
            command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@eventId", calendarEvent.Id);
            command.Parameters.AddWithValue("@userUsername", username);
            SQLiteDataReader reader = command.ExecuteReader();
            List<string> membersOfEvent = new List<string>();

            while (reader.Read())
            {
                membersOfEvent.Add(reader.GetString(0));
            }

            connection.Close();

            //send changed notification
            CreateNotifications(new NotificationDataModel(calendarEvent.Id, DateTime.Now, false, false, false, true, false, false, false, false), membersOfEvent);
        }

        public void UpdateForeignEvent(EventDataModel calendarEvent, string username)
        {
            connection.Open();
            string sqlQuery = "UPDATE ParticipationInEvent SET alertStatus = @alertStatus " +
                "WHERE EventId = @eventId AND UserUsername = @userUsername;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@alertStatus", Convert.ToInt32(calendarEvent.AlertStatus));
            command.Parameters.AddWithValue("@eventId", calendarEvent.Id);
            command.Parameters.AddWithValue("@userUsername", username);

            command.ExecuteNonQuery();

            connection.Close();
        }

        /// <summary>
        /// This method deletes the specified event and removes all the direct and indirect connections it has with 
        /// any user/users. It also removes its connection with the calendar that contained the event. 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userWhoDeletedTheEvent"></param>
        public void DeleteEvent(int id, string userWhoDeletedTheEvent)
        {
            if (GetEvent(id).EventCreatorName == userWhoDeletedTheEvent)
            {
                DeleteNativeEvent(id, userWhoDeletedTheEvent);
            }
            else
            {
                DeleteForeignEvent(id, userWhoDeletedTheEvent);
            }
        }

        //TODO implement soft delete
        /// <summary>
        /// This method deletes the specified event and removes all the direct and indirect connections it has with 
        /// any user/users. It also removes its connection with the calendar that contained the event. 
        /// </summary>
        /// <param name="id">The event's id</param>
        public void DeleteNativeEvent(int id, string userWhoDeletedTheEvent)
        {
            //find the users of the event
            connection.Open();
            string sqlQuery = "SELECT UserUsername FROM ParticipationInEvent " +
               "JOIN EVENT ON ParticipationInEvent.EventId = Event.Id " +
               "AND Event.EventCreator != ParticipationInEvent.UserUsername " +
               "WHERE EVENT.Id = @id;";

            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@id", id);

            SQLiteDataReader reader = command.ExecuteReader();
            List<string> membersOfEvent = new List<string>();

            while (reader.Read())
            {
                membersOfEvent.Add(reader.GetString(0));
            }
            connection.Close();

            //change to soft delete the event
            connection.Open();

            //if noone else has been invited to the event, then hard delete immediatelly
            if (membersOfEvent.Count == 0)
            {
                sqlQuery = "DELETE FROM ParticipationInEvent WHERE EventId = @id;" +
                           "DELETE FROM Notification WHERE EventId = @id AND InvitationPending = 1;" +
                           "DELETE FROM Comment WHERE EventId = @id;" +
                           "DELETE FROM Event WHERE Id = @id;";
            }
            //else soft delete
            else
            {
                sqlQuery = "DELETE FROM ParticipationInEvent WHERE EventId = @id;" +
                           "DELETE FROM Notification WHERE EventId = @id AND InvitationPending = 1;" +
                           "DELETE FROM Comment WHERE EventId = @id;";
            }
            command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@id", id);

            command.ExecuteNonQuery();

            connection.Close();

            //update for soft delete
            if (membersOfEvent.Count != 0)
            {
                //TODO change the soft delete field
                connection.Open();
                sqlQuery = "UPDATE Event SET SoftDeleted = 1 WHERE id = @id;";
                command = new SQLiteCommand(sqlQuery, connection);

                command.Parameters.AddWithValue("@id", id);

                command.ExecuteNonQuery();
                connection.Close();

                //send notification that the event was deleted to everyone else(everyone who is not the creator)
                CreateNotifications(new NotificationDataModel(id, DateTime.Now, false, false, false, false, false, false, true, false), membersOfEvent);
            }
        }

        /// <summary>
        /// This method deletes the specified event and removes all the direct and indirect connections it has with 
        /// any user/users. It also removes its connection with the calendar that contained the event. 
        /// </summary>
        /// <param name="id">The event's id</param>
        /// <param name="userWhoDeletedTheEvent">The user who deleted/removed the event from their calendar</param>
        public void DeleteForeignEvent(int id, string userWhoDeletedTheEvent)
        {
            //delete the user's participation
            connection.Open();
            string sqlQuery = "DELETE FROM ParticipationInEvent WHERE EventId = @id AND UserUsername = @userUsername;" +
                "DELETE FROM Comment WHERE EventId = @id AND UserUsername = @userUsername;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@userUsername", userWhoDeletedTheEvent);

            command.ExecuteNonQuery();

            connection.Close();

            //Send notification to everyone else
            //find the users
            connection.Open();
            sqlQuery = "SELECT UserUsername FROM ParticipationInEvent " +
               "JOIN Event ON ParticipationInEvent.EventId = Event.Id " +
               "WHERE Event.Id = @id;";

            command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@id", id);
            SQLiteDataReader reader = command.ExecuteReader();

            List<string> membersOfEvent = new List<string>();

            while (reader.Read())
            {
                membersOfEvent.Add(reader.GetString(0));
            }

            connection.Close();
            //send deleted notification to everyone else
            CreateNotifications(new NotificationDataModel(id, DateTime.Now, false, false, false, false, false, false, true, false), membersOfEvent);
        }

        public void SendAlertNotification(int eventId, string username)
        {
            //send the notification
            CreateNotifications(new NotificationDataModel(eventId, DateTime.Now, false, false, false, false, false, false, false, true), new List<string> { username });

            //update the alert status
            connection.Open();
            string sqlQuery = "UPDATE ParticipationInEvent SET AlertStatus = 0 " +
                "WHERE EventId = @eventId AND UserUsername = @username;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@eventId", eventId);
            command.Parameters.AddWithValue("@username", username);

            command.ExecuteNonQuery();

            connection.Close();
        }

        public void DeleteNotification(int eventId, string userUsername, DateTime notificationTime)
        {
            //delete the notification
            //TODO fix that event id = 0;
            connection.Open();
            string sqlQuery = "DELETE FROM Notification WHERE EventId = @eventId AND UserUsername = @userUsername AND NotificationTime = @notificationTime;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@eventId", eventId);
            command.Parameters.AddWithValue("@userUsername", userUsername);
            command.Parameters.AddWithValue("@notificationTime", notificationTime.ToString());

            command.ExecuteNonQuery();

            connection.Close();

            //test for hard deletion of the event
            //Check if the event is soft deleted
            connection.Open();
            sqlQuery = "SELECT SoftDeleted FROM Event WHERE Event.Id = @id;";

            command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@id", eventId);
            SQLiteDataReader reader = command.ExecuteReader();
            bool isSoftDeleted = false;

            while (reader.Read())
            {
                isSoftDeleted = Convert.ToBoolean(reader.GetInt32(0));
            }

            //if it is not soft deleted
            if (!isSoftDeleted)
            {
                connection.Close();
                return;
            }

            //otherwise find how many notifications exis for the soft deleted event
            sqlQuery = "SELECT COUNT(id) FROM Notification " +
                       "JOIN Event ON Notification.EventId = Event.Id " +
                       "WHERE Event.SoftDeleted = 1 AND Event.Id = @id";

            command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@id", eventId);
            reader = command.ExecuteReader();
            int countOfSoftDeletedNotifications = 0;

            while (reader.Read())
            {
                countOfSoftDeletedNotifications = reader.GetInt32(0);
            }

            //if there are notifications of deletion to other users, do not delete the event
            if (countOfSoftDeletedNotifications > 0)
            {
                connection.Close();
                return;
            }

            //otherwise hard delete the event
            sqlQuery = "DELETE FROM Event WHERE Id = @id;";
            command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@id", eventId);
            command.ExecuteNonQuery();

            connection.Close();

        }
    }
}
