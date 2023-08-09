using Microsoft.AspNetCore.Mvc;
using DataAccess.Logic;
using DataAccess.Models;
using System.Collections.Generic;
using System;
using SoftwareTechnologyCalendarApplication.Models;

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
            ViewData["DuplicateEmail"] = false;
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

            ActiveUser.AccessConfirmationPage = true;
            return RedirectToAction("RegisterVerificationEmailMessage", "Services", new {username = user.Username , email = userr.Email});
        }

        public IActionResult Login(bool setFalseResetMessage)
        {
            if (ActiveUser.User != null) throw new NotImplementedException();

            ViewData["WrongUsernamePassword"] = false;
            ViewData["FalseResetAccount"] = setFalseResetMessage ? true : false;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(UserLogin user)
        {
            if (ActiveUser.User != null) throw new NotImplementedException();

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
            ActiveUser.User = new User(userDataModel);

            foreach (Event calendarEvent in ActiveUser.User.EventsThatTheUserParticipates)
            {
                if (calendarEvent.AlertStatus && (calendarEvent.StartingTime.AddHours(-1) < DateTime.Now && DateTime.Now < calendarEvent.EndingTime))
                {
                    _eventDataAccess.SendAlertNotification(calendarEvent.Id, ActiveUser.User.Username);
                }
            }

            //get the new notifications of the user, which might have been created by the alert status
            ActiveUser.User = new User(_userDataAccess.GetUser(user.Username));

            if (ActiveUser.User.Notifications.Count != 0)
            {
                ActiveUser.HasNotifications = true;
            }

            return RedirectToAction("HomePage","Home", new {pagination = 1});
        }

        public IActionResult ForgotPassword(string username, string email)
        {
            if (ActiveUser.User != null) throw new NotImplementedException();

            //if the username was not empty
            if(username != "" && username != null)
            {
                User user = new User(_userDataAccess.GetUser(username));
                //if the user does not exist
                if(user.Username == null)
                {
                    return RedirectToAction("Login", "Authentication", new { setFalseResetMessage = true });
                }
                //otherwise
                ActiveUser.AccessConfirmationPage = true;
                return RedirectToAction("ResetPasswordEmailMessage", "Services", new {username = username, email = user.Email});
            }
            //if the email want not empty
            string userUsername = "";
            foreach (UserDataModel tempUser in _userDataAccess.GetUsers())
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
            ActiveUser.AccessConfirmationPage = true;
            return RedirectToAction("ResetPasswordEmailMessage", "Services", new { username = userUsername, email = email });
            
        }

        public IActionResult ResetPassword(string username, string token)
        {
            if (ActiveUser.User != null) throw new NotImplementedException();

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
            if (ActiveUser.User != null) throw new NotImplementedException();
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

            ActiveUser.User = new User(updatedUser);
            return RedirectToAction("HomePage","Home");
        }

        public IActionResult LogOut()
        {
            ActiveUser.AuthorizeUser();

            ActiveUser.User = null;
            return RedirectToAction("Login", "Authentication");
        }
    }
}
