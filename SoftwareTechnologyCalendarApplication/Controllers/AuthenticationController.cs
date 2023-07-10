using Microsoft.AspNetCore.Mvc;
using DataAccess.Logic;
using DataAccess.Models;
using System.Collections.Generic;
using System;
using SoftwareTechnologyCalendarApplication.Models;
using SoftwareTechnologyCalendarApplicationMVC.Models;
using System.Linq;
using System.Threading;

namespace SoftwareTechnologyCalendarApplication.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IUserDataAccess _userDataAccess;
        private readonly IEventDataAccess _eventDataAccess;
        public AuthenticationController(IUserDataAccess userDataAccess, IEventDataAccess eventDataAccess)
        {

            _userDataAccess = userDataAccess;
            _eventDataAccess = eventDataAccess;

        }
        public IActionResult Register()
        {
            if (ActiveUser.User != null) throw new NotImplementedException();
            ViewData["DuplicateAccount"] = false;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            if (ActiveUser.User != null) throw new NotImplementedException();

            if (!ModelState.IsValid)
            {
                return View();
            }

            List<UserDataModel> users = _userDataAccess.GetUsers();
            foreach (UserDataModel userDataModel in users)
            {
                if(userDataModel.Username == user.Username)
                {
                    ViewData["DuplicateAccount"] = true;
                    return View();
                }
            }

            UserDataModel userr = new UserDataModel();
            userr.Username = user.Username;
            userr.Password = user.Password;
            userr.Email = user.Email;
            userr.Phone = user.Phone;
            userr.Fullname = user.Fullname;
            _userDataAccess.CreateUser(userr);
            //authenticates the user
            ActiveUser.User = new User(userr); 
            return RedirectToAction("HomePage", "Home", new {pagination = 1 });
        }

        public IActionResult Login()
        {
            if (ActiveUser.User != null) throw new NotImplementedException();

            ViewData["WrongUsernamePassword"] = false;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(UserLogin user)
        {
            if (ActiveUser.User != null) throw new NotImplementedException();

            ViewData["WrongUsernamePassword"] = false;
            if(!ModelState.IsValid) {
                return View(user);
            }
            UserDataModel userDataModel = _userDataAccess.GetUser(user.Username);
            if ((userDataModel == null) || (userDataModel.Password != user.Password))
            {
                ViewData["WrongUsernamePassword"]=true; 
                return View();
            }
            //authenticates the user
            ActiveUser.User = new User(userDataModel);

            if(ActiveUser.User.Notifications.Count != 0)
            {
                ActiveUser.HasNotifications = true;
            }

            return RedirectToAction("HomePage","Home", new {pagination = 1});
        }

        public IActionResult LogOut()
        {
            if (ActiveUser.User == null) throw new NotImplementedException();

            ActiveUser.User = null;
            return RedirectToAction("Login", "Authentication");
        }
    }
}
