using CityHall.Config;
using NUnit.Framework;
using System.Configuration;
using Moq;
using Ninject;
using RestSharp;
using CityHall.Responses;

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

            public static ValueResponse Value(string value)
            {
                return new ValueResponse
                {
                    Response = "Ok",
                    value = value
                };
            }
        }

        public static void Response<T>(T value)
            where T : new()
        {
            var mockClient = new Mock<IRestClient>();
            var mockResponse = new Mock<IRestResponse<T>>();
            mockResponse.Setup(r => r.Data).Returns(value);
            mockClient.Setup(m => m.Execute<T>(It.IsAny<IRestRequest>())).Returns(mockResponse.Object);
            CityHall.Config.Ninject.Kernel.Rebind<IRestClient>().ToConstant(mockClient.Object);
        }

        public static void Response<T1, T2>(T1 value1, T2 value2)
            where T1 : new()
            where T2 : new()
        {            
            var mockResponse1 = new Mock<IRestResponse<T1>>();
            mockResponse1.Setup(r => r.Data).Returns(value1);

            var mockResponse2 = new Mock<IRestResponse<T2>>();
            mockResponse2.Setup(r => r.Data).Returns(value2);

            var mockClient = new Mock<IRestClient>();
            mockClient.Setup(m => m.Execute<T1>(It.IsAny<IRestRequest>())).Returns(mockResponse1.Object);
            mockClient.Setup(m => m.Execute<T2>(It.IsAny<IRestRequest>())).Returns(mockResponse2.Object);

            CityHall.Config.Ninject.Kernel.Rebind<IRestClient>().ToConstant(mockClient.Object);
        }

        [SetUp]
        public void SetUp()
        {
            // Ensure that the IoC is always called
            CityHall.Config.Ninject.RegisterIoc();

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
