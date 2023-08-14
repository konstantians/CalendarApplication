using DataAccess.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Numerics;
using System.Text;

namespace DataAccess.Logic
{
    /// <summary>
    /// handles the dataAccess for the user
    /// </summary>
    public class UserDataAccess : IUserDataAccess
    {

        private readonly IConfiguration Configuration;
        private readonly SQLiteConnection connection;
        private readonly ICalendarDataAccess CalendarDataAccess;
        private readonly IEventDataAccess EventDataAccess;

        /// <summary>
        /// constuctor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="calendarDataAccess"></param>
        /// <param name="eventDataAccess"></param>
        public UserDataAccess(IConfiguration configuration, ICalendarDataAccess calendarDataAccess, IEventDataAccess eventDataAccess)
        {
            Configuration = configuration;
            connection = new SQLiteConnection(Configuration.GetConnectionString("Default"));
            CalendarDataAccess = calendarDataAccess;
            EventDataAccess = eventDataAccess;

        }

        /// <summary>
        /// This method returns all the users from the database and also all their calendars populated(with their events filled)
        /// and their events populated. Additionally it returns the comments of the users and it also returns all the events they either have created or participate in.
        /// </summary>
        /// <param name="getInactiveUsers">this parameter if set will make the method also return the inactive users</param>
        /// <returns>All the active users and also the inactive users if specified</returns>
        public List<UserDataModel> GetUsers(bool getInactiveUsers)
        {
            connection.Open();
            string sqlQuery = getInactiveUsers ? "SELECT * FROM User;" : "SELECT * FROM User WHERE IsActive = 1;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

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
                user.Calendars = ReturnCalendarsOfUser(user.Username);
                user.EventsThatTheUserParticipates = ReturnEventsOfUser(user.Username);
                user.Notifications = GetNotificationsOfUser(user.Username);
                user.Comments = GetCommentsOfUser(user.Username);

                users.Add(user);
            }
            connection.Close();

            return users;
        }

        /// <summary>
        /// This method returns one user using the given username from the database and also all their calendars populated(with their events filled)
        /// and their events populated. Additionally it returns the comments of the user and it also returns all the events they either have created or participate in.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <returns>The specified user</returns>
        public UserDataModel GetUser(string username)
        {
            connection.Open();
            string sqlQuery = "SELECT * FROM User WHERE Username = @username AND IsActive = 1;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@username", username);
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
                user.Calendars = ReturnCalendarsOfUser(username);
                user.EventsThatTheUserParticipates = ReturnEventsOfUser(username);
                user.Notifications = GetNotificationsOfUser(user.Username);
                user.Comments = GetCommentsOfUser(user.Username);

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
            string sqlQuery = "SELECT Id FROM Calendar JOIN User ON " +
                "Calendar.UserUsername = User.Username WHERE Username = @username;";
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
        /// The events created, might be created by the user, foreign accepted and foreign non-accepted(yet).
        /// Use this method to do the initial check on whether or not to show notifications for invitations.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>The events that the user is associated with</returns>
        private List<EventDataModel> ReturnEventsOfUser(string username)
        {
            string sqlQuery = "SELECT Event.Id FROM Event " +
                              "JOIN ParticipationInEvent ON ParticipationInEvent.EventId = Event.Id " +
                              "JOIN User ON ParticipationInEvent.UserUsername = User.Username " +
                              "WHERE User.Username = @username;";

            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@username", username);
            SQLiteDataReader reader = command.ExecuteReader();

            List<EventDataModel> events = new List<EventDataModel>();

            while (reader.Read())
            {
                EventDataModel calendarEvent = EventDataAccess.GetEvent(reader.GetInt32(0), username);
                events.Add(calendarEvent);
            }

            return events;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public List<NotificationDataModel> GetNotificationsOfUser(string username)
        {
            string sqlQuery = "SELECT EventId, UserSender, NotificationTime, InvitationPending, EventAccepted, EventRejected, " +
                "EventChanged, CommentAdded, CommentDeleted, EventDeleted, AlertNotification, HasBeenSeen " +
                "FROM Notification WHERE Notification.UserReceiver = @userUsername;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@userUsername", username);
            SQLiteDataReader reader = command.ExecuteReader();

            List<NotificationDataModel> notificationDataModels = new List<NotificationDataModel>();
            while (reader.Read())
            {
                NotificationDataModel notificationDataModel = new NotificationDataModel();

                notificationDataModel.EventOfNotification = EventDataAccess.GetEventForNotifications(reader.GetInt32(0));
                notificationDataModel.SenderUser = GetUserBasicInformation(reader.GetString(1));
                notificationDataModel.NotificationTime = DateTime.Parse(reader.GetString(2));
                notificationDataModel.InvitationPending = Convert.ToBoolean(reader.GetInt32(3));
                notificationDataModel.EventAccepted = Convert.ToBoolean(reader.GetInt32(4));
                notificationDataModel.EventRejected = Convert.ToBoolean(reader.GetInt32(5));
                notificationDataModel.EventChanged = Convert.ToBoolean(reader.GetInt32(6));
                notificationDataModel.CommentAdded = Convert.ToBoolean(reader.GetInt32(7));
                notificationDataModel.CommentDeleted = Convert.ToBoolean(reader.GetInt32(8));
                notificationDataModel.EventDeleted = Convert.ToBoolean(reader.GetInt32(9));
                notificationDataModel.AlertNotification = Convert.ToBoolean(reader.GetInt32(10));
                notificationDataModel.HasBeenSeen = Convert.ToBoolean(reader.GetInt32(11));

                notificationDataModels.Add(notificationDataModel);
            }
            return notificationDataModels;
        }

        List<CommentDataModel> GetCommentsOfUser(string username)
        {
            string sqlQuery = "SELECT EventId, UserUsername, CommentText, CommentDate FROM Comment WHERE Comment.UserUsername = @userUsername;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@userUsername", username);
            SQLiteDataReader reader = command.ExecuteReader();

            List<CommentDataModel> commentDataModels = new List<CommentDataModel>();
            while (reader.Read())
            {
                CommentDataModel commentDataModel = new CommentDataModel();

                commentDataModel.EventOfComment = EventDataAccess.GetEvent(reader.GetInt32(0));
                commentDataModel.UserWhoMadeTheComment = GetUserBasicInformation(username);
                commentDataModel.CommentText = reader.GetString(2);
                commentDataModel.CommentDate = DateTime.Parse(reader.GetString(3));

                commentDataModels.Add(commentDataModel);
            }
            return commentDataModels;
        }



        /// <summary>
        /// This method creates a user. It must be mentioned that this method does not create calendars or events.
        /// In case you want to create those you can by using the createCalendar in the calendarDataAccess class
        /// or the createEvent in the eventDataAccess. The connection with the user there is automatic by providing
        /// the user's username.
        /// </summary>
        /// <param name="user">The user model</param>
        public void CreateUser(UserDataModel user)
        {
            // check for other users with the same username
            if (GetUser(user.Username).Username != null)
            {
                throw new Exception("There is another user with the same username");
            }

            connection.Open();
            string sqlQuery = "INSERT INTO User (Username, Password, FullName, DateOfBirth, Email, Phone, IsActive) " +
                              "VALUES (@username, @password, @fullName, @dateOfBirth, @email, @phone, 0);";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@password", user.Password);
            command.Parameters.AddWithValue("@fullName", user.Fullname);
            command.Parameters.AddWithValue("@dateOfBirth", user.DateOfBirth);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@phone", user.Phone);
            command.ExecuteNonQuery();
            connection.Close();
        }

        /// <summary>
        /// This method updates the user. It must be mentioned that it does not update the calendar or the events
        /// of the user aand it only updates the static information of the user. 
        /// If you need to update those you will have to call the UpdateCalendar method 
        /// from the CalendarDataAccess class and the UpdateEvent method from the EventDataAccess class.  
        /// </summary>
        /// <param name="user">The user's name</param>
        public void UpdateUser(UserDataModel user)
        {
            connection.Open();
            string sqlQuery = "UPDATE User SET Password = @password, FullName = @fullname, DateOfBirth = @dateOfBirth, Email = @email, Phone = @phone " +
                "WHERE Username = @username;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@password", user.Password);
            command.Parameters.AddWithValue("@fullName", user.Fullname);
            command.Parameters.AddWithValue("@dateOfBirth", user.DateOfBirth.ToString());
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
            //if the user did not update the username then we can simply use the update user method
            if(user.Username == oldUsername)
            {
                UpdateUser(user);
                return;
            }

            //to avoid unique constraint
            string email = user.Email;
            user.Email = "";

            //first create a new user with the given username
            CreateUser(user);

            //then update all information in the database to point to the new user and activate the new user in the database
            connection.Open();
            string sqlQuery = "UPDATE Comment SET UserUsername = @username WHERE UserUsername = @oldUsername;" +
                "UPDATE Event SET EventCreator = @username WHERE EventCreator = @oldUsername;" +
                "UPDATE Token SET UserUsername = @username WHERE UserUsername = @oldUsername;" +
                "UPDATE ParticipationInEvent SET UserUsername = @username WHERE UserUsername = @oldUsername;" +
                "UPDATE Notification SET UserSender = @username WHERE UserSender = @oldUsername;" +
                "UPDATE Notification SET UserReceiver = @username WHERE UserReceiver = @oldUsername;" +
                "UPDATE Calendar SET UserUsername = @username WHERE UserUsername = @oldUsername;" +
                "UPDATE User SET IsActive = 1 WHERE Username = @username";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@oldUsername", oldUsername);
            command.ExecuteNonQuery();

            connection.Close();

            //finally delete the old user
            DeleteUser(oldUsername);

            //fix the email
            user.Email = email;
            UpdateUser(user);
        }

        /// <summary>
        /// This method deletes the user and and all their calendars, comments and events. It also removes any indirect connections
        /// That the user has with any event. Finally it sends relevant notifications to other users who either participated in an event
        /// that now is deleted, because the user does not exist any more and also notifications to those who have/participate in an event that
        /// the deleted user participated in.
        /// </summary>
        /// <param name="username">The user's name</param>
        public void DeleteUser(string username)
        {
            //get the user and delete all the calendars(this will also delete all the events and it will take care
            //of the ParticipationInEvent table for these events)
            UserDataModel user = GetUser(username);
            foreach (CalendarDataModel calendar in user.Calendars)
            {
                CalendarDataAccess.DeleteCalendar(calendar.Id, user.Username);
            }

            connection.Open();

            //now remove the user entries from the participationInEvent table
            //delete the user itself
            string sqlQuery = "DELETE FROM ParticipationInEvent WHERE UserUsername = @username;" +
                              "DELETE FROM Token WHERE UserUsername = @username;" +
                              "DELETE FROM User WHERE Username = @username;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@username", username);
            command.ExecuteNonQuery();
            connection.Close();
        }

        private UserDataModel GetUserBasicInformation(string username)
        {
            string sqlQuery = "SELECT Username, Password, FullName, DateOfBirth, Email, Phone FROM User WHERE User.Username = @username;";

            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@username", username);

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


        ////////////////////////////////// Tokens code //////////////////////////////////


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<TokenDataModel> GetTokens()
        {
            connection.Open();
            string sqlQuery = "SELECT * FROM Token;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
            SQLiteDataReader reader = command.ExecuteReader();

            List<TokenDataModel> tokens = new List<TokenDataModel>();

            while (reader.Read())
            {
                TokenDataModel token = new TokenDataModel();
                token.Token = reader.GetString(0);
                token.TokenExpiration = DateTime.Parse(reader.GetString(1));
                token.UserOfToken = reader.GetString(2);

                tokens.Add(token);
            }
            connection.Close();

            return tokens;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool TokenExists(string username, string token)
        {
            connection.Open();
            string sqlQuery = "SELECT * FROM Token WHERE Token = @token AND UserUsername = @username;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@token", token);

            SQLiteDataReader reader = command.ExecuteReader();

            TokenDataModel tokenDataModel = new TokenDataModel();

            while (reader.Read())
            {
                tokenDataModel.Token = reader.GetString(0);
                tokenDataModel.TokenExpiration = DateTime.Parse(reader.GetString(1));
                tokenDataModel.UserOfToken = reader.GetString(2);
                tokenDataModel.IsActivationToken = Convert.ToBoolean(reader.GetInt32(3));
                tokenDataModel.IsResetPasswordToken = Convert.ToBoolean(reader.GetInt32(4));
                tokenDataModel.IsSessionToken = Convert.ToBoolean(reader.GetInt32(5));
            }

            connection.Close();

            //check if there is a token
            return tokenDataModel.Token != null;
        }

        public void ActivateUser(string username, string token)
        {
            connection.Open();
            string sqlQuery = "UPDATE User SET IsActive = 1 WHERE Username = @username;";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@username", username);
            command.ExecuteNonQuery();
            connection.Close();

            DeleteToken(token);
        }

        public void CreateAccountActivationToken(string token, string username)
        {
            CreateToken(token, username, true, false, false);
        }

        public void CreateResetPasswordToken(string token, string username)
        {
            CreateToken(token, username, false, true, false);
        }

        public void CreateSessionToken(string token, string username)
        {
            CreateToken(token, username, false, false, true);
        }

        private void CreateToken(string token, string username, bool isActivationToken, bool isResetPasswordToken, bool isSessionToken)
        {
            connection.Open();
            string sqlQuery = "INSERT INTO Token (Token, ExpirationDate, UserUsername, IsActivationToken, IsResetPasswordToken, IsSessionToken) " +
                              "VALUES (@token, @expirationDate, @userUsername, @isActivationToken, @isResetPasswordToken, @isSessionToken);";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);

            command.Parameters.AddWithValue("@token", token);
            if (!isSessionToken)
            {
                command.Parameters.AddWithValue("@expirationDate", DateTime.Now.AddHours(3).ToString());
            }
            else
            {
                command.Parameters.AddWithValue("@expirationDate", DateTime.MaxValue.ToString());
            }
            command.Parameters.AddWithValue("@userUsername", username);
            command.Parameters.AddWithValue("@isActivationToken", Convert.ToInt32(isActivationToken));
            command.Parameters.AddWithValue("@isResetPasswordToken", Convert.ToInt32(isResetPasswordToken));
            command.Parameters.AddWithValue("@isSessionToken", Convert.ToInt32(isSessionToken));
            command.ExecuteNonQuery();
            connection.Close();
        }

        public void DeleteToken(string token)
        {
            connection.Open();
            string sqlQuery = "DELETE FROM Token WHERE token = @token;";

            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@token", token);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public void DeleteAccountActivationTokenAndUser(string token, string username)
        {

            connection.Open();
            string sqlQuery = "DELETE FROM Token WHERE token = @token;" +
                "DELETE FROM User WHERE username = @username;";

            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@token", token);
            command.Parameters.AddWithValue("@username", username);
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}
