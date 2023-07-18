using DataAccess.Models;
using SoftwareTechnologyCalendarApplication.Models;
using System;

namespace SoftwareTechnologyCalendarApplication.Models
{
    public class Comment
    {
        public Event EventOfComment { get; set; }
        public User UserWhoMadeTheComment { get; set; }
        public string CommentText { get; set; }
        public DateTime CommentDate { get; set; }

        //default constructor
        public Comment(){}

        public Comment(Event eventOfComment, User userWhoMadeTheComment, string commentText, DateTime commentDate)
        {
            EventOfComment = eventOfComment;
            UserWhoMadeTheComment = userWhoMadeTheComment;
            CommentText = commentText;
            CommentDate = commentDate;
        }

        public Comment(CommentDataModel commentDataModel)
        {
            EventOfComment = new Event(commentDataModel.EventOfComment);
            UserWhoMadeTheComment = new User(commentDataModel.UserWhoMadeTheComment);
            CommentText = commentDataModel.CommentText;
            CommentDate = commentDataModel.CommentDate;
        }
    }
}
