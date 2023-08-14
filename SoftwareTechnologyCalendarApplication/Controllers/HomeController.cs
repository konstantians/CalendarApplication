using DataAccess.Logic;
using DataAccess.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.AccountSessionServices;
using SoftwareTechnologyCalendarApplication.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SoftwareTechnologyCalendarApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserDataAccess UserDataAccess;
        private readonly ICalendarDataAccess CalendarDataAccess;
        private readonly IEventDataAccess EventDataAccess;
        private readonly IActiveUsers _activeUsers;
        private readonly IAccountSessionManager _accountSessionManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, IUserDataAccess userDataAccess,
            ICalendarDataAccess calendarDataAccess, IEventDataAccess eventDataAccess, 
            IActiveUsers activeUsers, IAccountSessionManager accountSessionManager, IWebHostEnvironment webHostEnvironment)
        {
            UserDataAccess = userDataAccess;
            CalendarDataAccess = calendarDataAccess;
            EventDataAccess = eventDataAccess;
            _logger = logger;
            _activeUsers = activeUsers;
            _webHostEnvironment = webHostEnvironment;
            _accountSessionManager = accountSessionManager;
        }

        public IActionResult AddCalendar()
        {
            if (!AuthenticateAndCheckNotifications())
                return RedirectToAction("Login", "Authentication");

            ViewData["DuplicateCalendarTitle"] = false;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCalendar(Models.Calendar calendar)
        {
            if (!AuthenticateAndCheckNotifications())
                return RedirectToAction("Login", "Authentication");

            ViewData["DuplicateCalendarTitle"] = false;
            if (!ModelState.IsValid)
            {
                return View();
            }
            UserDataModel user = UserDataAccess.GetUser(_activeUsers.User.Username);
            foreach (CalendarDataModel calendarDataModelTemp in user.Calendars)
            {
                if (calendarDataModelTemp.Title == calendar.Title)
                {
                    ViewData["DuplicateCalendarTitle"] = true;
                    return View();
                };
            }

            CalendarDataModel calendarDataModel = new CalendarDataModel();
            calendarDataModel.Title = calendar.Title;
            //get the categories that are in the first place of the categories(they are all stored there)
            //and split them into different categories based on the | separator.
            if(calendar.Categories.First() == "")
            {
                calendarDataModel.Categories = calendar.Categories.First().Split("|").ToList();
            }


            //add the categories that the user checked in the checkbox area
            IEnumerable<string> selectedCategories = Request.Form["SelectedCategories"];
            foreach (string category in selectedCategories)
            {
                calendarDataModel.Categories.Add(category);
            }

            CalendarDataAccess.CreateCalendar(calendarDataModel, user.Username);
            return RedirectToAction("HomePage", "Home", new { pagination = 1 });
        }

        //TODO big bug. The calendar pagination is recked.
        //corrected pagination is probably important so that the events will not overflow
        //but not like that.
        //Idea prepei na kano correction se ola kai na to janakano opos prin
        public IActionResult HomePage(int pagination, bool calendarWasDeleted, string paginationMode)
        {
            if (!AuthenticateAndCheckNotifications())
                return RedirectToAction("Login", "Authentication");
            User user = _activeUsers.User;

            //if no pagination was given make it the default
            pagination = pagination == 0 ? 1 : pagination;

            //if it came from desktop there is no need for correction
            if (paginationMode == "desktop" || paginationMode == null || paginationMode == "")
            {
                ViewData["pagination"] = pagination * 6;

                //yes this math is tough
                //there is a pattern where if there is a decimal point then you just need to floor the value
                //otherwise you need to -1
                decimal x = ((decimal)pagination * 6 / 4);
                if (x != Math.Floor(x))
                    ViewData["paginationTablet"] = (int)(Math.Floor(x)) * 4;
                else
                    ViewData["paginationTablet"] = (x - 1) * 4; 

                ViewData["paginationMobile"] = ((pagination * 6 / 2) - 2) * 2;
            }
            //happened because the pagination was triggered on tablet
            else if (paginationMode == "tablet")
            {
                //yes this math is tough
                //there is a pattern where it turns out that if it is divisible by 3*pagination - 2 you need to ceiling the expression
                //there is a pattern where it turns out that if it is divisible by 3*pagination - 1 you need to floor the expression
                //there is a pattern where it turns out that if it is divisible by 3*pagination you do not need to do anything
                if((pagination % 3 == 1))
                {
                    ViewData["pagination"] = ((int)Math.Ceiling(((decimal)pagination * 4) / 6)) * 6;
                }
                else if(pagination % 3 == 2)
                {
                    ViewData["pagination"] =  ((int)Math.Floor(((decimal)pagination * 4) / 6)) * 6;
                }
                else
                {
                    ViewData["pagination"] = ((pagination * 4) / 6) * 6;
                }


                ViewData["paginationTablet"] = pagination * 4;
                ViewData["paginationMobile"] = ((pagination * 4 / 2) - 1) * 2;
            }
            //happened because the pagination was triggered on mobile
            else if (paginationMode == "mobile")
            {
                ViewData["pagination"] = ((int)Math.Ceiling(((decimal)pagination * 2) / 6)) * 6;
                ViewData["paginationTablet"] = ((int)Math.Ceiling(((decimal)pagination * 2) / 4)) * 4;
                ViewData["paginationMobile"] = pagination * 2;
            }
            
            ViewData["DeletedCalendar"] = calendarWasDeleted;
            ViewData["webHostPath"] = _webHostEnvironment.WebRootPath;
            return View(user);
        }

        public IActionResult DeleteCalendar(int calendarId)
        {
            if (!_activeUsers.CheckIfLoggedIn())
                return RedirectToAction("Login", "Authentication");
            _activeUsers.InstantiateUser();

            CalendarDataAccess.DeleteCalendar(calendarId, _activeUsers.User.Username);
            return RedirectToAction("HomePage", "Home", new { pagination = 1,
                calendarWasDeleted = true });
        }

        [HttpPost]
        public IActionResult DeleteEvent(int calendarId, int eventId, int year, int month, int day)
        {
            if (!_activeUsers.CheckIfLoggedIn())
                return RedirectToAction("Login", "Authentication");
            _activeUsers.InstantiateUser();

            EventDataAccess.DeleteEvent(eventId, _activeUsers.User.Username);
            return RedirectToAction("ViewCalendarDay", "Home", new
            {
                username = _activeUsers.User,
                calendarId = calendarId,
                year = year,
                month = month,
                day = day,
                eventWasDeleted = true
            });
        }

        public IActionResult ViewCalendar(int calendarId, int month, int year)
        {
            if (!AuthenticateAndCheckNotifications())
                return RedirectToAction("Login", "Authentication");

            int monthIndex = month == 0 ? DateTime.Today.Month : month;
            int yearIndex = year == 0 ? DateTime.Today.Year : year;

            int monthlength;
            if (monthIndex == 1 || monthIndex == 3 ||
                monthIndex == 5 || monthIndex == 7 ||
                monthIndex == 8 || monthIndex == 10 ||
                monthIndex == 12)
            {
                monthlength = 31;
            }
            else if (monthIndex == 4 || monthIndex == 6 ||
                monthIndex == 9 || monthIndex == 11)
            {
                monthlength = 30;
            }
            else if (yearIndex % 4 != 0 && monthIndex == 2)
            {
                monthlength = 28;
            }
            else
            {
                monthlength = 29;
            }

            CalendarDataModel calendarDataModel = CalendarDataAccess.GetCalendar(calendarId);

            List<Tuple<int,int>> daysWithEvents = new List<Tuple<int,int>>();
            foreach (EventDataModel calendarEvent in calendarDataModel.Events)
            {
                //if the event of the calendar is of the given month
                if(calendarEvent.StartingTime.Month == monthIndex)
                {
                    int numberOfTheDay = new Tuple<int, int>(calendarEvent.StartingTime.Day, 1).Item1;

                    // Check if a tuple with the same day already exists in daysWithEvents
                    Tuple<int, int> existingTuple = daysWithEvents.FirstOrDefault(t => t.Item1 == numberOfTheDay);

                    //If yes...
                    if (existingTuple != null)
                    {
                        // Increase the second value of the existing tuple and remove the previous tupple
                        daysWithEvents.Remove(existingTuple);
                        daysWithEvents.Add(new Tuple<int, int>(existingTuple.Item1, existingTuple.Item2 + 1));
                    }
                    //otherwise
                    else
                    {
                        daysWithEvents.Add(new Tuple<int, int>(calendarEvent.StartingTime.Day, 1));
                    }
                }
            }

            //get the offset (numerical represantation of the day- for example Monday = 0, Tuesday = 1 ...)
            //adjusted by one, because it starts at Sunday = 0 and we are going with monday = 0;
            int offset = new DateTime(yearIndex, monthIndex, 1).DayOfWeek != 0 ? (int)new DateTime(yearIndex, monthIndex, 1).DayOfWeek - 1 : 6;

            Models.Calendar calendar = new Models.Calendar(calendarDataModel);
            ViewData["MonthLength"] = monthlength;
            ViewData["Offset"] = offset;
            ViewData["MonthName"] = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(monthIndex);
            ViewData["Month"] = monthIndex;
            ViewData["Year"] = yearIndex;
            ViewData["DaysWithEvents"] = daysWithEvents;
            return View(calendar);
        }

        public IActionResult ViewCalendarDay(int calendarId, int month, int year, int day, bool eventWasDeleted)
        {
            if (!AuthenticateAndCheckNotifications())
                return RedirectToAction("Login", "Authentication");

            CalendarDataModel calendarDataModel = CalendarDataAccess.GetCalendar(calendarId);
            Models.Calendar calendar = new Models.Calendar(calendarDataModel);
            ViewData["Month"] = month;
            ViewData["Year"] = year;
            ViewData["Day"] = day;
            ViewData["EventWasDeleted"] = eventWasDeleted;
            return View(calendar);
        }

        public IActionResult editEvent(int calendarId, int eventId, int year, int month, int day, int forein)
        {
            if (!AuthenticateAndCheckNotifications())
                return RedirectToAction("Login", "Authentication");

            Event eventt = new Event(EventDataAccess.GetEvent(eventId, _activeUsers.User.Username));

            List<string> usernames = GetPeopleWhoHaveAlreadyBeenInvited(eventt);

            ViewData["DuplicateEventTitle"] = false;
            ViewData["CalendarId"] = calendarId;
            ViewData["EventId"] = eventId;
            ViewData["forein"] = forein;
            ViewData["Year"] = year;
            ViewData["Month"] = month;
            ViewData["Day"] = day;
            ViewData["usernames"] = usernames;

            return View(eventt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult editEvent(int calendarId, int eventId, Event eventt, int year, int month, int day, string[] invitations, string newComments)
        {
            if (!AuthenticateAndCheckNotifications())
                return RedirectToAction("Login", "Authentication");

            if (!ModelState.IsValid)
            {
                eventt = new Event(EventDataAccess.GetEvent(eventId, eventt.EventCreatorName));
                ViewData["DuplicateEventTitle"] = false;
                ViewData["CalendarId"] = calendarId;
                ViewData["EventId"] = eventId;
                ViewData["forein"] = eventt.EventCreatorName == _activeUsers.User.Username ? 0 : 1;
                ViewData["Year"] = year;
                ViewData["Month"] = month;
                ViewData["Day"] = day;
                ViewData["usernames"] = GetPeopleWhoHaveAlreadyBeenInvited(eventt);
                return View(eventt);
            }

            if (invitations != null)
            {
                foreach (string memberInvitation in invitations)
                {
                    EventDataAccess.InviteUsersToEvent(eventId,eventt.EventCreatorName,invitations.ToList());
                }
            }

            List<string> newCommentsText = new List<string>();
            if (newComments != null)
            {
                foreach (string newComment in newComments.Split("|"))
                {
                    newCommentsText.Add(newComment);
                }
            }

            if(newCommentsText.Count != 0)
            {
                EventDataAccess.CreateComments(newCommentsText, eventId, _activeUsers.User.Username);
                return RedirectToAction("ViewCalendarDay", "Home", new
                {
                    calendarId = calendarId,
                    year = year,
                    month = month,
                    day = day
                });
            }

            EventDataModel eventDataModel = new EventDataModel();
            eventDataModel.Id = eventId;
            eventDataModel.Title = eventt.Title;
            eventDataModel.Description = eventt.Description;
            eventDataModel.StartingTime = eventt.StartingTime;
            eventDataModel.EndingTime = eventt.EndingTime;
            eventDataModel.AlertStatus = eventt.AlertStatus;
            EventDataAccess.UpdateEvent(eventDataModel, _activeUsers.User.Username);
            return RedirectToAction("ViewCalendarDay", "Home", new
            { calendarId = calendarId, year = year,
                month = month, day = day });
        }

        private List<string> GetPeopleWhoHaveAlreadyBeenInvited(Event eventt)
        {
            //this is kinda messy, but essentionally it figures out which members have already been invited or already participate. 
            List<string> alreadyParticipate = new List<string>();
            foreach (User user in eventt.UsersThatParticipateInTheEvent)
            {
                alreadyParticipate.Add(user.Username);
            }
            List<string> usernames = new List<string>();
            foreach (UserDataModel userr in UserDataAccess.GetUsers(false))
            {

                if (userr.Username != _activeUsers.User.Username && !alreadyParticipate.Contains(userr.Username))
                {
                    bool goNext = false;
                    foreach (NotificationDataModel notificationDataModel in userr.Notifications)
                    {
                        if (notificationDataModel.EventOfNotification.Id == eventt.Id && notificationDataModel.InvitationPending)
                        {
                            goNext = true;
                            break;
                        }
                    }
                    if (goNext)
                    {
                        continue;
                    }
                    usernames.Add(userr.Username);
                }
            }

            return usernames;
        }

        public IActionResult addEvent(int calendarId, int year, int month, int day)
        {
            if (!AuthenticateAndCheckNotifications())
                return RedirectToAction("Login", "Authentication");

            ViewData["DuplicateEventTitle"] = false;
            ViewData["CalendarId"] = calendarId;
            ViewData["Year"] = year;
            ViewData["Month"] = month;
            ViewData["Day"] = day;
            DateTime dateTime = new DateTime(year, month, day);

            Event eventt = new Event();
            eventt.EndingTime = dateTime;
            eventt.StartingTime = dateTime;
            
            ViewData["usernames"] = getUsernamesOfAllUsers();
            return View(eventt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult addEvent(int calendarId, Event eventt, int year, int month, int day, string[] invitations, string newComments)
        {
            if (!AuthenticateAndCheckNotifications())
                return RedirectToAction("Login", "Authentication");

            if (!ModelState.IsValid)
            {
                ViewData["DuplicateEventTitle"] = false;
                ViewData["usernames"] = getUsernamesOfAllUsers();
                ViewData["CalendarId"] = calendarId;
                ViewData["Year"] = year;
                ViewData["Month"] = month;
                ViewData["Day"] = day;
                return View();
            }

            List<EventDataModel> eventList = EventDataAccess.GetEvents(calendarId);
            foreach (EventDataModel eventDataModelTemp in eventList)
            {
                //check if another event with the given title exists
                if (eventDataModelTemp.Title == eventt.Title)
                {
                    ViewData["DuplicateEventTitle"] = true;
                    ViewData["usernames"] = getUsernamesOfAllUsers();
                    ViewData["CalendarId"] = calendarId;
                    ViewData["Year"] = year;
                    ViewData["Month"] = month;
                    ViewData["Day"] = day;
                    return View();
                }
            }

            //get the comments
            List<CommentDataModel> newCommentsText = new List<CommentDataModel>();
            if (newComments != null)
            {
                foreach (string newComment in newComments.Split("|"))
                {
                    CommentDataModel commentDataModel = new CommentDataModel();
                    commentDataModel.CommentText = newComment;
                    newCommentsText.Add(commentDataModel);
                }
            }

            EventDataModel eventDataModel = new EventDataModel(eventt.Title, eventt.Description,
                eventt.StartingTime, eventt.EndingTime, _activeUsers.User.Username ,eventt.AlertStatus);
            //add the comments to the eventDataModel, so the will be created
            eventDataModel.EventComments = newCommentsText;

            int eventId = EventDataAccess.CreateEvent(eventDataModel, _activeUsers.User.Username, calendarId);
            EventDataAccess.InviteUsersToEvent(eventId, _activeUsers.User.Username, invitations.ToList());
            return RedirectToAction("ViewCalendarDay", "Home", new { calendarId = calendarId,
                year = year, month = month, day = day });
        }

        private List<string> getUsernamesOfAllUsers()
        {
            List<string> usernmes = new List<string>();
            foreach (UserDataModel userr in UserDataAccess.GetUsers(false))
            {
                if (userr.Username != _activeUsers.User.Username)
                {
                    usernmes.Add(userr.Username);
                }
            }
            return usernmes;
        }
        
        public IActionResult ViewNotifications(bool NoCalendarSelected)
        {
            if (!AuthenticateAndCheckNotifications())
                return RedirectToAction("Login", "Authentication");
            //this line only exists so the viewNotificationsIcons change faster
            _activeUsers.HasNotifications = false;

            EventDataAccess.UpdateSeenStatusOfNotifications(_activeUsers.User.Username);

            ViewData["NoCalendarSelected"] = NoCalendarSelected ? true : false;
            return View(_activeUsers.User);
        }

        [HttpPost]
        public IActionResult DeleteNotification(int eventId, string userSenderUsername, DateTime notificationTime)
        {
            if (!_activeUsers.CheckIfLoggedIn())
                return RedirectToAction("Login", "Authentication");
            _activeUsers.InstantiateUser();

            EventDataAccess.DeleteNotification(eventId, _activeUsers.User.Username, userSenderUsername, notificationTime);
            return RedirectToAction("ViewNotifications", "Home");
        }

        [HttpPost]
        public IActionResult AcceptInvitation(int eventId, string userSenderUsername, DateTime notificationTime, int calendarId/*, bool alertStatus*/)
        {
            if (!_activeUsers.CheckIfLoggedIn())
                return RedirectToAction("Login", "Authentication");
            _activeUsers.InstantiateUser();

            //This must be a bug with MVC. alertStatus exists on Request.Form["alertStatus"], but can not be binded from the 
            // actions parameters. 
            bool alertStatus = Request.Form["alertStatus"] == "on" ? true : false; 
            
            //if the user does not have a calendar return an error
            if(calendarId == 0)
            {
                return RedirectToAction("ViewNotifications", "Home", new { NoCalendarSelected = true});
            }
            
            EventDataAccess.AcceptInvitation(eventId, _activeUsers.User.Username, userSenderUsername, notificationTime, calendarId, alertStatus);
            return RedirectToAction("ViewNotifications", "Home");
        }

        [HttpPost]
        public IActionResult RejectInvitation(int eventId, string userSenderUsername, DateTime notificationTime)
        {
            if (!_activeUsers.CheckIfLoggedIn())
                return RedirectToAction("Login", "Authentication");
            _activeUsers.InstantiateUser();

            EventDataAccess.RejectInvitation(eventId, _activeUsers.User.Username, userSenderUsername, notificationTime);
            return RedirectToAction("ViewNotifications", "Home");
        }

        public IActionResult editAccount()
        {
            if (!AuthenticateAndCheckNotifications())
                return RedirectToAction("Login", "Authentication");

            ViewData["DuplicateAccount"] = false;
            ViewData["DuplicateEmail"] = false;
            UserDataModel userDataModelTemp = UserDataAccess.GetUser(_activeUsers.User.Username);
            User userr = new User(userDataModelTemp);
            return View(userr);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult editAccount(User userr, string oldUsername)
        {
            if (!AuthenticateAndCheckNotifications())
                return RedirectToAction("Login", "Authentication");

            if (!ModelState.IsValid)
            {
                return View();
            }

            List<UserDataModel> userList = UserDataAccess.GetUsers(true);
            foreach (UserDataModel userDataModelTemp in userList)
            {
                //if the user changes their username to a username that another user uses
                if (userDataModelTemp.Username == userr.Username && userDataModelTemp.Username!= _activeUsers.User.Username)
                {
                    ViewData["DuplicateAccount"] = true;
                    ViewData["DuplicateEmail"] = false;
                    userr.Username = oldUsername;
                    return View(userr);
                }

                //if the user changes their email to an email that another user uses
                if (userDataModelTemp.Email == userr.Email && userDataModelTemp.Username != _activeUsers.User.Username)
                {
                    ViewData["DuplicateAccount"] = false;
                    ViewData["DuplicateEmail"] = true;
                    userr.Email = _activeUsers.User.Email;
                    return View(userr);
                }

            }
            UserDataModel userDataModel = new UserDataModel();
            userDataModel.Username = userr.Username;
            userDataModel.Password = userr.Password;
            userDataModel.Fullname = userr.Fullname;
            userDataModel.DateOfBirth = userr.DateOfBirth;
            userDataModel.Phone = userr.Phone;
            userDataModel.Email = userr.Email;
            //delete session tokens of user
            foreach (TokenDataModel token in UserDataAccess.GetTokens())
            {
                if (token.IsSessionToken)
                    UserDataAccess.DeleteToken(token.Token);
                _accountSessionManager.DeleteSessionCookie();
            }

            UserDataAccess.UpdateUserAndUsername(userDataModel, oldUsername);
            _accountSessionManager.CreateSession(userDataModel.Username);

            return RedirectToAction("HomePage", "Home", new { pagination = 1 });
        }

        [HttpPost]
        public IActionResult deleteComment(string userWhoMadeTheComment, int eventId, DateTime commentDate, string commentText, 
            int calendarId, int year, int month, int day)
        {
            if (!_activeUsers.CheckIfLoggedIn())
                return RedirectToAction("Login", "Authentication");
            _activeUsers.InstantiateUser();

            CommentDataModel commentDataModel = new CommentDataModel();
            commentDataModel.UserWhoMadeTheComment.Username = userWhoMadeTheComment;
            commentDataModel.EventOfComment.Id = eventId;
            commentDataModel.CommentDate = commentDate;
            commentDataModel.CommentText = commentText;
            EventDataAccess.DeleteComment(commentDataModel);

            bool forein = userWhoMadeTheComment != _activeUsers.User.Username;
            return RedirectToAction("editEvent","Home", new {calendarId = calendarId, eventId = eventId, 
            year = year, month = month, day = day, forein = forein});
        }

        [HttpPost]
        public IActionResult editComment(string userWhoMadeTheComment, int eventId, DateTime commentDate, string commentText,
            int calendarId, int year, int month, int day, string oldCommentText)
        {
            if (!_activeUsers.CheckIfLoggedIn())
                return RedirectToAction("Login", "Authentication");
            _activeUsers.InstantiateUser();

            CommentDataModel commentDataModel = new CommentDataModel();
            commentDataModel.UserWhoMadeTheComment.Username = userWhoMadeTheComment;
            commentDataModel.EventOfComment.Id = eventId;
            commentDataModel.CommentDate = commentDate;
            commentDataModel.CommentText = commentText;
            EventDataAccess.UpdateComment(commentDataModel, oldCommentText);

            bool forein = userWhoMadeTheComment != _activeUsers.User.Username;
            return RedirectToAction("editEvent", "Home", new
            {
                calendarId = calendarId,
                eventId = eventId,
                year = year,
                month = month,
                day = day,
                forein = forein
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveScreenshot(IFormFile screenshot, int calendarId)
        {
            if (!_activeUsers.CheckIfLoggedIn())
                return RedirectToAction("Login", "Authentication");

            if (screenshot != null)
            {
                // Construct the file path within wwwroot/images/screenshots
                Guid guid = Guid.NewGuid();
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "screenshots", $"table_screenshot_{guid}.png");

                //delete the previous
                string previousScreenshot = CalendarDataAccess.GetCalendar(calendarId).ImagePath;
                string previousScreenshotImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "screenshots", previousScreenshot);
                try
                {
                    if(previousScreenshot != null && previousScreenshot != "")
                    {
                        System.IO.File.Delete(previousScreenshotImagePath);
                    }

                    //update the calendar screenshot
                    CalendarDataAccess.UpdateCalendarScreenshot(calendarId, $"table_screenshot_{guid}.png");

                    // Save the image to the specified path
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await screenshot.CopyToAsync(stream);
                    }

                    return Ok();
                }
                catch(Exception exception){
                    Console.WriteLine(exception.StackTrace);
                }
            }

            return BadRequest();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private bool AuthenticateAndCheckNotifications()
        {
            if (!_activeUsers.CheckIfLoggedIn())
                return false;
            _activeUsers.InstantiateUser();
            _activeUsers.CheckForNotifications();
            return true;
        }
    }
}
