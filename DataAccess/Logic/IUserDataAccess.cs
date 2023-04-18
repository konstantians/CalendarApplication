using DataAccess.Models;
using System.Collections.Generic;

namespace DataAccess.Logic
{
    public interface IUserDataAccess
    {
        void CreateUser(UserDataModel user);
        void DeleteUser(string username);
        UserDataModel GetUser(string username);
        List<UserDataModel> GetUsers();
        void UpdateUser(UserDataModel user);
        void UpdateUserAndUsername(UserDataModel user, string oldUsername);
    }
}