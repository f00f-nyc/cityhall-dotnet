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
    internal static class IRestRequestExt
    {
        public static void AddParameters(this IRestRequest request, Dictionary<string, string> args)
        {
            foreach (KeyValuePair<string, string> arg in args ?? new Dictionary<string, string>())
            {
                request.AddParameter(arg.Key, arg.Value);
            }
        }
    }

    public class SyncSettings : ISettings
    {
        static SyncSettings()
        {
            CityHallNinject.RegisterIoc();
        }

        internal SyncSettings(string url, string user, string password)
        {
            this.client = CityHallNinject.Kernel.Get<IRestClient>();
            this.client.BaseUrl = new Uri(url);
            this.client.CookieContainer = new CookieContainer();

            var login = this.Post<BaseResponse>(new { username = user, passhash = "" }, "auth/");
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
                    lock (this.client)
                    {
                        this.User = user;
                        this.LoggedIn = true;
                    }
                }
            }
        }

        private IRestClient client;
        public bool LoggedIn { get; protected set; }
        public string DefaultEnvironment { get; protected set; }
        public string User { get; protected set; }

        # region REST requests
        private T Execute<T>(IRestRequest request)
            where T : BaseResponse, new()
        {
            var response = this.client.Execute<T>(request);

            if (response == null)
            {
                throw new ErrorFromCityHallException(string.Format("Did not receive a response back from server for: method = {0}, resource = {1}", request.Method, request.Resource));
            }

            if (!response.Data.IsValid)
            {
                throw new ErrorFromCityHallException(response.Data.Message);
            }

            return response.Data;
        }

        private T Post<T>(object data, string location, params object[] args)
            where T : BaseResponse, new()
        {
            return this.PostWithParameters<T>(data, string.Format(location, args));
        }

        private T PostWithParameters<T>(object data, string location, Dictionary<string, string> args = null)
            where T : BaseResponse, new()
        {
            IRestRequest request = new RestRequest(location, Method.POST) { RequestFormat = DataFormat.Json };
            request.AddBody(data);
            request.AddParameters(args);
            return this.Execute<T>(request);
        }

        private T Get<T>(string location, params object[] args)
            where T : BaseResponse, new()
        {
            return this.GetWithParameters<T>(string.Format(location, args));
        }

        private T GetWithParameters<T>(string location, Dictionary<string, string> args = null)
            where T : BaseResponse, new()
        {
            IRestRequest request = new RestRequest(location, Method.GET) { RequestFormat = DataFormat.Json };
            request.AddParameters(args);
            return this.Execute<T>(request);
        }

        private BaseResponse DeleteFormat(string location, params object[] args)
        {
            return this.DeleteWithParameters(string.Format(location, args));
        }

        private BaseResponse DeleteWithParameters(string location, Dictionary<string, string> args = null)
        {
            IRestRequest request = new RestRequest(location, Method.DELETE) { RequestFormat = DataFormat.Json };
            request.AddParameters(args);
            return this.Execute<BaseResponse>(request);
        }
        #endregion

        #region CityHall.Synchronous.ISettings
        private void EnsureLoggedIn()
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
            this.Post<BaseResponse>(new { env=defaultEnvironment }, "auth/user/{0}/default/", this.User);
        }

        public void Logout()
        {
            lock (this.client)
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
            this.Post<BaseResponse>(new {}, "auth/env/{0}/", envName);
        }

        public UserInfo GetUserInfo(string userName)
        {
            this.EnsureLoggedIn();
            UserInfoResponse response = this.Get<UserInfoResponse>("auth/user/{0}", userName);
            return new UserInfo { Permissions = response.Environments.Select(kv => new UserRights { Environment = kv.Key, Rights = (Rights)kv.Value }).ToArray() };
        }

        public void CreateUser(string userName, string password)
        {
            if (string.Equals(userName, this.User))
            {
                throw new InvalidRequestException("You are passing your own user name to CreateUser(). Please use UpdatePassword() to update your own password");
            }

            this.SetPassword(userName, password);
        }

        public void UpdatePassword(string password)
        {
            this.SetPassword(this.User, password);
        }

        /// <summary>
        /// Even though CreateUser and UpdatePassword do the same thing, it makes a little more
        /// sense conceptually to break them out into their own, well-named methods, instead of
        /// requiring the user to know about the specifics of how the City Hall API works.
        /// </summary>
        /// <param name="userName">The user for whom to set the password, can be this.User</param>
        /// <param name="password">The plaintext password to set, it will be hashed before being passed across the wire</param>
        private void SetPassword(string userName, string password)
        {
            this.EnsureLoggedIn();
            var hash = string.IsNullOrEmpty(password) ? "" : SyncSettings.Hash(password);
            this.Post<BaseResponse>(new { passhash = hash }, "auth/user/{0}/", userName);
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

        public T GetRaw<T>(string environment, string path, Dictionary<string, string> args = null)
            where T : BaseResponse, new()
        {
            this.EnsureLoggedIn();
            path = string.Format("env/{0}{1}", environment, SyncSettings.SanitizePath(path));
            return this.GetWithParameters<T>(path, args);
        }

        private string GetEnv(string environment = null)
        {
            if (string.IsNullOrEmpty(environment) && string.IsNullOrEmpty(this.DefaultEnvironment))
            {
                throw new InvalidRequestException("attempted to retreive a value without specifying an enviornment and user '{0}' has no default environment", this.User);
            }
            return string.IsNullOrEmpty(environment) ? this.DefaultEnvironment : environment;
        }

        private static Dictionary<string, string> GetOverride(string over = null)
        {
            return string.IsNullOrEmpty(over)
                ? new Dictionary<string, string>()
                : new Dictionary<string, string> { { "override", over } };
        }

        public string GetValue(string path, string environment = null, string over = null)
        {
            return this.GetRaw<ValueResponse>(this.GetEnv(environment), path, args: SyncSettings.GetOverride(over)).value;
        }

        public Children GetChildren(string path, string environment = null)
        {
            ChildrenResponse response = this.GetRaw<ChildrenResponse>(this.GetEnv(environment), path, new Dictionary<string,string> { { "viewchildren", "true" } });
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

        public History GetHistory(string path, string environment = null, string over = null)
        {
            Dictionary<string, string> paramDict = SyncSettings.GetOverride(over);
            paramDict["viewhistory"] = "true";
            HistoryResponse response = this.GetRaw<HistoryResponse>(this.GetEnv(environment), path, paramDict);
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
        
        public void SetRaw(string environment, string path, Value value, string over = null)
        {
            this.EnsureLoggedIn();
            string location = string.Format("env/{0}{1}", this.GetEnv(environment), SyncSettings.SanitizePath(path));
            this.PostWithParameters<BaseResponse>(value.ToPayload(), location, SyncSettings.GetOverride(over));
        }

        public void Set(string environment, string path, string value, string over = null)
        {
            this.SetRaw(environment, path, new Value(value), over);
        }

        public void SetProtect(string environment, string path, bool protect, string over = null)
        {
            this.SetRaw(environment, path, new Value(protect), over);
        }

        public void Delete(string environment, string path, string over = null)
        {
            this.EnsureLoggedIn();
            string location = string.Format("env/{0}{1}", this.GetEnv(environment), SyncSettings.SanitizePath(path));
            this.DeleteWithParameters(location, SyncSettings.GetOverride(over));
        }        
        #endregion

        /**
         * Opens a connection to the given url using the user/password
         * If any of the parameters are passed in as null (default for all of them), then the 
         * library will get those values from the app config. If the username is not
         * in the app config and not passed in, the machine name will be used.  Likewise,
         * if the password can't be found, a blank one will be used.
         */
        public static ISettings Get(string url = null, string user = null, string password = null)
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
            return new SyncSettings(url, user, password);            
        }

        public static string Hash(string password)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(password);
            return string.Join("", md5.ComputeHash(inputBytes).Select(b => b.ToString("X2")));
        }

        private static string SanitizePath(string path)
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
    }
}
