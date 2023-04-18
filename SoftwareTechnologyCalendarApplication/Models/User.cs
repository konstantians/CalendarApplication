using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftwareTechnologyCalendarApplication.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<Calendar> Calendars { get; set; } = new List<Calendar>();
        public List<Event> EventsThatTheUserParticipates { get; set; } = new List<Event>();

        //default constructor
        public User(){}

        //constructor
        public User(string username,string password,string fullname,string email,string phone){
            Username = username;
            Password = password;
            Fullname = fullname;
            Email = email;
            Phone = phone;
        }

        //copy constructor
        public User(User user)
        {
            Username = user.Username;
            Password = user.Password;
            Fullname = user.Fullname;
            Email = user.Email;
            Phone = user.Phone;
        }

        //copy constructor for data model
        public User(UserDataModel userDataModel)
        {

        }
    }
}
