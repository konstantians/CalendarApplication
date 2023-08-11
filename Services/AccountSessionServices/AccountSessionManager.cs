using DataAccess.Logic;
using Microsoft.AspNetCore.Http;
using System;

namespace Services.AccountSessionServices
{
    public class AccountSessionManager : IAccountSessionManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserDataAccess _userDataAccess;

        public AccountSessionManager(IHttpContextAccessor httpContextAccessor, IUserDataAccess userDataAccess)
        {
            _httpContextAccessor = httpContextAccessor;
            _userDataAccess = userDataAccess;
        }
        public void CreateSession(string username)
        {
            Guid guid = Guid.NewGuid();
            string sessionTokenValue = $"sessionToken_{username}_{guid}";

            //create for security a session token in the database, which will be
            //used to make sure that the cookie is not fake.
            _userDataAccess.CreateSessionToken(sessionTokenValue, username);

            // Create a cookie and add the cookie to the response
            _httpContextAccessor.HttpContext.Response.Cookies.Append("CalendarAppSessionCookie", sessionTokenValue, new CookieOptions
            {
                //without specifyig the expiration date the cookie becomes a session cookie
                //which means that it will be active until that person closes their browser.
                /*Expires = DateTime.Now.AddHours(3),*/
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });
        }

        public (string,string) ReadSessionCookie()
        {
            // Read the value of the "CalendarAppSessionCookie"
            string cookieString = _httpContextAccessor.HttpContext.Request.Cookies["CalendarAppSessionCookie"];
            // If the browser does not have a token return failure 
            if(cookieString == null)
                return ("", "");

            string[] cookieParts = cookieString.Split('_');
            string username = cookieParts[1];
            string cookieValue = cookieParts[2];

            // If the database does not contain a token with the value of the cookie 
            // return failure(this is for security)
            if (!_userDataAccess.TokenExists(username, cookieString))
                return ("", "");
            
            return (username, cookieValue);
        }

        public void DeleteSessionCookie()
        {
            string sessionTokenValue = _httpContextAccessor.HttpContext.Request.Cookies["CalendarAppSessionCookie"];
            //if the cookie does not exist
            if (sessionTokenValue == "")
                return;
            _userDataAccess.DeleteToken(sessionTokenValue);
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("CalendarAppSessionCookie");
        }
    }
}
