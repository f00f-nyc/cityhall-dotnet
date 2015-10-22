using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CityHall.Config;
using CityHall.Synchronous;
using CityHall.Responses;
using RestSharp;
using CityHall.Exceptions;

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

        [Test]
        public void AttemptingToGetSettingsWithoutConfigThrowsException()
        {
            SyncSettingsTest.RemoveCityHallConfig();
            Assert.Throws<MissingConfigSection>(() => SyncSettings.Get());
        }

        [Test]
        public void SettingsAreReturned()
        {
            TestSetup.Response(TestSetup.Responses.Ok, TestSetup.Responses.Value("dev"));
            Assert.IsNotNull(SyncSettings.Get());
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

        [Test]
        public void PasswordHashingWorks()
        {
            string password = "123";
            string hash = SyncSettings.Hash(password);

            Assert.AreNotEqual(password, hash);
            Assert.IsNotNullOrEmpty(hash);
        }

        [Test]
        public void SettingsAssumeMachineNameAndNoPasswordIfNotPassedIn()
        {
            SyncSettingsTest.RemoveCityHallConfig();
            var settings = SyncSettings.Get("http://some.url");
            Assert.That(false);
        }
    }
}
