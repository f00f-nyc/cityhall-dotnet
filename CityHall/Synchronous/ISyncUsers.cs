using CityHall.Data;

namespace CityHall.Synchronous
{
    /// <summary>
    /// This class is meant for interfacing with users: creating, deleting, granting rights, etc.
    /// In broad terms, this refers to the auth/user/ and auth/grant section of the City Hall API.
    /// </summary>
    public interface ISyncUsers
    {
        /// <summary>
        /// Returns user info: the environments this user has permissions to, and what those permissions are
        /// </summary>
        /// <param name="username">The user to query</param>
        /// <returns></returns>
        UserInfo Get(string username);

        /// <summary>
        /// Create a user with neither permissions nor default environment.
        /// </summary>
        /// <param name="userName">The user to create, cannot be this.User</param>
        /// <param name="password">The plaintext password to set, it will be hashed before being passed across the wire</param>
        void CreateUser(string userName, string password);

        /// <summary>
        /// Deletes a user. Note that this call requires special permissions:
        ///   Either already have Grant permissions to all of that user's environments
        ///   or have Write permissions to the 'users' environment.
        /// Without these, the call will likely generate an ErrorFromCityHallException
        /// </summary>
        /// <param name="userName">The user to delete</param>
        void DeleteUser(string userName);

        /// <summary>
        /// Grant `userName` specific `rights` to `environment`. Note that if you want to revoke rights,
        /// you can Rights.None
        /// </summary>
        /// <param name="userName">The user to set, this may be the current logged in user</param>
        /// <param name="environment">The environment to grant to</param>
        /// <param name="rights">The new permission level on that environment</param>
        void Grant(string userName, string environment, Rights rights);
    }
}
