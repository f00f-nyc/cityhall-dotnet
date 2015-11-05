using CityHall.Config;
using NUnit.Framework;
using System.Configuration;
using Moq;
using Ninject;
using RestSharp;
using CityHall.Responses;
using System;
using CityHall.Exceptions;
using System.Collections.Generic;
using System.Linq;

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

            public static ValueResponse Val1 { get { return Responses.Value("val1"); } }

            public static ValueResponse DefaultEnvironment { get { return Responses.Value("dev"); } }
            
            public static EnvironmentResponse DevEnvironment
            {
                get
                {
                    return new EnvironmentResponse
                    {
                        Response = "Ok",
                        Users = new Dictionary<string,int>
                        {
                            { "test_user", 4 },
                            { "some_user", 1 },
                            { "cityhall", 4 }
                        }
                    };
                }
            }

            public static UserInfoResponse UserInfo
            {
                get
                {
                    return new UserInfoResponse
                    {
                        Response = "Ok",
                        Environments = new Dictionary<string, int>
                        {
                            {"dev", 4}, 
                            {"auto", 1}, 
                            {"users", 1}
                        }
                    };
                }
            }

            public static ChildrenResponse ChildrenResponse
            {
                get
                {
                    return new ChildrenResponse
                    {
                        Response = "Ok",
                        children = new List<ChildResponse>()
                        {
                            new ChildResponse
                            {
                                id = 302,
                                name = "value1",
                                @override = "",
                                path = "/app1/domainA/feature_1/value1/",
                                protect = false,
                                value = "1000"
                            },
                            new ChildResponse
                            {
                                id = 552,
                                name = "value1",
                                @override = TestSetup.Config.User,
                                path = "/app1/domainA/feature_1/value1/",
                                protect = false,
                                value = "2"
                            }
                        },
                        path = "/app1/domainA/feature_1/"
                    };
                }
            }

            public static HistoryResponse HistoryResponse
            {
                get
                {
                    return new HistoryResponse
                    {
                        Response = "Ok",
                        History = new List<LogEntry>
                        {
                            new LogEntry
                            {
                                active = false,
                                @override = "",
                                id = 12,
                                value = "999",
                                datetime = DateTime.Now.AddMinutes(-1.0),
                                protect = false,
                                name = "value1",
                                author = TestSetup.Config.User
                            },
                            new LogEntry
                            {
                                active = true,
                                @override = "",
                                id = 12,
                                value = "1000",
                                datetime = DateTime.Now,
                                protect = false,
                                name = "value1",
                                author = TestSetup.Config.User
                            }
                        }
                    };
                }
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

        public static Synchronous.ISyncSettings SetupCall<T>(
            T value, 
            Method method, 
            string resource = null, 
            string expectedJson = null,
            Dictionary<string, string> args = null
        )
            where T : BaseResponse, new()
        {
            var mockClient = TestSetup.Response(Responses.Ok, Responses.DefaultEnvironment);
            var settings = Synchronous.SyncSettings.Get();

            var mockResponse = new Mock<IRestResponse<T>>();
            mockResponse.Setup(r => r.Data).Returns(value);

            mockClient.Setup(c => c.Execute<T>(It.IsAny<IRestRequest>()))
                .Callback<IRestRequest>(r => {
                        Assert.AreEqual(method, r.Method);

                        if (!string.IsNullOrEmpty(resource))
                        {
                            Assert.AreEqual(resource, r.Resource);
                        }

                        if (!string.IsNullOrEmpty(expectedJson))
                        {
                            Assert.AreEqual(expectedJson, r.Parameters[0].Value);
                        }

                        foreach (KeyValuePair<string, string> kv in args ?? new Dictionary<string, string>())
                        {
                            Parameter param = r.Parameters.FirstOrDefault(p => p.Name.Equals(kv.Key));
                            Assert.NotNull(param, string.Format("Expected to find a param {0}:{1} in the list of params for [{2}@{3}]", kv.Key, kv.Value, r.Method, r.Resource));
                            Assert.AreEqual(param.Value, kv.Value);
                        }
                    })
                   .Returns(mockResponse.Object);
            
            return settings;
        }

        public static void ErrorResponseHandled<T>(Action call)
            where T : BaseResponse, new()
        {
            var mockClient = CityHall.Config.Ninject.Kernel.Get<Mock<IRestClient>>();
            var badResponse = new Mock<IRestResponse<T>>();
            T response = new T() { Response = TestSetup.Responses.NotOk.Response, Message = "Call has failed" };
            badResponse.Setup(r => r.Data).Returns(response);
            mockClient.Setup(m => m.Execute<T>(It.IsAny<IRestRequest>())).Returns(badResponse.Object);
            Assert.Throws<ErrorFromCityHallException>(() => call());
        }

        public static void LoggedOutHonored(CityHall.Synchronous.ISyncSettings settings, Action call)
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
