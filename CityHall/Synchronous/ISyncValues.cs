using CityHall.Data;
using CityHall.Responses;
using System.Collections.Generic;

namespace CityHall.Synchronous
{
    public interface ISyncValues
    {
        /*
            * Gets the value of path with override over on environment env
            *
            * If over is unspecified, then the override that matches the logged
            * in name is retrieved. Otherwise the default value is returned.
            * 
            * If env is null or empty, then the default environment is assumed.
            */
        string GetValue(string path, string environment = null, string over = null);

        /// <summary>
        /// This is provided in case you need a lower-level access to getting the values.
        /// 
        /// Note that most of these functions are covered by other functions in this interface,
        /// so this shouldn't be used most of the time.
        /// </summary>
        /// <typeparam name="T">The expected BaseResponse. One of: ValueResponse, ChildrenResponse, HistoryResponse</typeparam>
        /// <param name="environment">The environment to query</param>
        /// <param name="path">The full path of the value</param>
        /// <param name="args">Arguments in key:value configuration. One of: 
        ///   "override" which can be set to an empty string to specifically get the default value
        ///   "viewhistory" set to "true" in order to retrieve a HistoryResponse
        ///   "viewchildren" set to "true" in order to retrieve a ChildrenResponse
        /// </param>
        /// <returns>A BaseResponse of type T</returns>
        T GetRaw<T>(string environment, string path, Dictionary<string, string> args)
            where T : BaseResponse, new();

        /*
            * Gets the history of path with override over on environment env
            *
            * If over is unspecified, then the override that matches the logged
            * in name is retrieved. Otherwise the default value is returned.
            * 
            * If env is null or empty, then the default environment is assumed.
            */
        History GetHistory(string path, string environment = null, string over = null);

        /*
            * Gets the list of children of path on environment env
            * 
            * If env is null or empty, then the default environment is assumed.
            */
        Children GetChildren(string path, string environment = null);


        /// <summary>
        /// This is provided in case you need a lower-level access to setting values.
        /// By taking a Value class, you can set both a value and protect bit together.
        /// 
        /// The intention is for the User to make use of Set() and SetProtect() most of the time.
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <param name="over"></param>
        void SetRaw(string environment, string path, Value value, string over = null);

        /*
            * Sets the value of env/path/ to value.
            * 
            * If over is unspecified, then the override that matches the logged
            * in name is retrieved. Otherwise the default value is returned.
            * 
            * If env is null or empty, then the default environment is assumed.
            */
        void Set(string environment, string path, string value, string over = null);

        /*
            * Sets the protect bit of env/path/ to protect.
            * 
            * If over is unspecified, then the override that matches the logged
            * in name is retrieved. Otherwise the default value is returned.
            * 
            * If env is null or empty, then the default environment is assumed.
            */
        void SetProtect(string environment, string path, bool protect, string over = null);

        void Delete(string environment, string path, string over = null);
    }
}
