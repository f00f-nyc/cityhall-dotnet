using CityHall.Config;
using CityHall.Exceptions;
using CityHall.Responses;
using CityHall.Synchronous;
using Moq;
using NUnit.Framework;
using RestSharp;
using System;
using System.Configuration;
using System.Linq;

namespace CityHall.Test
{
    [TestFixture]
    public class SyncSettingsTest
    {
        private static void RemoveCityHallConfig()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.Sections.Remove("cityhall");
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("cityhall");
        }

        /// <summary>
        /// If the config section has been removed by one of the tests, the test tear down will restore it
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (!config.Sections.OfType<CityHallConfigSection>().Any())
            {
                config.Sections.Add("cityhall", TestSetup.Config);
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("cityhall");
            }
        }

        /// <summary>
        /// A user should be able to set url, username, and password in the config file, 
        /// then call SyncSettings.Get() to get a logged in instance. This is the entry point
        /// into CityHall.
        /// </summary>
        [Test]
        public void SettingsAreReturned()
        {
            TestSetup.Response(TestSetup.Responses.Ok, TestSetup.Responses.Value("dev"));
            Assert.IsNotNull(SyncSettings.Get());
            TestSetup.ErrorResponseHandled(() => SyncSettings.Get());
        }

        /// <summary>
        /// Attempting to get the settings without setting a URL in either the config or passing them to
        /// the function will raise a MissingConfigSection.
        /// </summary>
        [Test]
        public void AttemptingToGetSettingsWithoutConfigThrowsException()
        {
            SyncSettingsTest.RemoveCityHallConfig();
            Assert.Throws<MissingConfigSection>(() => SyncSettings.Get());
        }

        /// <summary>
        /// Make sure that the underlying hashing works for passwords.
        /// </summary>
        [Test]
        public void PasswordHashingWorks()
        {
            string password = "123";
            string hash = SyncSettings.Hash(password);

            Assert.AreNotEqual(password, hash);
            Assert.IsNotNullOrEmpty(hash);
        }

        /// <summary>
        /// If the username and password cannot be retrieved from anywhere, the convention is for the 
        /// library to try to log in with the machine name and no password.
        /// </summary>
        [Test]
        public void SettingsAssumeMachineNameAndNoPasswordIfNotPassedIn()
        {
            SyncSettingsTest.RemoveCityHallConfig();
            Mock<IRestClient> client = TestSetup.Response(TestSetup.Responses.Value("dev"));
            Mock<IRestResponse<BaseResponse>> mockAuth = new Mock<IRestResponse<BaseResponse>>();
            string expectedJson = string.Format("{{\"username\":\"{0}\",\"passhash\":\"\"}}", Environment.MachineName);

            mockAuth.Setup(r => r.Data).Returns(TestSetup.Responses.Ok);
            client.Setup(c => c.Execute<BaseResponse>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => Assert.AreEqual(r.Parameters[0].Value, expectedJson))
                .Returns(mockAuth.Object);

            SyncSettings.Get("http://some.url");
        }

        /// <summary>
        /// The URL in the config file is honored
        /// </summary>
        [Test]
        public void ConfigSectionIsHonoredUrl()
        {
            Mock<IRestClient> client = TestSetup.Response(TestSetup.Responses.Ok, TestSetup.Responses.Value("dev"));
            client.SetupSet(c => c.BaseUrl = It.IsAny<Uri>()).Callback<Uri>(u => Assert.AreEqual(TestSetup.Config.Url, u.AbsoluteUri));
            SyncSettings.Get();
        }

        /// <summary>
        /// The URL passed in to file overrides the config one.
        /// </summary>
        [Test]
        public void UrlPassedInOverridesConfig()
        {
            Mock<IRestClient> client = TestSetup.Response(TestSetup.Responses.Ok, TestSetup.Responses.Value("dev"));
            string different_url = "http://test.new.url/api";
            client.SetupSet(c => c.BaseUrl = It.IsAny<Uri>()).Callback<Uri>(u => Assert.AreEqual(different_url, u.AbsoluteUri));
            SyncSettings.Get(url: different_url);
        }

        /// <summary>
        /// The City Hall convention is, when there's no password, we pass an empty string as the password.
        /// </summary>
        [Test]
        public void EmptyPasswordIsHonored()
        {
            Mock<IRestClient> client = TestSetup.Response(TestSetup.Responses.Value("dev"));
            Mock<IRestResponse<BaseResponse>> mockAuth = new Mock<IRestResponse<BaseResponse>>();
            string expectedJson = "{\"username\":\"test\",\"passhash\":\"\"}";

            mockAuth.Setup(r => r.Data).Returns(TestSetup.Responses.Ok);
            client.Setup(c => c.Execute<BaseResponse>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => Assert.AreEqual(r.Parameters[0].Value, expectedJson))
                .Returns(mockAuth.Object);

            SyncSettings.Get(user: "test", password: "");
        }

        /// <summary>
        /// When a password is passed in, it should be done in cleartext.  The password should be hashed before it passed to City Hall
        /// </summary>
        [Test]
        public void PasswordIsHashed()
        {
            Mock<IRestClient> client = TestSetup.Response(TestSetup.Responses.Value("dev"));
            Mock<IRestResponse<BaseResponse>> mockAuth = new Mock<IRestResponse<BaseResponse>>();
            string expectedJson = string.Format("{{\"username\":\"test\",\"passhash\":\"\"}}", SyncSettings.Hash("test"));

            mockAuth.Setup(r => r.Data).Returns(TestSetup.Responses.Ok);
            client.Setup(c => c.Execute<BaseResponse>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => Assert.AreEqual(r.Parameters[0].Value, expectedJson))
                .Returns(mockAuth.Object);

            SyncSettings.Get(user: "test", password: "test");
        }

        /// <summary>
        /// Upon logging in, the default environment is retrieved and can be inspected.
        /// </summary>
        [Test]
        public void DefaultEnvironmentIsRetrieved()
        {
            TestSetup.Response(TestSetup.Responses.Ok, TestSetup.Responses.Value("dev"));
            var settings = SyncSettings.Get();
            Assert.AreEqual("dev", settings.DefaultEnvironment);
        }

        /// <summary>
        /// Logging out hits the correct url
        /// </summary>
        [Test]
        public void LoggingOutWorks()
        {
            var client = TestSetup.Response(TestSetup.Responses.Ok, TestSetup.Responses.Value("dev"));
            var mockOk = new Mock<IRestResponse<BaseResponse>>();
            var settings = SyncSettings.Get();

            mockOk.Setup(r => r.Data).Returns(TestSetup.Responses.Ok);
            client.Setup(c => c.Execute<BaseResponse>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => 
                    {
                        Assert.AreEqual(Method.DELETE, r.Method);
                        Assert.AreEqual("auth/", r.Resource);
                    })
                .Returns(mockOk.Object);
            
            settings.Logout();
            
            TestSetup.Response(TestSetup.Responses.Ok, TestSetup.Responses.Value("dev"));
            settings = SyncSettings.Get();
            TestSetup.ErrorResponseHandled(() => settings.Logout());
        }

        /// <summary>
        /// You should be able to set the default enviornment.
        /// This test is here for completeness sake, since retrieving the default 
        /// environment is done as part logging in.
        /// </summary>
        [Test]
        public void SettingDefaultEnvironment()
        {
            var client = TestSetup.Response(TestSetup.Responses.Ok, TestSetup.Responses.Value("dev"));
            var mockOk = new Mock<IRestResponse<BaseResponse>>();
            var settings = SyncSettings.Get();
            var resource = string.Format("auth/user/{0}/default/", TestSetup.Config.User);
            var expectedJson = "{\"env\":\"qa\"}";

            mockOk.Setup(r => r.Data).Returns(TestSetup.Responses.Ok);
            client.Setup(c => c.Execute<BaseResponse>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r =>
                {
                    Assert.AreEqual(Method.POST, r.Method);
                    Assert.AreEqual(resource, r.Resource);
                    Assert.AreEqual(expectedJson, r.Parameters[0].Value);
                })
                .Returns(mockOk.Object);

            settings.SetDefaultEnvironment("qa");
            TestSetup.ErrorResponseHandled(() => settings.SetDefaultEnvironment("qa"));
            TestSetup.LoggedOutHonored(settings, () => settings.SetDefaultEnvironment("qa"));
        }

        public void TestRestSharp()
        {
            var client = new RestClient();
            client.BaseUrl = new Uri("http://digitalborderlands.herokuapp.com/api/");

            RestRequest login = new RestRequest("auth/", Method.POST) { RequestFormat = DataFormat.Json };
            login.AddBody(new { username = "guest", passhash = "" });
            IRestResponse<BaseResponse> response = client.Execute<BaseResponse>(login);
        }

        public void TestRealCityHall()
        {
            var settings = SyncSettings.Get("http://digitalborderlands.herokuapp.com/api/", "test", "");
        }
    }
}
