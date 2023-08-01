using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SoftwareTechnologyCalendarApplication.Models
{
    public class User
    {
        [Required(ErrorMessage ="You need to provide a Username")]
        public string Username { get; set; }
        
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "You need to provide a Password")]
        public string Password { get; set; }
        
        [Required(ErrorMessage = "You need to provide your Fullname")]
        [RegularExpression("^\\b(?!.*?\\s{2})[A-Za-z ]{1,50}\\b$", ErrorMessage ="Your Fullname must only consist of characters!")]
        public string Fullname { get; set; }

        [Display(Name = "Date Of Birth")]
        [Required(ErrorMessage = "You need to provide your birthday date")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "You need to provide your email address")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        
        [StringLength(10,MinimumLength =10,ErrorMessage ="This is too short or too long to be a real phone number")]
        [RegularExpression("^[0-9]+$", ErrorMessage ="Your phone number must only consist of numbers!")]
        [Required(ErrorMessage = "You need to provide your phone number")]
        
        public string Phone { get; set; }
        public List<Calendar> Calendars { get; set; } = new List<Calendar>();
        public List<Event> EventsThatTheUserParticipates { get; set; } = new List<Event>();
        public List<Notification> Notifications { get; set; } = new List<Notification>();

        //default constructor
        public User(){}

        //constructor
        public User(string username,string password,string fullname,DateTime dateOfBirth,string email,string phone){
            Username = username;
            Password = password;
            Fullname = fullname;
            DateOfBirth = dateOfBirth;
            Email = email;
            Phone = phone;
        }

        //copy constructor
        public User(User user)
        {
            Username = user.Username;
            Password = user.Password;
            Fullname = user.Fullname;
            DateOfBirth = user.DateOfBirth;
            Email = user.Email;
            Phone = user.Phone;
        }

        //copy constructor for data model
        public User(UserDataModel userDataModel)
        {
            Username = userDataModel.Username;
            Password = userDataModel.Password;
            Fullname = userDataModel.Fullname;
            DateOfBirth = userDataModel.DateOfBirth;
            Email = userDataModel.Email;
            Phone = userDataModel.Phone;

            foreach (EventDataModel calendarEvent in userDataModel.EventsThatTheUserParticipates)
            {
                EventsThatTheUserParticipates.Add(new Event(calendarEvent));
            }


            foreach (CalendarDataModel calendar in userDataModel.Calendars)
            {
                Calendars.Add(new Calendar(calendar));
            }

            foreach (NotificationDataModel notification in userDataModel.Notifications)
            {
                Notifications.Add(new Notification(notification));
            }
        }
    }
}
