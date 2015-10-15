using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityHall.Synchronous
{
    public class Settings
    {
        /**
         * Opens a connection to the given url using the user/password
         */
        public static ISettings FromUrl(string url, string user, string password)
        {
            throw new NotImplementedException();
        }

        /**
         * Opens a connection to the given url using the user/password
         * If any of the parameters are passed in as null (default for all of them), then the 
         * library will get those values from the app config. If the username is not
         * in the app config and not passed in, the machine name will be used.  Likewise,
         * if the password can't be found, a blank one will be used.
         */
        public static ISettings Create(string url = null, string user = null, string password = null)
        {
            throw new NotImplementedException();
        }
    }
}
