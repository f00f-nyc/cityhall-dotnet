using CityHall.Exceptions;
using System.Configuration;
using System.Linq;

namespace CityHall.Synchronous
{
    public class SyncSettings
    {
        /// <summary>
        /// Opens a connection to the given url using the user/password
        /// If any of the parameters are passed in as null (default for all of them), then the 
        /// library will get those values from the app config, or use standard defaults.
        /// </summary>
        /// <param name="url">The URL of the City Hall server, if it is null it will attempt to pull it from the config file</param>
        /// <param name="user">The name to login with.  If it is null, it will attempt to pull it from the config file. If it doesn't exist there, it will use the current machine name.</param>
        /// <param name="password">The password to use. If it is null, it will attempt to pull it from the config file. If it doesn't exist there, it will use a blank password.</param>
        /// <returns>An ISyncSettings instance</returns>
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
