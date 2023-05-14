using Microsoft.AspNetCore.Mvc;
using SoftwareTechnologyCalendarApplication.Models;
using DataAccess.Logic;
using DataAccess.Models;
using Microsoft.Extensions.Configuration;
using System;

namespace SoftwareTechnologyCalendarApplicationMVC.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IUserDataAccess _userDataAccess;
        public AuthenticationController(IUserDataAccess userDataAccess)
        {

            _userDataAccess = userDataAccess;

        }
        public IActionResult Register()
        {
            return View();
            //return View("Register", "~/Views/Shared/_emptyLayout.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            if(ModelState.IsValid)
            {
                UserDataModel userr = new UserDataModel();
                userr.Username = user.Username;
                userr.Password = user.Password;
                userr.Email = user.Email;
                userr.Phone = user.Phone;
                userr.Fullname = user.Fullname;
                _userDataAccess.CreateUser(userr);
                return RedirectToAction("Login");
            }

            return View();
        }

        public IActionResult Login()
        {
            ViewData["WrongUsernamePassword"] = false;
            return View();
            //return View("Login", "~/Views/Shared/_emptyLayout.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(UserLogin user)
        {
            ViewData["WrongUsernamePassword"] = false;
            if(!ModelState.IsValid) {
                return View(user);
            }
            UserDataModel userDataModel = _userDataAccess.GetUser(user.Username);
            if ((userDataModel == null) || (userDataModel.Password != user.Password))
            {
                ViewData["WrongUsernamePassword"]=true; return View();
                //throw new Exception("There is no user with the username and password that you provided");
            }
            return RedirectToAction("Register");
            //return View("SuccessfulLogin_HomePageOfUser");
        }
    }
}
