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
        public static ISyncSettings New(string url = null, string user = null, string password = null)
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

        /// <summary>
        /// A static instance of ISyncSettings which will be used by SyncSettings.Get()
        /// It cannot be set and will be instantiated on the first call to Settings.Get() or SyncSettings.Get()
        /// </summary>
        public static ISyncSettings Connection { get; internal set; }

        /// <summary>
        /// Retrieves the value from the server. If the value doesn't exist, or you don't have access to it, returns null.
        /// 
        /// This (and its async sister in CityHall.Settings) should be the most often-used function of the library.
        /// The first call will instantiate CityHall.Synchronous.SyncSettings.Connection, using config file settings.
        /// </summary>
        /// <param name="path">The path of the value. Starting or trailing '/' may be omitted.</param>
        /// <param name="environment">If it is null or empty, it will use ISyncSettings.DefaultEnvironmnet</param>
        /// <param name="over">The override to get. If this is unspecified, then the override that matches
        /// the logged in user is retrieved.  Otherwise, the default value is returned.</param>
        public static string Get(string path, string environment = null, string over = null)
        {
            if ((SyncSettings.Connection == null) || !SyncSettings.Connection.LoggedIn)
            {
                lock (SyncSettings.syncObject)
                {
                    SyncSettings.Connection = SyncSettings.New();
                }
            }

            return SyncSettings.Connection.Values.Get(path, environment, over);
        }

        private static object syncObject = new object();
    }
}
