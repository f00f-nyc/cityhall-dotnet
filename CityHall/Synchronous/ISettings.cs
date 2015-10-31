using CityHall.Data;
using CityHall.Responses;
using System.Collections.Generic;
using AsyncISettings = CityHall.ISettings;

namespace CityHall.Synchronous
{
    public interface ISettings
    {
        AsyncISettings AsynchronousSettings();
        
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

        T GetRaw<T>(string environment, string path, Dictionary<string, string> args)
            where T : BaseResponse, new();
        string GetValue(string path, string environment = null, string over = null);
        Children GetChildren(string path, string environment = null);
        History GetHistory(string path, string environment = null, string over = null);

        void SetRaw(string environment, string path, Value value, string over = null);
        void Set(string environment, string path, string value, string over = null);
        void SetProtect(string environment, string path, bool protect, string over = null);

        void Delete(string environment, string path, string over = null);
    }
}
