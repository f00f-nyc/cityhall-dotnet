using System.Threading.Tasks;
using SyncSettings = CityHall.Synchronous.ISettings;

namespace CityHall
{
    public interface ISettings
    {
        /*
         * This should be the most oft-used function to retrieve a value from the server
         */
        Task<string> Get(string path, string over = null, string env = null);

        /*
         * Get a synchronous version of these settings.
         */
        SyncSettings Synchronous { get; }

        /*
         * Use this field for logging in and user/environment management
         */
        IAuthorization Authorization { get; }

        /*
         * Use this field for getting and setting values
         */
        IValues Values { get; }
    }
}
