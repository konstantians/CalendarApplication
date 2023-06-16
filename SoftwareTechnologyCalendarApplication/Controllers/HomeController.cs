using DataAccess.Logic;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SoftwareTechnologyCalendarApplication.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Calendar = SoftwareTechnologyCalendarApplication.Models.Calendar;

namespace SoftwareTechnologyCalendarApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserDataAccess UserDataAccess;
        private readonly ICalendarDataAccess CalendarDataAccess;
        private readonly IEventDataAccess EventDataAccess;

        public HomeController(ILogger<HomeController> logger, IUserDataAccess userDataAccess, ICalendarDataAccess calendarDataAccess, IEventDataAccess eventDataAccess)
        {
            UserDataAccess = userDataAccess;
            CalendarDataAccess = calendarDataAccess;
            EventDataAccess = eventDataAccess;
            _logger = logger;
        }

        public IActionResult AddCalendar(string username)
        {
            ViewData["DuplicateCalendarTitle"] = false;
            ViewData["User"] = username;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCalendar(string username,Calendar calendar)
        {
            ViewData["DuplicateCalendarTitle"] = false;
            if (!ModelState.IsValid)
            {
                return View();
            }
            UserDataModel user = UserDataAccess.GetUser(username);
            foreach(CalendarDataModel calendarDataModelTemp in user.Calendars)
            {
                if (calendarDataModelTemp.Title == calendar.Title)
                {
                    ViewData["DuplicateCalendarTitle"] = true;
                    ViewData["User"] = username;
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

            CalendarDataAccess.CreateCalendar(calendarDataModel, username);
            return RedirectToAction("HomePage", "Home", new { username = username, pagination = 1 });
        }

        public IActionResult HomePage(string username, int pagination, bool calendarWasDeleted)
        {
            UserDataModel userDataModel = UserDataAccess.GetUser(username);
            User user = new User(userDataModel);
            ViewData["DeletedCalendar"] = calendarWasDeleted;
            ViewData["pagination"] = pagination * 6;
            ViewData["User"] = username;
            return View(user);
        }

        public IActionResult DeleteCalendar(string username,int calendarId)
        {
            CalendarDataAccess.DeleteCalendar(calendarId);
            return RedirectToAction("HomePage", "Home", new { username = username, pagination = 1 ,
                calendarWasDeleted = true});
        }

        public IActionResult LogOut()
        {
            return RedirectToAction("Login", "Authentication");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult editEvent( string username, int eventId)
        {
            ViewData["DuplicateEventTitle"] = false;
            ViewData["User"] = username;
            ViewData["Editing"] = true;
            ViewData["EventId"] = eventId;
            EventDataModel eventDataModelTemp = EventDataAccess.GetEvent(eventId);
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
        public IActionResult editEvent(string username, int eventId, Event eventt)
        {
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
            EventDataAccess.UpdateEvent(eventDataModel);
            return RedirectToAction("HomePage", "Home", new { username = username, pagination = 1 });
        }

        public IActionResult addEvent(string username, int calendarId)
        {
            ViewData["DuplicateEventTitle"] = false;
            ViewData["User"] = username;
            ViewData["CalendarId"] = calendarId;
            ViewData["Editing"] = false;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult addEvent(string username, int calendarId, Event eventt)
        {
            //username = ModelState.GetValueOrDefault("username").ToString();
            //calendarId = (int)ModelState.GetValueOrDefault("calendarId").ToString();
            //username = ModelState["username"].RawValue.ToString();
            //string calendarId2 = ModelState["calendarId"].RawValue.ToString();
            //calendarId = Convert.ToInt32(calendarId2);

            //ModelState.Remove("username");
            //ModelState.Remove("calendarId");
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
                    ViewData["User"] = username;
                    return View();
                }
            }
            //username = Request("username").toString();
            EventDataModel eventDataModel = new EventDataModel();
            eventDataModel.Id = eventt.Id;
            eventDataModel.Title = eventt.Title;
            eventDataModel.Description = eventt.Description;
            eventDataModel.StartingTime = eventt.StartingTime;
            eventDataModel.EndingTime = eventt.EndingTime;
            eventDataModel.AlertStatus = eventt.AlertStatus;

            EventDataAccess.CreateEvent(eventDataModel, username, calendarId);
            return RedirectToAction("HomePage", "Home", new { username = username, pagination = 1 });
        }
    }
}
