using CityHall.Data;
using CityHall.Responses;
using System.Collections.Generic;
using CityHall;

namespace CityHall.Synchronous
{
    public interface ISyncSettings
    {
        ISettings AsynchronousSettings();
        ISyncValues Values { get; }
        string GetValue(string path, string environment = null, string over = null);
        
        string DefaultEnvironment { get; }
        void Logout();

        void SetDefaultEnvironment(string defaultEnvironment);
        EnvironmentInfo GetEnvironment(string envName);
        void CreateEnvironment(string envName);
        UserInfo GetUserInfo(string username);
        void CreateUser(string userName, string password);
        void UpdatePassword(string password);
        void DeleteUser(string userName);
        void Grant(string userName, string environment, Rights rights);
    }
}
