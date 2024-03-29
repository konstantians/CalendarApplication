﻿using DataAccess.Models;
using SoftwareTechnologyCalendarApplicationMVC.CustomValidators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SoftwareTechnologyCalendarApplication.Models
{
    public class Event
    {
        public int Id { get; set; }
        [CustomRequired]
        public string Title { get; set; }
        
        public string Description { get; set; }
        [CheckMinutes]
        [CheckStartBeforeEndTime]
        public DateTime StartingTime { get; set; }
        [CheckMinutes]
        [CheckStartBeforeEndTime]
        public DateTime EndingTime { get; set; }
        public bool AlertStatus { get; set; }
        public string EventCreatorName { get; set; }
        public List<User> UsersThatParticipateInTheEvent { get; set; } = new List<User>();
        public List<Notification> Notifications { get; set; } = new List<Notification>();
        public List<Comment> EventComments { get; set; } = new List<Comment>();

        public Event(){}

        public Event(EventDataModel eventDataModel){
            Id = eventDataModel.Id;
            Title = eventDataModel.Title;
            Description = eventDataModel.Description;
            StartingTime = eventDataModel.StartingTime;
            EndingTime = eventDataModel.EndingTime;
            AlertStatus = eventDataModel.AlertStatus;
            EventCreatorName = eventDataModel.EventCreatorName;
            foreach (UserDataModel userDataModel in eventDataModel.UsersThatParticipateInTheEvent)
            {
                User tempUser = new User();
                tempUser.Username = userDataModel.Username;
                tempUser.Password = userDataModel.Password;
                tempUser.Fullname = userDataModel.Fullname;
                tempUser.Email = userDataModel.Email;
                tempUser.Phone = userDataModel.Phone;

                UsersThatParticipateInTheEvent.Add(tempUser);
            }
            foreach (CommentDataModel commentDataModel in eventDataModel.EventComments)
            {
                EventComments.Add(new Comment(commentDataModel));
            }
        }
    }
}
