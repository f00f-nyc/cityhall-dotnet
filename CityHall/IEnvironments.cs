using CityHall.Data;
using System.Threading.Tasks;

namespace CityHall
{    
    /// <summary>
    /// This class is meant for interfacing with environments: getting, creating, and default environments
    /// In broad terms, this refers to the auth/env/ section of the City Hall API.
    /// </summary>
    public interface IEnvironments
    {
        /// <summary>
        /// The default environment for this user.  If it is null, that means no default has been set 
        /// and subsequent calls which omit the environment variable will throw InvalidRequestException.
        /// </summary>
        string Default { get; }

        /// <summary>
        /// Sets the default environment.
        /// </summary>
        /// <param name="defaultEnvironment">The default environment to set it to</param>
        Task SetDefault(string defaultEnvironment);

        /// <summary>
        /// Gets Environment info: a list of users who are authorized on it, and their permission level.
        /// </summary>
        /// <param name="envName">The name of the environment to query</param>
        /// <returns></returns>
        Task<EnvironmentInfo> Get(string envName);

        /// <summary>
        /// Creates an environment. The user creating it will have Grant permissions for it.
        /// </summary>
        /// <param name="envName">The name of the environment to create, it must be unique.</param>
        Task Create(string envName);
    }
}
