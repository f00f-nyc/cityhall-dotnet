using CityHall.Data;
using CityHall.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CityHall
{
    /// <summary>
    /// This class is meant for interfacing with values: getting, setting, or deleting.
    /// In broad terms, this refers to the /env/ section of the City Hall API.
    /// </summary>
    public interface IValues
    {
        /// <summary>
        /// This is a duplicated method from ISyncSettings, only here for completeness sake.
        /// 
        /// Retrieves the value from the server. If the value doesn't exist, or you don't have access to it, returns null.
        /// </summary>
        /// <param name="path">The path of the value. Starting or trailing '/' may be omitted.</param>
        /// <param name="environment">If it is null or empty, it will use ISyncSettings.DefaultEnvironmnet</param>
        /// <param name="over">The override to get. If this is unspecified, then the override that matches
        /// the logged in user is retrieved.  Otherwise, the default value is returned.</param>
        /// <returns></returns>
        Task<string> Get(string path, string environment = null, string over = null);

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
        Task<T> GetRaw<T>(string environment, string path, Dictionary<string, string> args)
            where T : BaseResponse, new();

        /// <summary>
        /// Gets the history of path with override over on environment env
        /// </summary>
        /// <param name="path">The path of the value. Starting or trailing '/' may be omitted.</param>
        /// <param name="environment">If it is null or empty, it will use ISyncSettings.DefaultEnvironmnet</param>
        /// <param name="over">The override to get. If this is unspecified, then the override that matches
        /// the logged in user is retrieved.  Otherwise, the default value is returned.</param>
        /// <returns></returns>
        Task<History> GetHistory(string path, string over, string environment = null);

        /// <summary>
        /// Gets the list of children of path on environment env
        /// </summary>        /// <param name="path">The path of the value. Starting or trailing '/' may be omitted.</param>
        /// <param name="environment">If it is null or empty, it will use ISyncSettings.DefaultEnvironmnet</param>
        /// <returns></returns>
        Task<Children> GetChildren(string path, string environment = null);

        /// <summary>
        /// This is provided in case you need a lower-level access to setting values.
        /// By taking a Value class, you can set both a value and protect bit together.
        /// 
        /// The intention is for the User to make use of Set() and SetProtect() most of the time.
        /// </summary>      
        /// <param name="path">The path of the value. Starting or trailing '/' may be omitted.</param>
        /// <param name="environment">The environment for the path, this may not be omitted</param>
        /// <param name="value">The new Value to set 'path' to</param>
        /// <param name="over">The override to set. This may not be omitted.</param>
        Task SetRaw(string environment, string path, Value value, string over);

        /// <summary>
        /// Sets the value of env/path/ to value.
        /// </summary>      
        /// <param name="environment">The environment for the path, this may not be omitted</param>
        /// <param name="path">The path of the value. Starting or trailing '/' may be omitted.</param>
        /// <param name="value">The new value to set 'path' to</param>
        /// <param name="over">The override to set. This may not be omitted.</param>
        Task Set(string environment, string path, string value, string over);

        /// <summary>
        /// Sets the protect bit of env/path/ to protect.
        /// </summary>      
        /// <param name="environment">The environment for the path, this may not be omitted</param>
        /// <param name="path">The path of the value. Starting or trailing '/' may be omitted.</param>
        /// <param name="protect">The new value to of the protect bit</param>
        /// <param name="over">The override to set. This may not be omitted.</param>
        Task SetProtect(string environment, string path, bool protect, string over);

        /// <summary>
        /// Deletes the value at env/path/
        /// </summary>      
        /// <param name="environment">The environment for the path, this may not be omitted</param>
        /// <param name="path">The path of the value. Starting or trailing '/' may be omitted.</param>
        /// <param name="over">The override to set. This may not be omitted.</param>
        Task Delete(string environment, string path, string over);
    }
}
