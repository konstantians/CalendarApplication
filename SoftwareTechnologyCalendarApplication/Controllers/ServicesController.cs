using DataAccess.Logic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IIS.Core;
using Services.AccountSessionServices;
using Services.BackgroundServices;
using Services.EmailSendingMechanism;
using SoftwareTechnologyCalendarApplication;
using SoftwareTechnologyCalendarApplication.Models;
using System;
using System.Web;

namespace SoftwareTechnologyCalendarApplicationMVC.Controllers
{
    public class ServicesController : Controller
    {
        private readonly IEmailService EmailService;
        private readonly IUserDataAccess UserDataAccess;
        private readonly IActiveUsers ActiveUsers;
        private readonly IAccountSessionManager AccountSessionManager;
        public ServicesController(IEmailService emailService, IUserDataAccess userDataAccess, IActiveUsers activeUsers, IAccountSessionManager accountSessionManager)
        {
            EmailService = emailService;
            UserDataAccess = userDataAccess;
            ActiveUsers = activeUsers;
            AccountSessionManager = accountSessionManager;
        }

        [HttpPost]
        public IActionResult ContactFormSubmission(string emailSender, string messageTitle, string messageContent)
        {
            if (!ActiveUsers.CheckIfLoggedIn())
                return RedirectToAction("Login", "Authentication");
            ActiveUsers.InstantiateUser();
            ActiveUsers.CheckForNotifications();

            // in this case kinnaskonstantinos0@gmail.com just sends an email to itself, but we still pass the email of the user
            // using the body of the email
            bool result = EmailService.SendContactFormEmail(emailSender, messageTitle, messageContent);

            //set for this specific method the regular layout
            ViewData["Layout"] = "_Layout";
            ViewData["EmailSendSuccessfully"] = result ? true : false;
            return View();
        }

        public IActionResult RegisterVerificationEmailMessage(string username, string email)
        {
            if (ActiveUsers.CheckIfLoggedIn())
                return RedirectToAction("HomePage", "Home");

            if (!TempData.ContainsKey("allowed"))
                return RedirectToAction("Login", "Authentication");

            string token = Guid.NewGuid().ToString();
            string confirmationLink = Url.Action("ConfirmEmail", "Services", new { token, username }, Request.Scheme);
            bool emailSendSuccessfully = EmailService.SendEmail(email, "Account Confirmation",
                $"Please click on the following link to confirm your email: {confirmationLink}");
            
            if (emailSendSuccessfully)
            {
                ViewData["EmailSendSuccessfully"] = true; 
                UserDataAccess.CreateAccountActivationToken(token, username);
                return View();
            }
            ViewData["EmailSendSuccessfully"] = false;
            //delete the user, because there is no way to activate them.
            UserDataAccess.DeleteUser(username);
            return View();
        }

        public IActionResult ResetPasswordEmailMessage(string username, string email)
        {
            if (ActiveUsers.CheckIfLoggedIn())
                return RedirectToAction("HomePage", "Home");

            if (!TempData.ContainsKey("allowed"))
                return RedirectToAction("Login", "Authentication");

            string token = Guid.NewGuid().ToString();
            string confirmationLink = Url.Action("ResetPassword", "Authentication", new { token, username }, Request.Scheme);
            bool emailSendSuccessfully = EmailService.SendEmail(email, "Reset Account Password",
                $"Please click on the following link to reset your account password: {confirmationLink}");

            if (emailSendSuccessfully)
            {
                ViewData["EmailSendSuccessfully"] = true;
                UserDataAccess.CreateResetPasswordToken(token, username);
                return View();
            }
            ViewData["EmailSendSuccessfully"] = false;
            return View();
        }

        public IActionResult ConfirmEmail(string token, string username)
        {
            //if the token is not valid
            if (!UserDataAccess.TokenExists(username, token)) {
                throw new NotImplementedException();
            }

            //activate the user
            UserDataAccess.ActivateUser(username, token);
            //create the session which means that a session token is created in the database
            //and a cookie is created in the users browser
            AccountSessionManager.CreateSession(username);

            return RedirectToAction("HomePage", "Home", new { pagination = 1 });
        }
    }
}
