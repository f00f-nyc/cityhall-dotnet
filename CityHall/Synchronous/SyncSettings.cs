using CityHall.Data;
using CityHall.Exceptions;
using CityHall.Responses;
using Ninject;
using RestSharp;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using CityHallNinject = CityHall.Config.Ninject;

namespace CityHall.Synchronous
{
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
            string fullLocation = string.Format(location, args);
            IRestRequest request = new RestRequest(fullLocation, Method.POST) { RequestFormat = DataFormat.Json };
            request.AddBody(data);
            return this.Execute<T>(request);
        }

        private T Get<T>(string location, params object[] args)
            where T : BaseResponse, new()
        {
            string fullLocation = string.Format(location, args);
            IRestRequest request = new RestRequest(fullLocation, Method.GET) { RequestFormat = DataFormat.Json };
            return this.Execute<T>(request);
        }

        private BaseResponse Delete(string location, params object[] args)
        {
            string fullLocation = string.Format(location, args);
            IRestRequest request = new RestRequest(fullLocation, Method.DELETE) { RequestFormat = DataFormat.Json };
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
                    BaseResponse delete = this.Delete("auth/");
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
    }
}
