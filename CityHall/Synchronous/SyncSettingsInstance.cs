using CityHall.Data;
using CityHall.Exceptions;
using CityHall.Responses;
using Ninject;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using CityHallNinject = CityHall.Config.Ninject;

namespace CityHall.Synchronous
{
    internal class SyncSettingsInstance : SyncClient, ISyncSettings
    {
        static SyncSettingsInstance()
        {
            CityHallNinject.RegisterIoc();
        }

        /// <summary>
        /// This will actually make a call into URL with the given username and password to establish a session.
        /// 
        /// It is made internal because the choice to put a potentially long-running call inside the constructor
        /// is non-standard, and users shouldn't be surprised with it.
        /// </summary>
        /// <param name="url">The url to use, cannot pass null</param>
        /// <param name="user">The username to use, cannot pass null</param>
        /// <param name="password">The plaintext password to use, null is acceptable</param>
        internal SyncSettingsInstance(string url, string user, string password)
            : base(CityHallNinject.Kernel.Get<IRestClient>(), url)
        {
            this.syncObject = new object();

            BaseResponse login = this.Post<BaseResponse>(new { username = user, passhash = Password.Hash(password) }, "auth/");
            if (login.IsValid)
            {
                try
                {
                    this.DefaultEnvironment = this.Get<ValueResponse>("auth/user/{0}/default/", user).value;
                }
                catch
                {
                    this.DefaultEnvironment = null;
                }
                finally
                {
                    lock (this.syncObject)
                    {
                        this.User = user;
                        this.LoggedIn = true;
                        this.Values = new SyncValues(this);
                    }
                }
            }
        }

        public bool LoggedIn { get; private set; }
        public ISyncValues Values { get; private set; }
        public object syncObject;
        public string DefaultEnvironment { get; protected set; }
        public string User { get; protected set; }

        internal void EnsureLoggedIn()
        {
            if (!this.LoggedIn)
            {
                throw new NotLoggedInException();
            }
        }

        public CityHall.ISettings AsynchronousSettings()
        {
            this.EnsureLoggedIn();

            throw new NotImplementedException();
        }

        public void SetDefaultEnvironment(string defaultEnvironment)
        {
            this.EnsureLoggedIn();
            this.Post<BaseResponse>(new { env = defaultEnvironment }, "auth/user/{0}/default/", this.User);
        }

        public void Logout()
        {
            lock (this.syncObject)
            {
                if (this.LoggedIn)
                {
                    BaseResponse delete = this.DeleteFormat("auth/");
                    this.LoggedIn = false;
                }
            }
        }

        public EnvironmentInfo GetEnvironment(string envName)
        {
            this.EnsureLoggedIn();
            EnvironmentResponse response = this.Get<EnvironmentResponse>("auth/env/{0}/", envName);
            return new EnvironmentInfo { Rights = response.Users.Select(kv => new EnvironmentRights { User = kv.Key, Rights = (Rights)kv.Value }).ToArray() };
        }

        public void CreateEnvironment(string envName)
        {
            this.EnsureLoggedIn();
            this.Post<BaseResponse>(new { }, "auth/env/{0}/", envName);
        }

        public UserInfo GetUserInfo(string userName)
        {
            this.EnsureLoggedIn();
            UserInfoResponse response = this.Get<UserInfoResponse>("auth/user/{0}", userName);
            return new UserInfo { Permissions = response.Environments.Select(kv => new UserRights { Environment = kv.Key, Rights = (Rights)kv.Value }).ToArray() };
        }

        /// <summary>
        /// Create a user with neither permissions nor default environment.
        /// </summary>
        /// <param name="userName">The user to create, cannot be this.User</param>
        /// <param name="password">The plaintext password to set, it will be hashed before being passed across the wire</param>
        public void CreateUser(string userName, string password)
        {
            if (string.Equals(userName, this.User))
            {
                throw new InvalidRequestException("You are passing your own user name to CreateUser(). Please use UpdatePassword() to update your own password");
            }

            this.EnsureLoggedIn();
            var hash = string.IsNullOrEmpty(password) ? "" : Password.Hash(password);
            this.Post<BaseResponse>(new { passhash = hash }, "auth/user/{0}/", userName);
        }

        public void UpdatePassword(string password)
        {
            this.EnsureLoggedIn();
            var hash = string.IsNullOrEmpty(password) ? "" : Password.Hash(password);
            this.Put(new { passhash = hash }, "auth/user/{0}/", this.User);
        }

        public void DeleteUser(string userName)
        {
            this.EnsureLoggedIn();
            this.DeleteFormat("auth/user/{0}/", userName);
        }

        public void Grant(string userName, string environment, Rights rights)
        {
            this.EnsureLoggedIn();
            this.Post<BaseResponse>(new { env = environment, user = userName, rights = (int)rights }, "auth/grant/");
        }

        internal string GetEnv(string environment = null)
        {
            if (string.IsNullOrEmpty(environment) && string.IsNullOrEmpty(this.DefaultEnvironment))
            {
                throw new InvalidRequestException("attempted to retreive a value without specifying an enviornment and user '{0}' has no default environment", this.User);
            }
            return string.IsNullOrEmpty(environment) ? this.DefaultEnvironment : environment;
        }

        public string GetValue(string path, string environment = null, string over = null)
        {
            return this.Values.GetValue(path, environment, over);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static string SanitizePath(string path)
        {
            StringBuilder sb = new StringBuilder();
            bool sbUsed = false;

            if (string.IsNullOrEmpty(path))
            {
                return "/";
            }

            if (path[0] != '/')
            {
                sbUsed = true;
                sb.Append('/');
            }

            sb.Append(path);

            if (!path.EndsWith("/"))
            {
                sbUsed = true;
                sb.Append('/');
            }

            return sbUsed ? sb.ToString() : path;
        }
        
        /// <summary>
        /// City Hall api places values at env/{environment name}/{path}/
        /// If you want to retrieve a specific override, you can specify as a query parameter.  (i.e. ?override={override})
        /// And, if you want to retrieve the default value, specifically, the same entry point is used, but no value
        /// is passed in. (i.e. ?override=)
        /// </summary>
        /// <param name="over">The optional value passed into the public function</param>
        /// <returns>A dictionary containing the override param, or an empty dictionary to be passed to the Get or Post wrappers</returns>
        internal static Dictionary<string, string> GetOverride(string over = null)
        {
            return over == null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string> { { "override", over } };
        }
    }
}
