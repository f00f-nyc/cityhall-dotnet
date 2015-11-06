using CityHall.Data;
using CityHall.Synchronous;
using System.Threading.Tasks;

namespace CityHall
{
    internal class Environments : IEnvironments
    {
        public Environments(ISyncSettings self)
        {
            this.self = self;
        }

        private ISyncSettings self;

        public string Default { get { return this.self.Environments.Default; } }

        public Task SetDefault(string defaultEnvironment)
        {
            return Task.Factory.StartNew(() => this.self.Environments.SetDefault(defaultEnvironment));
        }

        public Task<EnvironmentInfo> Get(string envName)
        {
            return Task.Factory.StartNew<EnvironmentInfo>(() => this.self.Environments.Get(envName));
        }

        public Task Create(string envName)
        {
            return Task.Factory.StartNew(() => this.self.Environments.Create(envName));
        }
    }
}
