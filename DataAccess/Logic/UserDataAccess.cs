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

        public UserDataAccess(IConfiguration configuration)
        {
            Configuration = configuration;
            connection = new SQLiteConnection(Configuration.GetConnectionString("Default"));

        }
        

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

                users.Add(user);
            }
            connection.Close();

            return users;
        }

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

            }
            connection.Close();

            return user;
        }

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

        public void DeleteUser(string username)
        {
            connection.Open();
            //TODO when the user is deleted make sure to delete the events he participates in
            //And The calendars he has

            string sqlQuery = "Delete from User where Username = @username";
            SQLiteCommand command = new SQLiteCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@username", username);
            command.ExecuteNonQuery();
            connection.Close();
        }

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
