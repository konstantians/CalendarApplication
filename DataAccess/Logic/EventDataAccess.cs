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
    /// <summary>
    /// handles the dataAccess for the events, comments and notifications.
    /// </summary>
    public class EventDataAccess : IEventDataAccess
    {
        private readonly IConfiguration Configuration;
        private readonly SQLiteConnection connection;
        /// <summary>
        /// constructor used to instansiate the connection with the database.
        /// </summary>
        /// <param name="configuration"></param>
        public EventDataAccess(IConfiguration configuration)
        {
            Configuration = configuration;
            connection = new SQLiteConnection(Configuration.GetConnectionString("Default"));

        }

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
        public List<EventDataModel> GetEvents(int calendarId)
        {
            connection.Open();
            string sqlQuery = "SELECT Id, Title, Description, StartingTime, EndingTime, EventCreator FROM Event " +
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
                calendarEvent.Description = !reader.IsDBNull(2) ? reader.GetString(2) : "";
                calendarEvent.StartingTime = DateTime.Parse(reader.GetString(3));
                calendarEvent.EndingTime = DateTime.Parse(reader.GetString(4));
                calendarEvent.EventCreatorName = reader.GetString(5);
                calendarEvent.UsersThatParticipateInTheEvent = ReturnUsersOfEvent(calendarEvent.Id);
                calendarEvent.EventComments = ReturnCommentsOfEvent(calendarEvent.Id);

                events.Add(calendarEvent);
            }
            connection.Close();

            return events;
        }

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
                calendarEvent.Description = !reader.IsDBNull(2) ? reader.GetString(2) : "";
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
                calendarEvent.Description = !reader.IsDBNull(2) ? reader.GetString(2) : "";
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
        /// returns the events like GetEvent ignoring the soft delete.
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
                    calendarEvent.Description = !reader.IsDBNull(2) ? reader.GetString(2) : "";
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
        /// These users have only their basic information such as username, password etc filled and 
        /// do not have their calendar, comments or events filled properties filled. In case you want to also fill this information
        /// you can use the GetUser method in the UserDataAccess class using the name of the user you want.
        /// </summary>
        /// <param name="id">The event's id</param>
        /// <returns>The user's who are associated directly or indirectly with the specified event</returns>
        List<UserDataModel> ReturnUsersOfEvent(int id)
        {
            //filter the users that have this event
            string sqlQuery = "SELECT Username, Password, Fullname, DateOfBirth, Email, Phone FROM User " +
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
                user.DateOfBirth = DateTime.Parse(reader.GetString(3));
                user.Email = reader.GetString(4);
                user.Phone = reader.GetString(5);

                users.Add(user);
            }

            return users;
        }

        /// <summary>
        /// This is a helper method that is used by the GetEvents and GetEvent methods return all the comments
        /// of the specified event using the given id. It also fills the basic/static information of the EventOfComment
        /// and the UserWhoMadeTheComment property.
        /// </summary>
        /// <param name="id">the id of the event</param>
        /// <returns>the comments of the given event</returns>
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
                string description = !reader.IsDBNull(1) ? reader.GetString(1) : "";
                CommentDataModel comment = new CommentDataModel();
                comment.EventOfComment = new EventDataModel(id, reader.GetString(0), description,
                    DateTime.Parse(reader.GetString(2)), DateTime.Parse(reader.GetString(3)), reader.GetString(4));
                comment.UserWhoMadeTheComment = ReturnUserOfComment(reader.GetString(4));
                comment.CommentText = reader.GetString(5);
                comment.CommentDate = DateTime.Parse(reader.GetString(6));

                comments.Add(comment);
            }

            return comments;
        }

        /// <summary>
        /// This is a helper method that is used by the ReturnCommentsOfEvent to get 
        /// the basic information of a user using the given username.
        /// </summary>
        /// <param name="userUsername">the username of the user</param>
        /// <returns>the basic information of a user</returns>
        UserDataModel ReturnUserOfComment(string userUsername)
        {
            string sqlQuery = "SELECT Username, Password, FullName, DateOfBirth, Email, Phone From User WHERE User.Username = @username;";

            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@username", userUsername);

            SQLiteDataReader reader = command.ExecuteReader();

            UserDataModel user = new UserDataModel();

            while (reader.Read())
            {
                user.Username = reader.GetString(0);
                user.Password = reader.GetString(1);
                user.Fullname = reader.GetString(2);
                user.DateOfBirth = DateTime.Parse(reader.GetString(3));
                user.Email = reader.GetString(4);
                user.Phone = reader.GetString(5);
            }

            return user;
        }

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

        /// <summary>
        /// This method creates comments for an event using the comments list parameter. It also sends
        /// notifications to all the other users who participate for the given event that a comment has been
        /// created and the relavent information of that comment.
        /// </summary>
        /// <param name="comments">The texts of the created comments</param>
        /// <param name="eventId">The id of the event for which the comment was created</param>
        /// <param name="creatorOfComment">The name of the creator of the comment</param>
        public void CreateComments(List<string> comments, int eventId, string creatorOfComment)
        {
            string sqlQuery;
            SQLiteCommand command;

            connection.Open();
            int counter = 0;
            foreach (string comment in comments)
            {
                sqlQuery = "INSERT INTO Comment (EventId, UserUsername, CommentText, CommentDate) " +
                           "VALUES (@eventId, @userUsername, @commentText, @commentDate);";

                command = new SQLiteCommand(sqlQuery, connection);

                command.Parameters.AddWithValue("@eventId", eventId);
                command.Parameters.AddWithValue("@userUsername", creatorOfComment);
                command.Parameters.AddWithValue("@commentText", comment);
                command.Parameters.AddWithValue("@commentDate", DateTime.Now.AddSeconds(counter).ToString());
                counter++;
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

        /// <summary>
        /// This method updates an event using the information that exists on the updated information that exists 
        /// in commentDatamodel parameter. The oldCommentText parameter is necessary for finding out which comment 
        /// was changed.
        /// </summary>
        /// <param name="commentDataModel">The commentDataModel model that contains the updated information of the comment</param>
        /// <param name="oldCommentText">the old text of the comment</param>
        public void UpdateComment(CommentDataModel commentDataModel, string oldCommentText)
        {
            string sqlQuery;
            SQLiteCommand command;

            connection.Open();
            sqlQuery = "UPDATE Comment SET CommentText = @commentText WHERE EventId = @eventId " +
                "AND UserUsername = @userUsername AND CommentDate = @commentDate AND CommentText = @oldCommentText;";

            command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@eventId", commentDataModel.EventOfComment.Id);
            command.Parameters.AddWithValue("@userUsername", commentDataModel.UserWhoMadeTheComment.Username);
            command.Parameters.AddWithValue("@commentText", commentDataModel.CommentText);
            command.Parameters.AddWithValue("@commentDate", commentDataModel.CommentDate.ToString());
            command.Parameters.AddWithValue("@oldCommentText", oldCommentText);
            command.ExecuteNonQuery();

            connection.Close();

            //TODO possibly send notifications that a comment has been updated. Database does not support it yet
        }

        /// <summary>
        /// This method deletes a comment using the comments list parameter. It also sends
        /// notifications to all the other users who participate in that event, that the comment has been deleted 
        /// and the relavent information of that action(who deleted, when etc).
        /// </summary>
        /// <param name="commentDataModel">The information of the comment that is needed for locating the comment</param>
        public void DeleteComment(CommentDataModel commentDataModel)
        {
            connection.Open();
            string sqlQuery = "DELETE FROM Comment WHERE EventId = @eventId AND UserUsername = @userUsername AND CommentText = @commentText AND CommentDate = @commentDate;";

            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@eventId", commentDataModel.EventOfComment.Id);
            command.Parameters.AddWithValue("@userUsername", commentDataModel.UserWhoMadeTheComment.Username);
            command.Parameters.AddWithValue("@commentText", commentDataModel.CommentText);
            command.Parameters.AddWithValue("@commentDate", commentDataModel.CommentDate.ToString());
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

        /// <summary>
        /// This is the internal method that is used by all the other methods which create notifications. The reason for its existence
        /// is to keep the code DRY(do not repeat yourself). Additionally the parameter count makes sure that 2 notifications will never
        /// be created at the same time, by the same user for the same event.
        /// </summary>
        /// <param name="notificationDataModel">The model of the notification</param>
        /// <param name="receiversOfNotification">The usernames of the users who will receive this notification</param>
        /// <param name="count">Parameter that is Used when multiple notifications must be created at the same time, to avoid conflict</param>
        private void CreateNotifications(NotificationDataModel notificationDataModel, List<string> receiversOfNotification, int count = 0)
        {
            connection.Open();

            //create a notification for each one of them
            foreach (string memberOfEvent in receiversOfNotification)
            {

                string sqlQuery = "INSERT INTO Notification (EventId, UserUsername, NotificationTime, InvitationPending, EventAccepted, " +
                    "EventRejected, EventChanged, CommentAdded, CommentDeleted, EventDeleted, AlertNotification, HasBeenSeen) " +
                           "VALUES (@eventId, @userUsername, @notificationTime, @invitationPending ,@eventAccepted, " +
                           "@eventRejected, @eventChanged, @commentAdded, @commentDeleted, @eventDeleted, @alertNotification, 0);";
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

        /// <summary>
        /// This method is used to send invitations for the specified event to the specified users.
        /// </summary>
        /// <param name="eventId">The event's id</param>
        /// <param name="usernameOfCreator">The name of the person who created the event</param>
        /// <param name="usernamesOfInvitedUsers">The names of the people who are invited</param>
        public void InviteUsersToEvent(int eventId, string usernameOfCreator, List<string> usernamesOfInvitedUsers)
        {
            CreateNotifications(new NotificationDataModel(eventId, DateTime.Now, true, false, false, false, false, false, false, false), usernamesOfInvitedUsers);
        }

        /// <summary>
        /// This method is used to send an invitation for the specified event to the specified user.
        /// If you need to send multiple invations use the InviteUsersToEvent method instead.
        /// </summary>
        /// <param name="eventId">The event's id</param>
        /// <param name="usernameOfCreator">The name of the person who created the event</param>
        /// <param name="usernameOfInvitedUser">The name of the person who is invited</param>
        public void InviteUserToEvent(int eventId, string usernameOfCreator, string usernameOfInvitedUser)
        {
            CreateNotifications(new NotificationDataModel(eventId, DateTime.Now, true, false, false, false, false, false, false, false), new List<string> { usernameOfInvitedUser });
        }

        /// <summary>
        /// This method is used to accept an invitation that came from a different user for a specific event, 
        /// which is identified by the eventId parameter, and to make the indirect connection to the event for the invited user. 
        /// This method also deletes that invitation, so no further action is required. Finally it sends a notification to the 
        /// user who sent the invitation(creator of the event) that the invited user accepted their invitation.
        /// </summary>
        /// <param name="eventId">The id of the event</param>
        /// <param name="usernameOfInvitedUser">The username of the invited user who accepted the invitation</param>
        /// <param name="notificationTime">The date and time of the original invitation, which is used to delete the invitation</param>
        /// <param name="calendarId">The id of the calendar that contains the event</param>
        /// <param name="alertStatus">The alert status that the invited user chose(on or off)</param>
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

        /// <summary>
        /// This method is used to reject an invitation that came from a different user for a specific event, 
        /// which is identified by the eventId parameter. This method also deletes that invitation, so no further
        /// action is required. Finally it sends a notification to the user who sent the invitation(creator of the event)
        /// that the invited user rejected their invitation.
        /// </summary>
        /// <param name="eventId">The id of the event</param>
        /// <param name="usernameOfInvitedUser">The username of the invited user who rejected the invitation</param>
        /// <param name="notificationTime">The date and time of the original invitation, which is used to delete the invitation</param>
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
        /// Helper method that is used by the updateEvent method to update the event correctly, if the event was deemed to 
        /// be a native event. This method updates all necessary fields of the event using the information stored in the calendarEvent
        /// model and it also sends notification to every user who participates in the event that the event was updated.
        /// </summary>
        /// <param name="calendarEvent">The event's model</param>
        /// <param name="username">The username of the owner of the event</param>
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

        /// <summary>
        /// Helper method that is used by the updateEvent method to update the event correctly, if the event was deemed to 
        /// be a foreign event. This method updates only the fields of the event that are specific to the user such as alert status
        /// using the information stored in the calendarEvent model.        
        /// </summary>
        /// <param name="calendarEvent">The event's model</param>
        /// <param name="username">The username of the owner of the event</param>
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
        public void DeleteEvent(int eventId, string userWhoDeletedTheEvent)
        {
            if (GetEvent(eventId).EventCreatorName == userWhoDeletedTheEvent)
            {
                DeleteNativeEvent(eventId, userWhoDeletedTheEvent);
            }
            else
            {
                DeleteForeignEvent(eventId, userWhoDeletedTheEvent);
            }
        }

        /// <summary>
        /// This method is used by the DeleteEvent when it finds out that the event is a native one.
        /// This method first deletes every connection the event has except from the notification that have already
        /// been sent(it removes the invitation to the event, so noone can join a deleted event) and it soft deletes the
        /// event. Soft delete means that the event on its own remains on the database until all the notifications this method
        /// sends to each user who participated in the now deleted event have been aknowledged. Once all of them are the event
        /// gets deleted. If noone other than the creator of the event participated in that event then the event gets deleted
        /// immediatelly without going into the soft deleted mode.
        /// </summary>
        /// <param name="eventId">The event's id</param>
        /// <param name="userWhoDeletedTheEvent">The user who deleted the event</param>
        public void DeleteNativeEvent(int eventId, string userWhoDeletedTheEvent)
        {
            //find the users of the event
            connection.Open();
            string sqlQuery = "SELECT UserUsername FROM ParticipationInEvent " +
               "JOIN EVENT ON ParticipationInEvent.EventId = Event.Id " +
               "AND Event.EventCreator != ParticipationInEvent.UserUsername " +
               "WHERE EVENT.Id = @id;";

            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@id", eventId);

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

            command.Parameters.AddWithValue("@id", eventId);

            command.ExecuteNonQuery();

            connection.Close();

            //update for soft delete
            if (membersOfEvent.Count != 0)
            {
                connection.Open();
                sqlQuery = "UPDATE Event SET SoftDeleted = 1 WHERE id = @id;";
                command = new SQLiteCommand(sqlQuery, connection);

                command.Parameters.AddWithValue("@id", eventId);

                command.ExecuteNonQuery();
                connection.Close();

                //send notification that the event was deleted to everyone else(everyone who is not the creator)
                CreateNotifications(new NotificationDataModel(eventId, DateTime.Now, false, false, false, false, false, false, true, false), membersOfEvent);
            }
        }

        /// <summary>
        /// This method is used by the DeleteEvent when it finds out that the event is a foreign one. This method
        /// contrary to the DeleteNativeEvent, which is triggered when the event is a native one, is much simpler
        /// and it just removes the participation of the user from that event and the comments that user might have
        /// left on that event.
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

        /// <summary>
        /// This method is used to send an alert notification status for an event, which is identified by the eventId parameter.
        /// It should be used when an event is about to start and the user has the alert status for that event activated, but this
        /// method does not check for that, so make sure to check on the backend/frontend.
        /// </summary>
        /// <param name="eventId">The id of the event</param>
        /// <param name="username">The username of the user who will receive the alert notification status</param>
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

        /// <summary>
        /// This method is used to delete notifications and it also checks if the event that this notification was for 
        /// is soft deleted. If the event is soft deleted then it will check to see if there are other notifications for 
        /// that event and if no then it will hard delete the event. In any other case it will just stop.
        /// </summary>
        /// <param name="eventId">The id of the event</param>
        /// <param name="userUsername">The username of the user who the about to be deleted notification was sent to</param>
        /// <param name="notificationTime">The creation date and time of the about to be deleted notification</param>
        public void DeleteNotification(int eventId, string userUsername, DateTime notificationTime)
        {
            //delete the notification
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

            //otherwise find how many notifications exist for the soft deleted event
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

        /// <summary>
        /// This method should only be used when the system needs to understand that the
        /// user has aknowledged the notifications that have been sent to them.
        /// </summary>
        /// <param name="username">The username of the user who saw the notifications</param>
        public void UpdateSeenStatusOfNotifications(string username)
        {
            connection.Open();
            string sqlQuery = "UPDATE Notification SET HasBeenSeen = 1 WHERE UserUsername = @userUsername;";

            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@userUsername", username);
            command.ExecuteNonQuery();

            connection.Close();
        }
    }
}
