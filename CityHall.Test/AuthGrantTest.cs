using CityHall.Data;
using CityHall.Responses;
using NUnit.Framework;
using RestSharp;

namespace CityHall.Test
{
    [TestFixture]
    public class AuthGrantTest
    {
        /// <summary>
        /// Should be able to grant a user rights to another
        /// </summary>
        [Test]
        public void GrantRightsToUser()
        {
            var resource = "auth/user/test_user/";
            var expectedJson = "{\"env\":\"dev\",\"user\":\"a_new_user\",\"rights\":2}";
            var settings = TestSetup.SetupCall(TestSetup.Responses.UserInfo, Method.POST, resource, expectedJson);
            settings.Grant("a_new_user", "dev", Rights.ReadProtected);
            TestSetup.ErrorResponseHandled<BaseResponse>(() => settings.Grant("a_new_user", "dev", Rights.ReadProtected));
            TestSetup.LoggedOutHonored(settings, () => settings.Grant("a_new_user", "dev", Rights.ReadProtected));
        }
    }
}
