using CityHall.Exceptions;
using CityHall.Responses;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;

namespace CityHall.Synchronous
{
    /// <summary>
    /// Allow adding of multiple query string parameters to an IRestRequest
    /// </summary>
    internal static class IRestRequestExt
    {
        public static void AddParameters(this IRestRequest request, Dictionary<string, string> args)
        {
            foreach (KeyValuePair<string, string> arg in args ?? new Dictionary<string, string>())
            {
                request.AddParameter(arg.Key, arg.Value, ParameterType.QueryString);
            }
        }
    }

    /// <summary>
    /// This class is responsible for the calls to the server, as well as maintaining the session with the server.
    /// </summary>
    internal class SyncClient
    {
        public SyncClient(IRestClient client, string url)
        {
            this.client = client;
            this.client.BaseUrl = new Uri(url);
            this.client.CookieContainer = new CookieContainer();
        }

        private IRestClient client;

        protected T Execute<T>(IRestRequest request)
            where T : BaseResponse, new()
        {
            var response = this.client.Execute<T>(request);

            if (response == null)
            {
                throw new ErrorFromCityHallException(string.Format("Did not receive a response back from server for: method = {0}, resource = {1}", request.Method, request.Resource));
            }

            if (response.Data == null)
            {
                throw new ErrorFromCityHallException(string.Format("Did not receive data back from City Hall. Status Code: {0}", response.StatusCode));
            }

            if (!response.Data.IsValid)
            {
                throw new ErrorFromCityHallException(response.Data.Message);
            }

            return response.Data;
        }

        public T Post<T>(object data, string location, params object[] args)
            where T : BaseResponse, new()
        {
            return this.PostWithParameters<T>(data, string.Format(location, args));
        }

        public T PostWithParameters<T>(object data, string location, Dictionary<string, string> args = null)
            where T : BaseResponse, new()
        {
            IRestRequest request = new RestRequest(location, Method.POST) { RequestFormat = DataFormat.Json };
            request.AddBody(data);
            request.AddParameters(args);
            return this.Execute<T>(request);
        }

        public T Get<T>(string location, params object[] args)
            where T : BaseResponse, new()
        {
            return this.GetWithParameters<T>(string.Format(location, args));
        }

        public T GetWithParameters<T>(string location, Dictionary<string, string> args = null)
            where T : BaseResponse, new()
        {
            IRestRequest request = new RestRequest(location, Method.GET) { RequestFormat = DataFormat.Json };
            request.AddParameters(args);
            return this.Execute<T>(request);
        }

        public BaseResponse DeleteFormat(string location, params object[] args)
        {
            return this.DeleteWithParameters(string.Format(location, args));
        }

        public BaseResponse DeleteWithParameters(string location, Dictionary<string, string> args = null)
        {
            IRestRequest request = new RestRequest(location, Method.DELETE) { RequestFormat = DataFormat.Json };
            request.AddParameters(args);
            return this.Execute<BaseResponse>(request);
        }

        public BaseResponse Put(object data, string format, params object[] args)
        {
            string location = string.Format(format, args);
            IRestRequest request = new RestRequest(location, Method.PUT) { RequestFormat = DataFormat.Json };
            request.AddBody(data);
            return this.Execute<BaseResponse>(request);
        }
    }
}
