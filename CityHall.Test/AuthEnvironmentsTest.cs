using CityHall.Responses;
using NUnit.Framework;
using RestSharp;
using System.Linq;

namespace CityHall.Test
{
    [TestFixture]
    public class AuthEnvironmentsTest
    {
        /// <summary>
        /// A user should be able to get details for an environment
        /// </summary>
        [Test]
        public void GetEnvironments()
        {
            var resource = "auth/env/dev/";

            var settings = TestSetup.SetupCall(TestSetup.Responses.DevEnvironment, Method.GET, resource);           
            var env = settings.Environments.Get("dev");
            TestSetup.ErrorResponseHandled<EnvironmentResponse>(() => settings.Environments.Get("dev"));
            TestSetup.LoggedOutHonored(settings, () => settings.Environments.Get("dev"));

            Assert.AreEqual(env.Rights.Count(), TestSetup.Responses.DevEnvironment.Users.Count(), "The data received from the call should be visible in the return value");
        }

        /// <summary>
        /// A user should be able to create an environment
        /// </summary>
        [Test]
        public void CreateEnvironment()
        {
            var resource = "auth/env/qa/";

            var settings = TestSetup.SetupCall(TestSetup.Responses.Ok, Method.POST, resource);
            settings.Environments.Create("qa");
            TestSetup.ErrorResponseHandled<BaseResponse>(() => settings.Environments.Create("qa"));
            TestSetup.LoggedOutHonored(settings, () => settings.Environments.Create("qa"));
        }
    }
}
