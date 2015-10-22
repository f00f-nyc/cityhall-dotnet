using System.Collections.Generic;
using System.Threading.Tasks;
using Child = CityHall.Data.Child;
using CityHallHistory = CityHall.Data.History;

namespace CityHall
{
    public interface IValues
    {
        /*
         * Gets the value of path with override over on environment env
         *
         * If over is unspecified, then the override that matches the logged
         * in name is retrieved. Otherwise the default value is returned.
         * 
         * If env is null or empty, then the default environment is assumed.
         */
        Task<string> Get(string path, string over = null, string env = null);

        /*
         * This returns the raw JSON from the server for env/path/
         * 
         * Note that most of these functions are covered by other functions here,
         * so this shouldn't be used most of the time.
         * 
         * You can specify parameters by name in the parameters dictionary:
         *    override: [name] - will retrieve the override for env/path/
         *    viewhistory: True - will retrieve the history for env/path/
         *    viewchildren: True - will retrieve the children for env/path/
         */
        Task<string> GetJson(string env, string path, Dictionary<string,string> parameters);

        /*
         * Gets the history of path with override over on environment env
         *
         * If over is unspecified, then the override that matches the logged
         * in name is retrieved. Otherwise the default value is returned.
         * 
         * If env is null or empty, then the default environment is assumed.
         */
        Task<CityHallHistory[]> GetHistory(string path, string over = null, string env = null);

        /*
         * Gets the list of children of path on environment env
         * 
         * If env is null or empty, then the default environment is assumed.
         */
        Task<Child[]> GetChildren(string path, string env = null);

        /*
         * This sets the raw JSON for env/path/ to payload, which should be a valid JSON
         * 
         * Note that most of these functions are covered by other functions here,
         * so this shouldn't be used most of the time.
         * 
         * Optionally, you can specify the override as over.  Note here that not
         * specifying it will not default to the override named as the logged
         * in user, it will simply set the global default.
         */
        Task SetJson(string env, string path, string payload, string over = null);

        /*
         * Sets the value of env/path/ to value.
         * 
         * If over is unspecified, then the override that matches the logged
         * in name is retrieved. Otherwise the default value is returned.
         * 
         * If env is null or empty, then the default environment is assumed.
         */
        Task Set(string path, string value, string over = null, string env = null);

        /*
         * Sets the protect bit of env/path/ to protect.
         * 
         * If over is unspecified, then the override that matches the logged
         * in name is retrieved. Otherwise the default value is returned.
         * 
         * If env is null or empty, then the default environment is assumed.
         */
        Task SetProtect(string path, bool protect, string over = null, string env = null);
    }
}
