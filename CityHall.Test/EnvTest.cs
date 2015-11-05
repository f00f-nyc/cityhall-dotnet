using CityHall.Data;
using CityHall.Responses;
using NUnit.Framework;
using RestSharp;
using System.Collections.Generic;
using System.Linq;

namespace CityHall.Test
{
    [TestFixture]
    public class EnvTest
    {
        [Test]
        public void GetRawWorksNoParameters()
        {
            var settings = TestSetup.SetupCall<ValueResponse>(TestSetup.Responses.Val1, Method.GET, "env/dev/value1/");
            var value = settings.Values.GetRaw<ValueResponse>("dev", "value1", null);

            TestSetup.ErrorResponseHandled<ValueResponse>(() => settings.Values.GetRaw<ValueResponse>("dev", "value1", null));
            TestSetup.LoggedOutHonored(settings, () => settings.Values.GetRaw<ValueResponse>("dev", "value1", null));

            Assert.AreEqual(value.Response, TestSetup.Responses.Val1.Response);
            Assert.AreEqual(value.value, TestSetup.Responses.Val1.value);
            Assert.AreEqual(value.Message, TestSetup.Responses.Val1.Message);
        }

        [Test]
        public void GetRawWorksWithParams()
        {
            var settings = TestSetup.SetupCall<ValueResponse>(TestSetup.Responses.Val1, Method.GET, args: new Dictionary<string, string> { { "override", "cityhall" } });
            settings.Values.GetRaw<ValueResponse>("dev", "value1", new Dictionary<string, string> { { "override", "cityhall" } });
        }

        [Test]
        public void GetValue()
        {
            string location = string.Format("env/{0}/value1/", TestSetup.Responses.DefaultEnvironment.value);
            var settings = TestSetup.SetupCall<ValueResponse>(TestSetup.Responses.Val1, Method.GET, location);

            var value = settings.GetValue("value1"); 
            TestSetup.ErrorResponseHandled<ValueResponse>(() => settings.GetValue("value1"));
            TestSetup.LoggedOutHonored(settings, () => settings.GetValue("value1"));
            Assert.AreEqual(TestSetup.Responses.Val1.value, value);
        }

        [Test]
        public void GetValueOverride()
        {
            string location = string.Format("env/{0}/value1/", TestSetup.Responses.DefaultEnvironment.value);
            var args = new Dictionary<string, string>() { { "override", "cityhall" } };
            var settings = TestSetup.SetupCall<ValueResponse>(TestSetup.Responses.Val1, Method.GET, location, args: args);

            var value = settings.GetValue("value1", over: "cityhall");
            Assert.AreEqual(TestSetup.Responses.Val1.value, value);
        }

        [Test]
        public void GetValueEnvironment()
        {
            string location = "env/qa/value1/";
            var settings = TestSetup.SetupCall<ValueResponse>(TestSetup.Responses.Val1, Method.GET, location);

            var value = settings.GetValue("value1", environment: "qa");
            Assert.AreEqual(TestSetup.Responses.Val1.value, value);
        }

        [Test]
        public void GetValueEnvironmentOverride()
        {
            string location = "env/qa/value1/";
            var args = new Dictionary<string, string> { { "override", "cityhall" } };
            var settings = TestSetup.SetupCall<ValueResponse>(TestSetup.Responses.Val1, Method.GET, location, args: args);

            var value = settings.GetValue("value1", environment: "qa", over: "cityhall");
            Assert.AreEqual(TestSetup.Responses.Val1.value, value);
        }

        [Test]
        public void GetChildren()
        {
            string location = "env/dev/value1/";
            var args = new Dictionary<string, string> { { "viewchildren", "true" } };
            var settings = TestSetup.SetupCall<ChildrenResponse>(TestSetup.Responses.ChildrenResponse, Method.GET, location, args: args);

            var children = settings.Values.GetChildren("value1");
            TestSetup.ErrorResponseHandled<ChildrenResponse>(() => settings.Values.GetChildren("value1"));
            TestSetup.LoggedOutHonored(settings, () => settings.Values.GetChildren("value1"));
            Assert.AreEqual(TestSetup.Responses.ChildrenResponse.children.Count(), children.SubChildren.Count());
            Assert.AreEqual(TestSetup.Responses.ChildrenResponse.path, children.Path);
        }

        [Test]
        public void GetChildrenEnvironment()
        {
            string location = "env/qa/value1/";
            var settings = TestSetup.SetupCall<ChildrenResponse>(TestSetup.Responses.ChildrenResponse, Method.GET, location);

            var children = settings.Values.GetChildren("value1", environment: "qa");
        }

        [Test]
        public void GetHistory()
        {
            string location = "env/dev/value1/";
            var args = new Dictionary<string, string> { { "viewhistory", "true" } };
            var settings = TestSetup.SetupCall<HistoryResponse>(TestSetup.Responses.HistoryResponse, Method.GET, location);

            var history = settings.Values.GetHistory("value1");
            TestSetup.ErrorResponseHandled<HistoryResponse>(() => settings.Values.GetHistory("value1"));
            TestSetup.LoggedOutHonored(settings, () => settings.Values.GetHistory("value1"));
            Assert.AreEqual(TestSetup.Responses.HistoryResponse.History.Count(), history.Entries.Count());
        }

        [Test]
        public void GetHistoryOverride()
        {
            string location = "env/dev/value1/";
            var args = new Dictionary<string, string> { { "viewhistory", "true" }, {"override", "cityhall"} };
            var settings = TestSetup.SetupCall<HistoryResponse>(TestSetup.Responses.HistoryResponse, Method.GET, location, args: args);

            var history = settings.Values.GetHistory("value1", over: "cityhall");
        }

        [Test]
        public void GetHistoryEnvironment()
        {
            string location = "env/qa/value1/";
            var settings = TestSetup.SetupCall<HistoryResponse>(TestSetup.Responses.HistoryResponse, Method.GET, location);
            settings.Values.GetHistory("value1", environment: "qa");
        }

        [Test]
        public void SetRawValue()
        {
            string location = "env/qa/value1/";
            string expectedJson = "{\"value\":\"some value\"}";
            var settings = TestSetup.SetupCall<BaseResponse>(TestSetup.Responses.Ok, Method.POST, location, expectedJson);

            settings.Values.SetRaw("qa", "value1", new Value("some value"));
            TestSetup.ErrorResponseHandled<BaseResponse>(() => settings.Values.SetRaw("qa", "value1", new Value("some value")));
            TestSetup.LoggedOutHonored(settings, () => settings.Values.SetRaw("qa", "value1", new Value("some value")));
        }

        [Test]
        public void SetRawProtect()
        {
            string location = "env/qa/value1/";
            string expectedJson = "{\"protect\":true}";
            var settings = TestSetup.SetupCall<BaseResponse>(TestSetup.Responses.Ok, Method.POST, location, expectedJson);

            settings.Values.SetRaw("qa", "value1", new Value(true));
        }

        [Test]
        public void SetRawValueProtect()
        {
            string location = "env/qa/value1/";
            string expectedJson = "{\"value\":\"some value\",\"protect\":true}";
            var settings = TestSetup.SetupCall<BaseResponse>(TestSetup.Responses.Ok, Method.POST, location, expectedJson);

            settings.Values.SetRaw("qa", "value1", new Value("some value", true));
        }

        [Test]
        public void SetRawOverride()
        {
            string location = "env/qa/value1/";
            Dictionary<string, string> args = new Dictionary<string,string> { { "override", "cityhall" } };
            var settings = TestSetup.SetupCall<BaseResponse>(TestSetup.Responses.Ok, Method.POST, location, args: args);

            settings.Values.SetRaw("qa", "value1", new Value("some value"), over: "cityhall");
        }


        [Test]
        public void Set()
        {
            string location = "env/qa/value1/";
            string expectedJson = "{\"value\":\"some value\"}";
            var settings = TestSetup.SetupCall<BaseResponse>(TestSetup.Responses.Ok, Method.POST, location, expectedJson);

            settings.Values.Set("qa", "value1", "some value");
            TestSetup.ErrorResponseHandled<BaseResponse>(() => settings.Values.Set("qa", "value1", "some value"));
            TestSetup.LoggedOutHonored(settings, () => settings.Values.Set("qa", "value1", "some value"));
        }

        [Test]
        public void SetOverride()
        {
            string location = "env/qa/value1/";
            Dictionary<string, string> args = new Dictionary<string, string> { { "override", "cityhall" } };
            var settings = TestSetup.SetupCall<BaseResponse>(TestSetup.Responses.Ok, Method.POST, location, args: args);

            settings.Values.Set("qa", "value1", "some_value", over: "cityhall");
        }

        [Test]
        public void SetProtect()
        {
            string location = "env/qa/value1/";
            string expectedJson = "{\"protect\":false}";
            var settings = TestSetup.SetupCall<BaseResponse>(TestSetup.Responses.Ok, Method.POST, location, expectedJson);

            settings.Values.SetProtect("qa", "value1", false);
        }

        [Test]
        public void SetProtectOverride()
        {
            string location = "env/qa/value1/";
            Dictionary<string, string> args = new Dictionary<string, string> { { "override", "cityhall" } };
            var settings = TestSetup.SetupCall<BaseResponse>(TestSetup.Responses.Ok, Method.POST, location, args: args);

            settings.Values.SetProtect("qa", "value1", false, over: "cityhall");
        }

        [Test]
        public void Delete()
        {
            string location = "env/qa/value1/";
            var settings = TestSetup.SetupCall<BaseResponse>(TestSetup.Responses.Ok, Method.DELETE, location);
            settings.Values.Delete("qa", "value1");
            TestSetup.ErrorResponseHandled<BaseResponse>(() => settings.Values.Delete("qa", "value1"));
            TestSetup.LoggedOutHonored(settings, () => settings.Values.Delete("qa", "value1"));
        }

        [Test]
        public void DeleteOverride()
        {
            string location = "env/qa/value1/";
            Dictionary<string, string> args = new Dictionary<string, string> { { "override", "cityhall" } };
            var settings = TestSetup.SetupCall<BaseResponse>(TestSetup.Responses.Ok, Method.DELETE, location, args: args);
            settings.Values.Delete("qa", "/value1/", over: "cityhall");
        }
    }
}
