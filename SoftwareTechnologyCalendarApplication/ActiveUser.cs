using SoftwareTechnologyCalendarApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoftwareTechnologyCalendarApplication
{
    public static class ActiveUser
    {
        public static User User { get; set; } = null;
        public static bool HasNotifications { get; set;} = false;
        /// <summary>
        /// used to avoid having a user access an error page or a confirmation page that they should not.
        /// </summary>
        public static bool AccessConfirmationPage { get; set; } = false;

        /// <summary>
        /// check if there is a user logged in and if not throw an exception(for now)
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public static void AuthorizeUser()
        {
            if (User == null)
            {
                throw new NotImplementedException();
            }
        }

        public static void CheckAccessConfirmationPage()
        {
            if (!AccessConfirmationPage)
            {
                throw new NotImplementedException();
            }
        }
    }
}
