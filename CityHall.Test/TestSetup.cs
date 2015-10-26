using CityHall.Config;
using NUnit.Framework;
using System.Configuration;
using Moq;
using Ninject;
using RestSharp;
using CityHall.Responses;
using System;
using CityHall.Exceptions;

namespace CityHall.Test
{
    [SetUpFixture]
    public class TestSetup
    {
        public static CityHallConfigSection Config
        {
            get
            {
                return new CityHallConfigSection() 
                { 
                    Url = "http://not.a.real.url/api/",
                    User = "some_user", 
                    Password = "some_password" 
                };
            }
        }

        public class Responses
        {
            public static BaseResponse Ok { get { return new BaseResponse { Response = "Ok" }; } }

            public static BaseResponse NotOk { get { return new BaseResponse { Response = "Failure", Message = "An error has occurred" }; } }

            public static ValueResponse Value(string value)
            {
                return new ValueResponse
                {
                    Response = "Ok",
                    value = value
                };
            }
        }

        public static Mock<IRestClient> Response<T>(T value)
            where T : new()
        {
            var mockClient = new Mock<IRestClient>();
            var mockResponse = new Mock<IRestResponse<T>>();
            mockResponse.Setup(r => r.Data).Returns(value);
            mockClient.Setup(m => m.Execute<T>(It.IsAny<IRestRequest>())).Returns(mockResponse.Object);
            
            CityHall.Config.Ninject.Kernel.Rebind<IRestClient>().ToConstant(mockClient.Object);
            CityHall.Config.Ninject.Kernel.Rebind<Mock<IRestClient>>().ToConstant(mockClient);
            
            return mockClient;
        }

        public static Mock<IRestClient> Response<T1, T2>(T1 value1, T2 value2)
            where T1 : new()
            where T2 : new()
        {
            var mockClient = TestSetup.Response<T1>(value1);
            var mockResponse2 = new Mock<IRestResponse<T2>>();
            mockResponse2.Setup(r => r.Data).Returns(value2);
            mockClient.Setup(m => m.Execute<T2>(It.IsAny<IRestRequest>())).Returns(mockResponse2.Object);
            return mockClient;
        }

        public static void ErrorResponseHandled(Action call)
        {
            var mockClient = CityHall.Config.Ninject.Kernel.Get<Mock<IRestClient>>();
            var badResponse = new Mock<IRestResponse<BaseResponse>>();
            badResponse.Setup(r => r.Data).Returns(TestSetup.Responses.NotOk);
            mockClient.Setup(m => m.Execute<BaseResponse>(It.IsAny<IRestRequest>())).Returns(badResponse.Object);
            Assert.Throws<ErrorFromCityHallException>(() => call());
        }

        public static void LoggedOutHonored(CityHall.Synchronous.ISettings settings, Action call)
        {
            var mockClient = CityHall.Config.Ninject.Kernel.Get<Mock<IRestClient>>();
            var logoutOkay = new Mock<IRestResponse<BaseResponse>>();
            logoutOkay.Setup(r => r.Data).Returns(TestSetup.Responses.Ok);
            mockClient.Setup(m => m.Execute<BaseResponse>(It.IsAny<IRestRequest>())).Returns(logoutOkay.Object);
            settings.Logout();

            Assert.Throws<NotLoggedInException>(() => call());
        }

        [SetUp]
        public void SetUp()
        {
            // Ensure that the IoC is always called
            CityHall.Config.Ninject.RegisterIoc();
            CityHall.Config.Ninject.Kernel.Bind<Mock<IRestClient>>().ToConstant(new Mock<IRestClient>());

            // Ensure that the config section of the app config exists
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.Sections.Get("cityhall") == null)
            {
                config.Sections.Add("cityhall", TestSetup.Config);
            }
            else
            {
                config.Sections.Remove("cityhall");
                config.Sections.Add("cityhall", TestSetup.Config);
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("cityhall");
        }
    }
}
