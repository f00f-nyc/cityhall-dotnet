using CityHall.Data;
using AsyncISettings = CityHall.ISettings;

namespace CityHall.Synchronous
{
    public interface ISettings
    {
        AsyncISettings AsynchronousSettings();
        
        string DefaultEnvironment { get; }
        void SetDefaultEnvironment(string defaultEnvironment);
        void Logout();

        EnvironmentInfo GetEnvironment(string envName);
        void CreateEnvironment(string envName);
    }
}
