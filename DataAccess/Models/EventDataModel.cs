using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DataAccess.Models
{
    public class EventDataModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartingTime { get; set; }
        public DateTime EndingTime { get; set; }
        public string EventCreatorName { get; set; }
        public bool AlertStatus { get; set; }
        public List<UserDataModel> UsersThatParticipateInTheEvent { get; set; } = new List<UserDataModel>();
        public List<CommentDataModel> EventComments { get; set; } = new List<CommentDataModel>();
        
        //default constructor
        public EventDataModel() {}

        public EventDataModel(int id, string title, string description, DateTime startingTime, DateTime endingTime, string eventCreator)
        {
            Id = id;
            Title = title;
            Description = description;
            StartingTime = startingTime;
            EndingTime = endingTime;
            EventCreatorName = eventCreator;
        }

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
    }
}
