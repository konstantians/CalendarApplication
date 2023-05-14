using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftwareTechnologyCalendarApplication.Models
{
    public class UserLogin
    {
        [Required(ErrorMessage = "You need to provide a Username")]
        public string Username { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "You need to provide a Password")]
        public string Password { get; set; }
        
        //default constructor
        public UserLogin() { }

        //constructor
        public UserLogin(string username, string password)
        {
            Username = username;
            Password = password;
        }

        //copy constructor
        public UserLogin(UserLogin user)
        {
            Username = user.Username;
            Password = user.Password;
        }

        //copy constructor for data model
        public UserLogin(UserDataModel userDataModel)
        {

        }
    }
}
