using CityHall.Data;
using CityHall.Exceptions;
using CityHall.Responses;
using Ninject;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using CityHallNinject = CityHall.Config.Ninject;

namespace CityHall.Synchronous
{
    public class SyncSettings
    {
        /**
         * Opens a connection to the given url using the user/password
         * If any of the parameters are passed in as null (default for all of them), then the 
         * library will get those values from the app config. If the username is not
         * in the app config and not passed in, the machine name will be used.  Likewise,
         * if the password can't be found, a blank one will be used.
         */
        public static ISyncSettings Get(string url = null, string user = null, string password = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var section = config.Sections.OfType<CityHall.Config.CityHallConfigSection>().FirstOrDefault();

                if (section == null)
                {
                    throw new MissingConfigSection();
                }

                url = section.Url;

                if (string.IsNullOrEmpty(user))
                {
                    user = section.User;
                    password = section.Password;
                }
            }

            if (string.IsNullOrEmpty(user))
            {
                user = System.Environment.MachineName;
                password = "";
            }

            password = string.IsNullOrEmpty(password) ? "" : password;
            return new SyncSettingsInstance(url, user, password);
        }
    }
}
