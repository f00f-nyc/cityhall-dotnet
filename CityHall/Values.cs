using CityHall.Data;
using CityHall.Synchronous;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CityHall
{
    internal class Values : IValues
    {
        public Values(ISyncSettings self)
        {
            this.self = self;
        }

        private ISyncSettings self;

        public Task<string> Get(string path, string environment = null, string over = null)
        {
            return Task.Factory.StartNew(() => this.self.Values.Get(path, environment, over));
        }

        public Task<T> GetRaw<T>(string environment, string path, Dictionary<string, string> args) 
            where T : Responses.BaseResponse, new()
        {
            return Task.Factory.StartNew<T>(() => this.self.Values.GetRaw<T>(environment, path, args));
        }

        public Task<History> GetHistory(string path, string over, string environment = null)
        {
            return Task.Factory.StartNew<History>(() => this.self.Values.GetHistory(path, over, environment));
        }

        public Task<Children> GetChildren(string path, string environment = null)
        {
            return Task.Factory.StartNew<Children>(() => this.self.Values.GetChildren(path, environment));
        }

        public Task SetRaw(string environment, string path, Value value, string over)
        {
            return Task.Factory.StartNew(() => this.self.Values.SetRaw(environment, path, value, over));
        }

        public Task Set(string environment, string path, string value, string over)
        {
            return Task.Factory.StartNew(() => this.self.Values.Set(environment, path, value, over));
        }

        public Task SetProtect(string environment, string path, bool protect, string over)
        {
            return Task.Factory.StartNew(() => this.self.Values.SetProtect(environment, path, protect, over));
        }

        public Task Delete(string environment, string path, string over)
        {
            return Task.Factory.StartNew(() => this.self.Values.Delete(environment, path, over));
        }
    }
}
