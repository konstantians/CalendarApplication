using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public class CalendarDataModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
        public List<EventDataModel> Events { get; set; } = new List<EventDataModel>();
    }
}
