using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DataAccess.Models
{
    /// <summary>
    /// The Event model that is used by the dataAccess.
    /// </summary>
    public class EventDataModel
    {
        /// <summary>
        /// The id of the event, which is used to identify it.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The title of the event.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// The description of the event.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The date and time that the event starts.
        /// </summary>
        public DateTime StartingTime { get; set; }
        /// <summary>
        /// The date and time that the event ends.
        /// </summary>
        public DateTime EndingTime { get; set; }
        /// <summary>
        /// The username of the creator of the event
        /// </summary>
        public string EventCreatorName { get; set; }
        /// <summary>
        /// The status that checks on whether or not a user should get a notification
        /// if the event is about to begin.
        /// </summary>
        public bool AlertStatus { get; set; }
        /// <summary>
        /// The list that contains all the users that participate in the event. Most of the time it will contain
        /// only their static information tho.
        /// </summary>
        public List<UserDataModel> UsersThatParticipateInTheEvent { get; set; } = new List<UserDataModel>();
        /// <summary>
        /// The list that contains the events that are made by the various users of the event.
        /// </summary>
        public List<CommentDataModel> EventComments { get; set; } = new List<CommentDataModel>();
        
        /// <summary>
        /// default constructor
        /// </summary>
        public EventDataModel() {}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="startingTime"></param>
        /// <param name="endingTime"></param>
        /// <param name="eventCreator"></param>
        public EventDataModel(string title, string description, DateTime startingTime, DateTime endingTime, string eventCreator)
        {
            Title = title;
            Description = description;
            StartingTime = startingTime;
            EndingTime = endingTime;
            EventCreatorName = eventCreator;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="startingTime"></param>
        /// <param name="endingTime"></param>
        /// <param name="eventCreator"></param>
        public EventDataModel(int id, string title, string description, DateTime startingTime, DateTime endingTime, string eventCreator)
        {
            Id = id;
            Title = title;
            Description = description;
            StartingTime = startingTime;
            EndingTime = endingTime;
            EventCreatorName = eventCreator;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="startingTime"></param>
        /// <param name="endingTime"></param>
        /// <param name="eventCreator"></param>
        /// <param name="alertStatus"></param>
        public EventDataModel(int id, string title, string description, DateTime startingTime, DateTime endingTime, string eventCreator, bool alertStatus)
        {
            Id = id;
            Title = title;  
            Description = description;
            StartingTime = startingTime;
            EndingTime = endingTime;
            EventCreatorName = eventCreator;
            AlertStatus = alertStatus;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="startingTime"></param>
        /// <param name="endingTime"></param>
        /// <param name="eventCreator"></param>
        /// <param name="alertStatus"></param>
        public EventDataModel(string title, string description, DateTime startingTime, DateTime endingTime, string eventCreator, bool alertStatus)
        {
            Title = title;
            Description = description;
            StartingTime = startingTime;
            EndingTime = endingTime;
            EventCreatorName = eventCreator;
            AlertStatus = alertStatus;
        }
    }
}
