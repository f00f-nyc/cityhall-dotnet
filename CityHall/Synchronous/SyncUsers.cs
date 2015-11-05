using CityHall.Data;
using CityHall.Exceptions;
using CityHall.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityHall.Synchronous
{
    internal class SyncUsers : ISyncUsers
    {
        public SyncUsers(SyncSettingsInstance client)
        {
            this.client = client;
        }

        private SyncSettingsInstance client;

        public UserInfo Get(string userName)
        {
            this.client.EnsureLoggedIn();
            UserInfoResponse response = this.client.Get<UserInfoResponse>("auth/user/{0}", userName);
            return new UserInfo { Permissions = response.Environments.Select(kv => new UserRights { Environment = kv.Key, Rights = (Rights)kv.Value }).ToArray() };
        }

        public void CreateUser(string userName, string password)
        {
            if (string.Equals(userName, this.client.User))
            {
                throw new InvalidRequestException("You are passing your own user name to CreateUser(). Please use UpdatePassword() to update your own password");
            }

            this.client.EnsureLoggedIn();
            var hash = string.IsNullOrEmpty(password) ? "" : Password.Hash(password);
            this.client.Post<BaseResponse>(new { passhash = hash }, "auth/user/{0}/", userName);
        }

        public void DeleteUser(string userName)
        {
            this.client.EnsureLoggedIn();
            this.client.DeleteFormat("auth/user/{0}/", userName);
        }

        public void Grant(string userName, string environment, Rights rights)
        {
            this.client.EnsureLoggedIn();
            this.client.Post<BaseResponse>(new { env = environment, user = userName, rights = (int)rights }, "auth/grant/");
        }
    }
}
