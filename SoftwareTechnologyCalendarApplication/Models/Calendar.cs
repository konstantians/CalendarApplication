using System;
using System.Collections.Generic;
using System.Text;

namespace SoftwareTechnologyCalendarApplication.Models
{
    public class Calendar
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
        public List<Event> Events { get; set; } = new List<Event>();
    }
}
