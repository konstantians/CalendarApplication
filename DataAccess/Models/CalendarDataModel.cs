using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    /// <summary>
    /// The Calendar model that is used by the dataAccess.
    /// </summary>
    public class CalendarDataModel
    {
        /// <summary>
        /// The id of the calendar.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The title of the calendar.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// The list that contains the categories of the calendar.
        /// </summary>
        public List<string> Categories { get; set; } = new List<string>();
        /// <summary>
        /// The native and foreign events that exist in the calendar.
        /// </summary>
        public List<EventDataModel> Events { get; set; } = new List<EventDataModel>();
    }
}
