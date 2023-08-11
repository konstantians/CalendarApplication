using DataAccess.Logic;
using Services.AccountSessionServices;
using SoftwareTechnologyCalendarApplication.Models;
using System;
using System.Linq;

namespace SoftwareTechnologyCalendarApplication
{
    public class ActiveUsers : IActiveUsers
    {
        public User User { get; set; } = null;
        public bool HasNotifications { get; set; } = false;
        /// <summary>
        /// used to avoid having a user access an error page or a confirmation page that they should not.
        /// </summary>
        public bool AccessConfirmationPage { get; set; } = false;

        private readonly IAccountSessionManager _accountSessionManager;
        private readonly IUserDataAccess _userDataAccess;
        private readonly IEventDataAccess _eventDataAccess;
        public ActiveUsers(IAccountSessionManager accountSessionManager, IUserDataAccess userDataAccess, IEventDataAccess eventDataAccess)
        {
            _accountSessionManager = accountSessionManager;
            _userDataAccess = userDataAccess;
            _eventDataAccess = eventDataAccess;
        }

        public bool CheckIfLoggedIn()
        {
            //if the user has a cookie that means they are logged in so return true
            //otherwise return false
            return _accountSessionManager.ReadSessionCookie().Item1 != "";
        }

        public void InstantiateUser() 
        {
            User = new User(_userDataAccess.GetUser(_accountSessionManager.ReadSessionCookie().Item1));
        }

        public void CheckForNotifications()
        {
            foreach (Event calendarEvent in User.EventsThatTheUserParticipates)
            {
                if (calendarEvent.AlertStatus && (calendarEvent.StartingTime.AddHours(-1) < DateTime.Now && DateTime.Now < calendarEvent.EndingTime))
                {
                    _eventDataAccess.SendAlertNotification(calendarEvent.Id, User.Username);
                }
            }

            //get the new notifications of the user and those, which might have been created by the alert status
            if (User.Notifications.Where(notification => notification.HasBeenSeen == false).Count() != 0) {
                HasNotifications = true;
            }
        }
    }
}
