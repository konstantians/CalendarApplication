using Microsoft.AspNetCore.Mvc;
using DataAccess.Logic;
using DataAccess.Models;
using System.Collections.Generic;
using System;
using SoftwareTechnologyCalendarApplication.Models;
using Services.AccountSessionServices;

namespace SoftwareTechnologyCalendarApplication.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IUserDataAccess _userDataAccess;
        private readonly IEventDataAccess _eventDataAccess;
        private readonly IAccountSessionManager _accountSessionManager;
        private readonly IActiveUsers _activeUsers;

        public AuthenticationController(IUserDataAccess userDataAccess, IEventDataAccess eventDataAccess, 
            IAccountSessionManager accountSessionManager, IActiveUsers activeUsers)
        {
            _userDataAccess = userDataAccess;
            _eventDataAccess = eventDataAccess;
            _accountSessionManager = accountSessionManager;
            _activeUsers = activeUsers;
        }
        public IActionResult Register()
        {
            if(_activeUsers.CheckIfLoggedIn())
                return RedirectToAction("HomePage", "Home");

            ViewData["DuplicateAccount"] = false;
            ViewData["DuplicateEmail"] = false;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            if (_activeUsers.CheckIfLoggedIn())
                return RedirectToAction("HomePage", "Home");

            if (!ModelState.IsValid)
            {
                return View();
            }

            List<UserDataModel> users = _userDataAccess.GetUsers(true);
            foreach (UserDataModel userDataModel in users)
            {
                if(userDataModel.Username == user.Username)
                {
                    ViewData["DuplicateAccount"] = true;
                    ViewData["DuplicateEmail"] = false;
                    return View();
                }

                if(userDataModel.Email == user.Email)
                {
                    ViewData["DuplicateAccount"] = false;
                    ViewData["DuplicateEmail"] = true;
                    return View();
                }
            }

            UserDataModel userr = new UserDataModel();
            userr.Fullname = user.Fullname;
            userr.Username = user.Username;
            userr.Password = user.Password;
            userr.DateOfBirth = user.DateOfBirth;
            userr.Email = user.Email;
            userr.Phone = user.Phone;
            _userDataAccess.CreateUser(userr);

            TempData["allowed"] = true;
            return RedirectToAction("RegisterVerificationEmailMessage", "Services", new {username = user.Username , email = userr.Email});
        }

        public IActionResult Login(bool setFalseResetMessage)
        {
            if (_activeUsers.CheckIfLoggedIn())
                return RedirectToAction("HomePage", "Home");

            ViewData["WrongUsernamePassword"] = false;
            ViewData["FalseResetAccount"] = setFalseResetMessage ? true : false;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(UserLogin user)
        {
            if (_activeUsers.CheckIfLoggedIn())
                return RedirectToAction("HomePage", "Home");

            ViewData["WrongUsernamePassword"] = false;
            ViewData["FalseResetAccount"] = false;

            if(!ModelState.IsValid) {
                return View(user);
            }

            UserDataModel userDataModel = _userDataAccess.GetUser(user.Username);
            if ((userDataModel == null) || (userDataModel.Password != user.Password))
            {
                ViewData["WrongUsernamePassword"]=true;
                ViewData["FalseResetAccount"] = false;
                return View();
            }

            //authenticates the user
            _accountSessionManager.CreateSession(userDataModel.Username);

            _activeUsers.User = new User(userDataModel);
            foreach (Event calendarEvent in _activeUsers.User.EventsThatTheUserParticipates)
            {
                if (calendarEvent.AlertStatus && (calendarEvent.StartingTime.AddHours(-1) < DateTime.Now && DateTime.Now < calendarEvent.EndingTime))
                {
                    _eventDataAccess.SendAlertNotification(calendarEvent.Id, _activeUsers.User.Username);
                }
            }

            return RedirectToAction("HomePage","Home", new {pagination = 1});
        }

        public IActionResult ForgotPassword(string username, string email)
        {
            if (_activeUsers.CheckIfLoggedIn())
                return RedirectToAction("HomePage", "Home");

            //if the username was not empty
            if (username != "" && username != null)
            {
                User user = new User(_userDataAccess.GetUser(username));
                //if the user does not exist
                if(user.Username == null)
                {
                    return RedirectToAction("Login", "Authentication", new { setFalseResetMessage = true });
                }
                //otherwise

                TempData["allowed"] = true;
                return RedirectToAction("ResetPasswordEmailMessage", "Services", new {username = username, email = user.Email});
            }
            //if the email want not empty
            string userUsername = "";
            foreach (UserDataModel tempUser in _userDataAccess.GetUsers(false))
            {
                if (tempUser.Email == email)
                {
                    userUsername = tempUser.Username;
                }
            }

            //if there is no user with that username
            if (userUsername == "")
            {
                return RedirectToAction("Login", "Authentication", new { setFalseResetMessage = true});
            }
            //otherwise
            TempData["allowed"] = true;
            return RedirectToAction("ResetPasswordEmailMessage", "Services", new { username = userUsername, email = email });
            
        }

        public IActionResult ResetPassword(string username, string token)
        {
            if (_activeUsers.CheckIfLoggedIn())
                return RedirectToAction("HomePage", "Home");

            if (!_userDataAccess.TokenExists(username, token))
            {
                throw new NotImplementedException();
            }
            ResetPasswordViewModel resetPasswordViewModel = new ResetPasswordViewModel();
            resetPasswordViewModel.Username = username;
            resetPasswordViewModel.Token = token;
            return View(resetPasswordViewModel);
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (_activeUsers.CheckIfLoggedIn())
                return RedirectToAction("HomePage", "Home");

            if (!_userDataAccess.TokenExists(resetPasswordViewModel.Username, resetPasswordViewModel.Token))
            {
                throw new NotImplementedException();
            }

            if (!ModelState.IsValid)
            {
                return View(resetPasswordViewModel);
            }

            UserDataModel updatedUser = _userDataAccess.GetUser(resetPasswordViewModel.Username);
            updatedUser.Password = resetPasswordViewModel.Password;
            _userDataAccess.UpdateUser(updatedUser);

            //now that the user is updated delete the token
            _userDataAccess.DeleteToken(resetPasswordViewModel.Token);

            //create the session token and the cookie for the updated user
            _accountSessionManager.CreateSession(updatedUser.Username);

            return RedirectToAction("HomePage","Home");
        }

        public IActionResult LogOut()
        {
            if (!_activeUsers.CheckIfLoggedIn())
                return RedirectToAction("Login", "Authentication");

            _accountSessionManager.DeleteSessionCookie();
            return RedirectToAction("Login", "Authentication");
        }
    }
}
