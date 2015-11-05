using CityHall.Data;
using CityHall.Responses;
using System.Collections.Generic;
using CityHall;

namespace CityHall.Synchronous
{
    public interface ISyncSettings
    {
        /// <summary>
        /// Retrieves the value from the server. If the value doesn't exist, or you don't have access to it, returns null.
        /// </summary>
        /// <param name="path">The path of the value. Starting or trailing '/' may be omitted.</param>
        /// <param name="environment">If it is null or empty, it will use ISyncSettings.DefaultEnvironmnet</param>
        /// <param name="over">The override to get. If this is unspecified, then the override that matches
        /// the logged in user is retrieved.  Otherwise, the default value is returned.</param>
        /// <returns></returns>
        string GetValue(string path, string environment = null, string over = null);

        /// <summary>
        /// The Task-based implementation of this instance
        /// </summary>
        ISettings AsynchronousSettings { get; }

        /// <summary>
        /// The interface to use for managing values
        /// </summary>
        ISyncValues Values { get; }

        /// <summary>
        /// The interface to use for managing environments
        /// </summary>
        ISyncEnvironments Environments { get; }

        /// <summary>
        /// The interface to use for managing users
        /// </summary>
        ISyncUsers Users { get; }

        /// <summary>
        /// The current logged in user
        /// </summary>
        string User { get; }

        /// <summary>
        /// For updating the current, logged-in user's password
        /// </summary>
        /// <param name="password">The cleartext password, which will be hashed before being transferred across the wire</param>
        void UpdatePassword(string password);

        /// <summary>
        /// Logs out of the session. Subsequent calls to the server will throw NotLoggedInException
        /// This call will also kill the session on the server.
        /// It isn't a requirement to log out, sessions on the server will expire on their own
        /// </summary>
        void Logout();
    }
}
