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
        public string ImagePath { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
        public List<Event> Events { get; set; } = new List<Event>();

        public Calendar(){}

        public Calendar(CalendarDataModel calendarDataModel){
            Id = calendarDataModel.Id;
            Title = calendarDataModel.Title;
            ImagePath = calendarDataModel.ImagePath;
            Categories = calendarDataModel.Categories;

            foreach (EventDataModel calendarEvent in calendarDataModel.Events)
            {
                Events.Add(new Event(calendarEvent));
            }
        }
    }
}
