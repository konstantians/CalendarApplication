using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftwareTechnologyCalendarApplication.Models
{
    public class Calendar
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "You need to provide a title")]
        public string Title { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
        public List<Event> Events { get; set; } = new List<Event>();

        public Calendar(){}

        public Calendar(CalendarDataModel calendarDataModel){
            Id = calendarDataModel.Id;
            Title = calendarDataModel.Title;
            Categories = calendarDataModel.Categories;

            foreach (EventDataModel calendarEvent in calendarDataModel.Events)
            {
                Event tempEvent = new Event();
                tempEvent.Id = calendarEvent.Id;
                tempEvent.Title = calendarEvent.Title;
                tempEvent.Description = calendarEvent.Description;
                tempEvent.StartingTime = calendarEvent.StartingTime;
                tempEvent.EndingTime = calendarEvent.EndingTime;
                tempEvent.AlertStatus = calendarEvent.AlertStatus;
                foreach(UserDataModel userDataModel in calendarEvent.UsersThatParticipateInTheEvent)
                {
                    User tempUser = new User();
                    tempUser.Username = userDataModel.Username;
                    tempUser.Password = userDataModel.Password;
                    tempUser.Fullname = userDataModel.Fullname;
                    tempUser.Phone = userDataModel.Phone;
                    tempEvent.UsersThatParticipateInTheEvent.Add(tempUser);
                }
                Events.Add(tempEvent);
            }
        }
    }
}
