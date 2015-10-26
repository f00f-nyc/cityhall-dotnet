using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncISettings = CityHall.ISettings;

namespace CityHall.Synchronous
{
    public interface ISettings
    {
        AsyncISettings AsynchronousSettings();
        
        string DefaultEnvironment { get; }
        void SetDefaultEnvironment(string defaultEnvironment);
        void Logout();
    }
}
