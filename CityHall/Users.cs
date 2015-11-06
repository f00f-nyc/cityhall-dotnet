using CityHall.Data;
using CityHall.Synchronous;
using System.Threading.Tasks;

namespace CityHall
{
    internal class Users : IUsers
    {
        public Users(ISyncSettings self)
        {
            this.self = self;
        }

        private ISyncSettings self;

        public Task<UserInfo> Get(string username)
        {
            return Task.Factory.StartNew<UserInfo>(() => this.self.Users.Get(username));
        }

        public Task CreateUser(string userName, string password)
        {
            return Task.Factory.StartNew(() => this.self.Users.CreateUser(userName, password));
        }

        public Task DeleteUser(string userName)
        {
            return Task.Factory.StartNew(() => this.self.Users.DeleteUser(userName));
        }

        public Task Grant(string userName, string environment, Data.Rights rights)
        {
            return Task.Factory.StartNew(() => this.self.Users.Grant(userName, environment, rights));
        }
    }
}
