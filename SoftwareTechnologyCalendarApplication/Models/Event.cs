using SoftwareTechnologyCalendarApplicationMVC.SpareClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftwareTechnologyCalendarApplication.Models
{
    public class Event
    {
        public int Id { get; set; }
        //[Required]
        public string Title { get; set; }
        
        public string Description { get; set; }
        //[RegularExpression(@"\d{2}[\/]\d{2}[\/]\d{4} \d{2}:(00)|(30)")]
        //[CheckMinutes]
        public DateTime StartingTime { get; set; }
        //[RegularExpression(@"\d{2}[\/]\d{2}[\/]\d{4} \d{2}:(00)|(30)")]
        //[CheckMinutes]
        public DateTime EndingTime { get; set; }
        public bool AlertStatus { get; set; }
        public List<User> UsersThatParticipateInTheEvent { get; set; } = new List<User>();
    }
}
