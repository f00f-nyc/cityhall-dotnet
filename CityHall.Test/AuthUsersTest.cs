using CityHall.Exceptions;
using CityHall.Responses;
using NUnit.Framework;
using RestSharp;
using System.Linq;

namespace CityHall.Test
{
    [TestFixture]
    public class AuthUsersTest
    {
        /// <summary>
        /// Get information about a user, his permissions on all environments.
        /// </summary>
        public void GetUser()
        {
            var resource = "auth/user/test_user/";

            var settings = TestSetup.SetupCall(TestSetup.Responses.UserInfo, Method.GET, resource);
            var env = settings.Users.Get("test_user");
            TestSetup.ErrorResponseHandled<EnvironmentResponse>(() => settings.Users.Get("test_user"));
            TestSetup.LoggedOutHonored(settings, () => settings.Users.Get("test_user"));

            Assert.AreEqual(env.Permissions.Count(), TestSetup.Responses.UserInfo.Environments.Count(), "The data received from the call should be visible in the return value");
        }

        /// <summary>
        /// Creates a user with the given password.  
        /// The user, by default has no default environment and no rights to read any environment.
        /// </summary>
        [Test]
        public void CreateUser()
        {
            var resource = "auth/user/a_new_user/";
            var expectedJson = "{\"passhash\":\"\"}";
            var settings = TestSetup.SetupCall(TestSetup.Responses.Ok, Method.POST, resource, expectedJson);
            settings.Users.CreateUser("a_new_user", "");
            TestSetup.ErrorResponseHandled<BaseResponse>(() => settings.Users.CreateUser("a_new_user", ""));
            TestSetup.LoggedOutHonored(settings, () => settings.Users.CreateUser("a_new_user", ""));

            expectedJson = string.Format("{{\"passhash\":\"{0}\"}}", Synchronous.Password.Hash("password"));
            settings = TestSetup.SetupCall(TestSetup.Responses.Ok, Method.POST, resource, expectedJson);
            settings.Users.CreateUser("a_new_user", "password");

            Assert.Throws<InvalidRequestException>(() => settings.Users.CreateUser(TestSetup.Config.User, ""));
        }

        /// <summary>
        /// This will update your own password
        /// </summary>
        [Test]
        public void UpdatePassword()
        {
            var resource = string.Format("auth/user/{0}/", TestSetup.Config.User);
            var expectedJson = "{\"passhash\":\"\"}";
            var settings = TestSetup.SetupCall(TestSetup.Responses.Ok, Method.PUT, resource, expectedJson);
            settings.UpdatePassword("");
            TestSetup.ErrorResponseHandled<BaseResponse>(() => settings.UpdatePassword(""));
            TestSetup.LoggedOutHonored(settings, () => settings.UpdatePassword(""));

            expectedJson = string.Format("{{\"passhash\":\"{0}\"}}", Synchronous.Password.Hash("password"));
            settings = TestSetup.SetupCall(TestSetup.Responses.Ok, Method.PUT, resource, expectedJson);
            settings.UpdatePassword("password");
        }

        [Test]
        public void DeleteUser()
        {
            var resource = "auth/user/a_new_user/";
            var settings = TestSetup.SetupCall(TestSetup.Responses.Ok, Method.DELETE, resource);
            settings.Users.DeleteUser("a_new_user");
            TestSetup.ErrorResponseHandled<BaseResponse>(() => settings.Users.DeleteUser("a_new_user"));
            TestSetup.LoggedOutHonored(settings, () => settings.Users.DeleteUser("a_new_user"));

        }
    }
}
