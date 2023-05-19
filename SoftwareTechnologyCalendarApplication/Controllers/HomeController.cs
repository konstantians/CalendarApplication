using DataAccess.Logic;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SoftwareTechnologyCalendarApplication.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SoftwareTechnologyCalendarApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserDataAccess UserDataAccess;
        private readonly ICalendarDataAccess CalendarDataAccess;

        public HomeController(ILogger<HomeController> logger, IUserDataAccess userDataAccess, ICalendarDataAccess calendarDataAccess)
        {
            UserDataAccess = userDataAccess;
            CalendarDataAccess = calendarDataAccess;
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
    }
}
