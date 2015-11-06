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

        public Task<string> GetValue(string path, string environment = null, string over = null)
        {
            return Task.Factory.StartNew<string>(() => this.SynchronousSettings.GetValue(path, environment, over));
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
        /// Opens a connection to the given url using the user/password
        /// If any of the parameters are passed in as null (default for all of them), then the 
        /// library will get those values from the app config, or use standard defaults.
        /// </summary>
        /// <param name="url">The URL of the City Hall server, if it is null it will attempt to pull it from the config file</param>
        /// <param name="user">The name to login with.  If it is null, it will attempt to pull it from the config file. If it doesn't exist there, it will use the current machine name.</param>
        /// <param name="password">The password to use. If it is null, it will attempt to pull it from the config file. If it doesn't exist there, it will use a blank password.</param>
        /// <returns>An ISettings instance</returns>
        public static async Task<ISettings> Get(string url=null, string user=null, string password=null)
        {
            Synchronous.ISyncSettings self = await Task.Factory.StartNew<ISyncSettings>(() => SyncSettings.Get(url, user, password));
            return self.AsynchronousSettings;
        }
    }
}
