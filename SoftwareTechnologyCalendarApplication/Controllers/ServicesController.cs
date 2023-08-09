using DataAccess.Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IIS.Core;
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
        public ServicesController(IEmailService emailService, IUserDataAccess userDataAccess)
        {
            EmailService = emailService;
            UserDataAccess = userDataAccess;
        }

        [HttpPost]
        public IActionResult ContactFormSubmission(string emailSender, string messageTitle, string messageContent)
        {
            ActiveUser.AuthorizeUser();

            // in this case kinnaskonstantinos0@gmail.com just sends an email to itself, but we still pass the email of the user
            // using the body of the email
            bool result = EmailService.SendContactFormEmail(emailSender, messageTitle, messageContent);
            if(result)
            {
                ViewData["EmailSendSuccessfully"] = true;
                return View();
            }
            else
            {
                ViewData["EmailSendSuccessfully"] = false;
                return View();
            }
        }

        public IActionResult RegisterVerificationEmailMessage(string username, string email)
        {
            ActiveUser.CheckAccessConfirmationPage();
            ActiveUser.AccessConfirmationPage = false;

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
            ActiveUser.CheckAccessConfirmationPage();
            ActiveUser.AccessConfirmationPage = false;

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
            if (UserDataAccess.TokenExists(username, token)) {
                UserDataAccess.ActivateUser(username, token);
            }
            else
            {
                throw new NotImplementedException();
            }

            ActiveUser.User = new User(UserDataAccess.GetUser(username));
            return RedirectToAction("HomePage", "Home", new { pagination = 1 });
        }
    }
}
