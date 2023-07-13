using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public class CommentDataModel
    {
        public EventDataModel EventOfComment { get; set; } = new EventDataModel();
        public UserDataModel UserWhoMadeTheComment { get; set; } = new UserDataModel();
        public string CommentText { get; set; }
        public DateTime CommentDate { get; set; }
    }
}
