using System.Threading.Tasks;
using CityHallEnvironment = CityHall.Data.EnvironmentInfo;
using CityHallUser = CityHall.Data.User;
using CityHallRights = CityHall.Data.Rights;

namespace CityHall
{
    public interface IAuthorization
    {
        /*
         * Creates a session to `url` using `user` and plaintext password `password`
         */
        Task<bool> Login(string url, string user, string password);

        /*
         * Logs out of the current session
         */
        Task Logout();

        /*
         * Gets the default environment for this user
         */
        Task<string> GetDefaultEnvironment();

        /*
         * Sets the default environment for this user
         */
        Task SetDefaultEnvironment(string environment);

        /*
         * Gets information about the given environment
         */
        Task<CityHallEnvironment> GetEnvironment(string environment);

        /*
         * Create an environment
         */
        Task CreateEnvironment(string environment);

        /*
         * Gets information about a user
         */
        Task<CityHallUser> GetUser(string user);

        /*
         * Creates a user with the given password.
         * Note that neither rights nor a default environment are assigned.
         */
        Task CreateUser(string user, string password);

        /*
         * Updates the current user's password
         */
        Task UpdatePassword(string password);

        /*
         * Deletes a user
         */
        Task DeleteUser(string user);

        /*
         * Grant user rights on environment
         */
        Task GrantRights(string user, CityHallRights rights, string environment);
    }
}