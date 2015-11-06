using CityHall.Synchronous;
using System.Threading.Tasks;

namespace CityHall
{
    public class Settings : ISettings
    {
        internal Settings(ISyncSettings self)
        {
            this.SynchronousSettings = self;
            this.Values = new Values(this.SynchronousSettings);
            this.Environments = new Environments(this.SynchronousSettings);
            this.Users = new Users(this.SynchronousSettings);
        }

        public ISyncSettings SynchronousSettings { get; private set; }
        public IValues Values { get; private set; }
        public IEnvironments Environments { get; private set; }
        public IUsers Users { get; private set; }
        public string User { get { return this.SynchronousSettings.User; } }
        public bool LoggedIn { get { return this.SynchronousSettings.LoggedIn; } }

        public Task<string> GetValue(string path, string environment = null, string over = null)
        {
            return Task.Factory.StartNew<string>(() => this.SynchronousSettings.Values.Get(path, environment, over));
        }

        public Task UpdatePassword(string password)
        {
            return Task.Factory.StartNew(() => this.SynchronousSettings.UpdatePassword(password));
        }

        public Task Logout()
        {
            return Task.Factory.StartNew(() => this.SynchronousSettings.Logout());
        }

        /// <summary>
        /// A static instance of ISettings which will be used by Settings.Get()
        /// It cannot be set and will be instantiated on the first call to Settings.Get() or SyncSettings.Get()
        /// </summary>
        public static ISettings Connection { get { return SyncSettings.Connection.AsynchronousSettings; } }

        /// <summary>
        /// Opens a connection to the given url using the user/password
        /// If any of the parameters are passed in as null (default for all of them), then the 
        /// library will get those values from the app config, or use standard defaults.
        /// </summary>
        /// <param name="url">The URL of the City Hall server, if it is null it will attempt to pull it from the config file</param>
        /// <param name="user">The name to login with.  If it is null, it will attempt to pull it from the config file. If it doesn't exist there, it will use the current machine name.</param>
        /// <param name="password">The password to use. If it is null, it will attempt to pull it from the config file. If it doesn't exist there, it will use a blank password.</param>
        /// <returns>An ISettings instance</returns>
        public static async Task<ISettings> New(string url=null, string user=null, string password=null)
        {
            Synchronous.ISyncSettings self = await Task.Factory.StartNew<ISyncSettings>(() => SyncSettings.New(url, user, password));
            return self.AsynchronousSettings;
        }

        /// <summary>
        /// Retrieves the value from the server. If the value doesn't exist, or you don't have access to it, returns null.
        /// 
        /// This (and its sister in CityHall.Synchronous.SyncSettings) should be the most often-used function of the library.
        /// The first call will instantiate Connection, using config file settings.
        /// </summary>
        /// <param name="path">The path of the value. Starting or trailing '/' may be omitted.</param>
        /// <param name="environment">If it is null or empty, it will use ISyncSettings.DefaultEnvironmnet</param>
        /// <param name="over">The override to get. If this is unspecified, then the override that matches
        /// the logged in user is retrieved.  Otherwise, the default value is returned.</param>
        public static Task<string> Get(string path, string environment = null, string over = null)
        {
            return Task.Factory.StartNew<string>(() => SyncSettings.Get(path, environment, over));
        }
    }
}
