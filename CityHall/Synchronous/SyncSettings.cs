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
                this.DefaultEnvironment = this.Get<ValueResponse>("auth/user/{0}/default/", user).value;
            }
        }

        private IRestClient client;
        public string DefaultEnvironment { get; protected set; }

        # region REST requests
        private T Post<T>(object data, string location, params object[] args)
            where T : BaseResponse, new()
        {
            string fullLocation = string.Format(location, args);
            IRestRequest request = new RestRequest(fullLocation, Method.POST) { RequestFormat = DataFormat.Json };
            request.AddBody(data);
            var response = this.client.Execute<T>(request);
            return response.Data;
        }

        private T Get<T>(string location, params object[] args)
            where T : BaseResponse, new()
        {
            string fullLocation = string.Format(location, args);
            IRestRequest request = new RestRequest(fullLocation, Method.GET) { RequestFormat = DataFormat.Json };
            var response = this.client.Execute<T>(request);
            return response.Data;
        }
        #endregion

        #region CityHall.Synchronous.ISettings

        public CityHall.ISettings AsynchronousSettings()
        {
            throw new NotImplementedException();
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
