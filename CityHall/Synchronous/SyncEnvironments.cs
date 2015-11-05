using CityHall.Data;
using CityHall.Responses;
using System.Linq;

namespace CityHall.Synchronous
{
    internal class SyncEnvironments : ISyncEnvironments
    {
        public SyncEnvironments(SyncSettingsInstance client, string defaultEnvironment)
        {
            this.client = client;
            this.Default = defaultEnvironment;
        }

        private SyncSettingsInstance client;

        public string Default { get; private set; }

        public void SetDefault(string defaultEnvironment)
        {
            this.client.EnsureLoggedIn();
            this.client.Post<BaseResponse>(new { env = defaultEnvironment }, "auth/user/{0}/default/", this.client.User);
            this.Default = defaultEnvironment;
        }

        public EnvironmentInfo Get(string envName)
        {
            this.client.EnsureLoggedIn();
            EnvironmentResponse response = this.client.Get<EnvironmentResponse>("auth/env/{0}/", envName);
            return new EnvironmentInfo { Rights = response.Users.Select(kv => new EnvironmentRights { User = kv.Key, Rights = (Rights)kv.Value }).ToArray() };
        }

        public void Create(string envName)
        {
            this.client.EnsureLoggedIn();
            this.client.Post<BaseResponse>(new { }, "auth/env/{0}/", envName);
        }
    }
}
