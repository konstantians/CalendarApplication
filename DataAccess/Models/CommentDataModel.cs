using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    /// <summary>
    /// The Comment model that is used by the dataAccess.
    /// </summary>
    public class CommentDataModel
    {
        /// <summary>
        /// The event that the comment is part of.
        /// </summary>
        public EventDataModel EventOfComment { get; set; } = new EventDataModel();
        /// <summary>
        /// The user who made that comment.
        /// </summary>
        public UserDataModel UserWhoMadeTheComment { get; set; } = new UserDataModel();
        /// <summary>
        /// The text of the comment
        /// </summary>
        public string CommentText { get; set; }
        /// <summary>
        /// The date which the comment was created.
        /// </summary>
        public DateTime CommentDate { get; set; }
    }
}
