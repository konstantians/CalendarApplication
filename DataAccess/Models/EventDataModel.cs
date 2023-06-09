﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DataAccess.Models
{
    public class EventDataModel
    {
        //static information of the event
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartingTime { get; set; }
        public DateTime EndingTime { get; set; }
        public string EventCreatorName { get; set; }
        public List<UserDataModel> UsersThatParticipateInTheEvent { get; set; } = new List<UserDataModel>();
        public List<CommentDataModel> EventComments { get; set; } = new List<CommentDataModel>();

        //dynamic information of the event
        public bool AlertStatus { get; set; }

        //default constructor
        public EventDataModel() {}

        public EventDataModel(string title, string description, DateTime startingTime, DateTime endingTime, string eventCreator)
        {
            Title = title;
            Description = description;
            StartingTime = startingTime;
            EndingTime = endingTime;
            EventCreatorName = eventCreator;
        }

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

        public EventDataModel(int id, string title, string description, DateTime startingTime, DateTime endingTime, 
            string eventCreator, bool alertStatus, List<UserDataModel> usersThatParticipateInTheEvent, List<CommentDataModel> eventComments)
        {
            Id = id;
            Title = title;
            Description = description;
            StartingTime = startingTime;
            EndingTime = endingTime;
            EventCreatorName = eventCreator;
            AlertStatus = alertStatus;
            UsersThatParticipateInTheEvent = usersThatParticipateInTheEvent;
            EventComments = eventComments; 
        }
    }
}
