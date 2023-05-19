using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
        [Required(ErrorMessage = "You need to provide your email address")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [StringLength(10,MinimumLength =10,ErrorMessage ="This is too short or too long to be a real phone number")]
        [RegularExpression("^[0-9]+$", ErrorMessage ="Your phone number must only consist of numbers!")]
        [Required(ErrorMessage = "You need to provide your phone number")]
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
            Username = userDataModel.Username;
            Password = userDataModel.Password;
            Fullname = userDataModel.Fullname;
            Email = userDataModel.Email;
            Phone = userDataModel.Phone;

            foreach (EventDataModel calendarEvent in userDataModel.EventsThatTheUserParticipates)
            {
                Event tempCalendarEvent = new Event();
                tempCalendarEvent.Id = calendarEvent.Id;
                tempCalendarEvent.Title = calendarEvent.Title;
                tempCalendarEvent.AlertStatus = calendarEvent.AlertStatus;
                tempCalendarEvent.StartingTime = calendarEvent.StartingTime;
                tempCalendarEvent.EndingTime = calendarEvent.EndingTime;

                EventsThatTheUserParticipates.Add(tempCalendarEvent);
            }


            foreach (CalendarDataModel calendar in userDataModel.Calendars)
            {
                Calendar tempCalendar = new Calendar();
                tempCalendar.Id = calendar.Id;
                foreach(EventDataModel calendarEvent in calendar.Events) {
                    Event tempCalendarEvent = new Event();
                    tempCalendarEvent.Id = calendarEvent.Id;
                    tempCalendarEvent.Title = calendarEvent.Title;
                    tempCalendarEvent.AlertStatus = calendarEvent.AlertStatus;
                    tempCalendarEvent.StartingTime = calendarEvent.StartingTime;
                    tempCalendarEvent.EndingTime = calendarEvent.EndingTime;

                    tempCalendar.Events.Add(tempCalendarEvent);  
                }
                tempCalendar.Categories = calendar.Categories;
                tempCalendar.Title = calendar.Title;

                Calendars.Add(tempCalendar);
            }

        }
    }
}
