using CityHall.Data;
using CityHall.Responses;
using System.Collections.Generic;
using CityHall;

namespace CityHall.Synchronous
{
    public interface ISyncSettings
    {
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
        /// If the user is logged in. In practical terms, this boolean hold whether or not Logout() has been succesfully called.
        /// </summary>
        bool LoggedIn { get; }

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
