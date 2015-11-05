using CityHall.Data;
using CityHall.Responses;
using System.Collections.Generic;
using System.Linq;

namespace CityHall.Synchronous
{
    internal class SyncValues : ISyncValues
    {
        public SyncValues(SyncSettingsInstance client)
        {
            this.client = client;
        }

        private SyncSettingsInstance client;

        public string GetValue(string path, string environment = null, string over = null)
        {
            return this.GetRaw<ValueResponse>(this.client.GetEnv(environment), path, args: SyncSettingsInstance.GetOverride(over)).value;
        }

        public T GetRaw<T>(string environment, string path, Dictionary<string, string> args) 
            where T : Responses.BaseResponse, new()
        {
            this.client.EnsureLoggedIn();
            path = string.Format("env/{0}{1}", environment, SyncSettingsInstance.SanitizePath(path));
            return this.client.GetWithParameters<T>(path, args);
        }

        public History GetHistory(string path, string over, string environment = null)   
        {
            Dictionary<string, string> paramDict = SyncSettingsInstance.GetOverride(over);
            paramDict["viewhistory"] = "true";
            HistoryResponse response = this.GetRaw<HistoryResponse>(this.client.GetEnv(environment), path, paramDict);
            return new History
            {
                Entries = response.History
                    .Select(h => new Entry
                    {
                        Active = h.active,
                        Author = h.author,
                        DateTime = h.datetime,
                        Id = h.id,
                        Name = h.name,
                        Override = h.@override,
                        Protect = h.protect,
                        Value = h.value
                    })
                    .ToArray()
            };
        }

        public Children GetChildren(string path, string environment = null)
        {
            ChildrenResponse response = this.GetRaw<ChildrenResponse>(
                this.client.GetEnv(environment), 
                path, 
                new Dictionary<string, string> { { "viewchildren", "true" } }
            );

            return new Children
            {
                Path = response.path,
                SubChildren = response.children
                  .Select(c => new Child
                  {
                      Id = c.id,
                      Name = c.name,
                      Override = c.@override,
                      Path = c.path,
                      Protect = c.protect,
                      Value = c.value
                  })
                  .ToArray()
            };
        }

        public void SetRaw(string environment, string path, Data.Value value, string over)
        {
            this.client.EnsureLoggedIn();
            string location = string.Format("env/{0}{1}", this.client.GetEnv(environment), SyncSettingsInstance.SanitizePath(path));
            this.client.PostWithParameters<BaseResponse>(value.ToPayload(), location, SyncSettingsInstance.GetOverride(over));
        }

        public void Set(string environment, string path, string value, string over)
        {
            this.SetRaw(environment, path, new Value(value), over);
        }

        public void SetProtect(string environment, string path, bool protect, string over)
        {
            this.SetRaw(environment, path, new Value(protect), over);
        }

        public void Delete(string environment, string path, string over)
        {
            this.client.EnsureLoggedIn();
            string location = string.Format("env/{0}{1}", this.client.GetEnv(environment), SyncSettingsInstance.SanitizePath(path));
            this.client.DeleteWithParameters(location, SyncSettingsInstance.GetOverride(over));
        }
    }
}
