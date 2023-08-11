using SoftwareTechnologyCalendarApplication.Models;

namespace SoftwareTechnologyCalendarApplication
{
    public interface IActiveUsers
    {
        User User { get; set; }
        bool HasNotifications { get; set; }
        bool CheckIfLoggedIn();
        void InstantiateUser();
        void CheckForNotifications();
    }
}