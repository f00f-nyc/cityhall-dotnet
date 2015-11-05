using CityHall.Config;
using CityHall.Synchronous;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityHall
{
    public class Settings
    {
        /**
         * Opens a connection to the given url using the user/password
         * If any of the parameters are passed in as null (default for all of them), then the 
         * library will get those values from the app config. If the username is not
         * in the app config and not passed in, the machine name will be used.  Likewise,
         * if the password can't be found, a blank one will be used.
         */
        public static async Task<ISettings> Get(string url=null, string user=null, string password=null)
        {
            Synchronous.ISyncSettings self = await Task.Factory.StartNew<ISyncSettings>(() => SyncSettings.Get(url, user, password));
            return self.AsynchronousSettings;
        }
    }
}
