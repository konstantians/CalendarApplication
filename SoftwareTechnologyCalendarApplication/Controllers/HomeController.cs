using DataAccess.Logic;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SoftwareTechnologyCalendarApplication.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace SoftwareTechnologyCalendarApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserDataAccess UserDataAccess;
        private readonly ICalendarDataAccess CalendarDataAccess;
        private readonly IEventDataAccess EventDataAccess;

        public HomeController(ILogger<HomeController> logger, IUserDataAccess userDataAccess, 
            ICalendarDataAccess calendarDataAccess, IEventDataAccess eventDataAccess)
        {
            UserDataAccess = userDataAccess;
            CalendarDataAccess = calendarDataAccess;
            EventDataAccess = eventDataAccess;
            _logger = logger;
        }

        public IActionResult AddCalendar()
        {
            AuthorizeUser();

            ViewData["DuplicateCalendarTitle"] = false;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCalendar(Models.Calendar calendar)
        {
            AuthorizeUser();

            ViewData["DuplicateCalendarTitle"] = false;
            if (!ModelState.IsValid)
            {
                return View();
            }
            UserDataModel user = UserDataAccess.GetUser(ActiveUser.User.Username);
            foreach(CalendarDataModel calendarDataModelTemp in user.Calendars)
            {
                if (calendarDataModelTemp.Title == calendar.Title)
                {
                    ViewData["DuplicateCalendarTitle"] = true;
                    return View();
                };
            }

            CalendarDataModel calendarDataModel = new CalendarDataModel();
            calendarDataModel.Title = calendar.Title;
            //add the categories the user wrote in the textarea
            if(calendar.Categories.First() != null)
            {
                calendarDataModel.Categories = calendar.Categories;
            }

            //add the categories that the user checked in the checkbox area
            IEnumerable<string> selectedCategories = Request.Form["SelectedCategories"];
            foreach (string category in selectedCategories)
            {
                calendarDataModel.Categories.Add(category);
            }

            CalendarDataAccess.CreateCalendar(calendarDataModel, user.Username);
            return RedirectToAction("HomePage", "Home", new {pagination = 1 });
        }

        public IActionResult HomePage(int pagination, bool calendarWasDeleted)
        {
            AuthorizeUser();

            UserDataModel userDataModel = UserDataAccess.GetUser(ActiveUser.User.Username);
            User user = new User(userDataModel);
            ViewData["DeletedCalendar"] = calendarWasDeleted;
            ViewData["pagination"] = pagination * 6;
            return View(user);
        }

        public IActionResult DeleteCalendar(int calendarId)
        {
            AuthorizeUser();

            CalendarDataAccess.DeleteCalendar(calendarId);
            return RedirectToAction("HomePage", "Home", new { pagination = 1 ,
                calendarWasDeleted = true});
        }

        [HttpPost]
        public IActionResult DeleteEvent(int calendarId,int eventId,int year,int month, int day)
        {
            AuthorizeUser();

            EventDataAccess.DeleteEvent(eventId, ActiveUser.User.Username);
            return RedirectToAction("ViewCalendarDay", "Home", new
            {
                username = ActiveUser.User.Username,
                calendarId = calendarId,
                year = year,
                month = month,
                day = day,
                eventWasDeleted = true
            });
        }

        public IActionResult ViewCalendar(int calendarId, int month, int year)
        {
            AuthorizeUser();

            int monthIndex = month == 0? DateTime.Today.Month : month;
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

            //get the offset (numerical represantation of the day- for example Monday = 0, Tuesday = 1 ...)
            //adjusted by one, because it starts as Sunday = 0 and We are going with monday = 0;
            int offset = new DateTime(yearIndex, monthIndex, 1).DayOfWeek != 0 ? (int)new DateTime(yearIndex, monthIndex, 1).DayOfWeek - 1 : 6;

            CalendarDataModel calendarDataModel = CalendarDataAccess.GetCalendar(calendarId);
            Models.Calendar calendar = new Models.Calendar(calendarDataModel);
            ViewData["MonthLength"] = monthlength;
            ViewData["Offset"] = offset;
            ViewData["MonthName"] = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(monthIndex);
            ViewData["Month"] = monthIndex;
            ViewData["Year"] = yearIndex; 
            return View(calendar);
        }

        public IActionResult ViewCalendarDay(int calendarId, int month, int year, int day, bool eventWasDeleted)
        {
            AuthorizeUser();

            CalendarDataModel calendarDataModel = CalendarDataAccess.GetCalendar(calendarId);
            Models.Calendar calendar = new Models.Calendar(calendarDataModel);
            ViewData["Month"] = month;
            ViewData["Year"] = year;
            ViewData["Day"] = day;
            ViewData["EventWasDeleted"] = eventWasDeleted;
            return View(calendar);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult editEvent(int calendarId,int eventId, int year, int month, int day)
        {
            AuthorizeUser();

            ViewData["DuplicateEventTitle"] = false;
            ViewData["Editing"] = true;
            ViewData["CalendarId"] = calendarId;
            ViewData["EventId"] = eventId;

            ViewData["Year"] = year;
            ViewData["Month"] = month;
            ViewData["Day"] = day;
            EventDataModel eventDataModelTemp = EventDataAccess.GetEvent(eventId, ActiveUser.User.Username);
            Event eventt = new Event();
            eventt.Id=eventDataModelTemp.Id;
            eventt.Description = eventDataModelTemp.Description;
            eventt.Title = eventDataModelTemp.Title;
            eventt.StartingTime = eventDataModelTemp.StartingTime;
            eventt.EndingTime = eventDataModelTemp.EndingTime;
            eventt.AlertStatus = eventDataModelTemp.AlertStatus;
            return View(eventt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult editEvent(int calendarId,int eventId, Event eventt, int year, int month, int day)
        {
            AuthorizeUser();

            if (!ModelState.IsValid)
            {
                return View();
            }
            EventDataModel eventDataModel = new EventDataModel();
            eventDataModel.Id = eventId;
            eventDataModel.Title = eventt.Title;
            eventDataModel.Description = eventt.Description;
            eventDataModel.StartingTime = eventt.StartingTime;
            eventDataModel.EndingTime = eventt.EndingTime;
            eventDataModel.AlertStatus = eventt.AlertStatus;
            EventDataAccess.UpdateEvent(eventDataModel,ActiveUser.User.Username);
            return RedirectToAction("ViewCalendarDay", "Home", new
            {   calendarId = calendarId, year = year,
                month = month, day = day});
        }

        public IActionResult addEvent(int calendarId, int year, int month, int day)
        {
            AuthorizeUser();

            ViewData["DuplicateEventTitle"] = false;
            ViewData["CalendarId"] = calendarId;
            ViewData["Editing"] = false;
            ViewData["Year"] = year;
            ViewData["Month"] = month;
            ViewData["Day"] = day;
            //DateTime dt = DateTime.Now;
            DateTime dateTime = new DateTime(year, month, day);//,dt.Hour,dt.Minute,dt.Second);
            //DateTime.ParseExact()
            Event eventt = new Event();
            eventt.EndingTime = dateTime;
            eventt.StartingTime = dateTime;
            return View(eventt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult addEvent(int calendarId, Event eventt, int year, int month, int day)
        {
            AuthorizeUser();

            ViewData["DuplicateEventTitle"] = false;
            if (!ModelState.IsValid)
            {
                return View();
            }

            List < EventDataModel > eventList = EventDataAccess.GetEvents(calendarId);
            foreach (EventDataModel eventDataModelTemp in eventList)
            {
                if(eventDataModelTemp.Title == eventt.Title)
                {
                    ViewData["DuplicateEventTitle"] = true;
                    return View();
                }
            }
            EventDataModel eventDataModel = new EventDataModel();
            eventDataModel.Id = eventt.Id;
            eventDataModel.Title = eventt.Title;
            eventDataModel.Description = eventt.Description;
            eventDataModel.StartingTime = eventt.StartingTime;
            eventDataModel.EndingTime = eventt.EndingTime;
            eventDataModel.AlertStatus = eventt.AlertStatus;

            EventDataAccess.CreateEvent(eventDataModel, ActiveUser.User.Username, calendarId);
            return RedirectToAction("ViewCalendarDay", "Home", new {calendarId = calendarId ,
            year = year, month = month, day = day});
        }
        
        public IActionResult ViewNotifications(bool NoCalendarSelected)
        {
            AuthorizeUser();

            ActiveUser.HasNotifications = false;
            User user = new User(UserDataAccess.GetUser(ActiveUser.User.Username));

            ViewData["NoCalendarSelected"] = NoCalendarSelected ? true : false;
            return View(user);
        }

        [HttpPost]
        public IActionResult DeleteNotification(int eventId, DateTime notificationTime)
        {
            AuthorizeUser();

            EventDataAccess.DeleteNotification(eventId, ActiveUser.User.Username, notificationTime);
            return RedirectToAction("ViewNotifications", "Home");
        }

        [HttpPost]
        public IActionResult AcceptInvitation(int eventId, DateTime notificationTime, int calendarId/*, bool alertStatus*/)
        {
            AuthorizeUser();

            //This must be a bug with MVC. alertStatus exists on Request.Form["alertStatus"], but can not be binded from the 
            // actions parameters. 
            bool alertStatus = Request.Form["alertStatus"] == "on" ? true : false; 
            
            //if the user does not have a calendar return an error
            if(calendarId == 0)
            {
                return RedirectToAction("ViewNotifications", "Home", new { NoCalendarSelected = true});
            }
            
            EventDataAccess.AcceptInvitation(eventId, ActiveUser.User.Username, notificationTime, calendarId, alertStatus);
            return RedirectToAction("ViewNotifications", "Home");
        }

        [HttpPost]
        public IActionResult RejectInvitation(int eventId, DateTime notificationTime)
        {
            AuthorizeUser();

            EventDataAccess.RejectInvitation(eventId, ActiveUser.User.Username, notificationTime);
            return RedirectToAction("ViewNotifications", "Home");
        }

        private static void AuthorizeUser()
        {
            if (ActiveUser.User == null)
            {
                throw new NotImplementedException();
            }
        }
        public IActionResult editAccount()
        {
            ViewData["DuplicateEventTitle"] = false;
            UserDataModel userDataModelTemp = UserDataAccess.GetUser(ActiveUser.User.Username);
            User userr = new User(userDataModelTemp);
            //eventt.Id = eventDataModelTemp.Id;
            //eventt.Description = eventDataModelTemp.Description;
            //eventt.Title = eventDataModelTemp.Title;
            //eventt.StartingTime = eventDataModelTemp.StartingTime;
            //eventt.EndingTime = eventDataModelTemp.EndingTime;
            //eventt.AlertStatus = eventDataModelTemp.AlertStatus;
            //UserName = username;
            return View(userr);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult editAccount(User userr)
        {
            //if (UserName == "")
            //{
            //    return View();
            //}
            ViewData["DuplicateEventTitle"] = false;
            if (!ModelState.IsValid)
            {
                return View();
            }

            List<UserDataModel> userList = UserDataAccess.GetUsers();
            foreach (UserDataModel userDataModelTemp in userList)
            {
                if (userDataModelTemp.Username == userr.Username && userDataModelTemp.Username!= ActiveUser.User.Username)//UserName)
                {
                    ViewData["DuplicateUsername"] = true;
                    //Prepei edo na valo UserName=""; ?
                    return View();
                }
                //if (userDataModelTemp.Password == userr.Password && userDataModelTemp.Username !=username)// UserName)
                //{
                //    ViewData["DuplicateEventTitle"] = true;
                //    ViewData["User"] = username;
                //    return View();
                //}
                //if (userDataModelTemp.Fullname == userr.Fullname && userDataModelTemp.Username != username)// UserName)
                //{
                //    ViewData["DuplicateEventTitle"] = true;
                //    ViewData["User"] = username;
                //    return View();
                //}
                if (userDataModelTemp.Email == userr.Email && userDataModelTemp.Username != ActiveUser.User.Username)// UserName)
                {
                    ViewData["DuplicateEmail"] = true;
                    return View();
                }
                //if (userDataModelTemp.Phone == userr.Phone && userDataModelTemp.Username != username)// UserName)
                //{
                //    ViewData["DuplicateEventTitle"] = true;
                //    ViewData["User"] = username;
                //    return View();
                //}

            }
            UserDataModel userDataModel = new UserDataModel();
            userDataModel.Username = userr.Username;
            userDataModel.Password = userr.Password;
            userDataModel.Phone = userr.Phone;
            userDataModel.Email = userr.Email;
            userDataModel.Fullname = userr.Fullname;
            UserDataAccess.UpdateUser(userDataModel);
            //UserDataAccess.UpdateUserAndUsername(userDataModel, username);// UserName);
            return RedirectToAction("HomePage", "Home", new { pagination = 1 });
        }
    }
}
