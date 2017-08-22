using Aphrodite.Models;
using Nancy;
using StreamServices;
using StreamServices.Buffer;
using System;
using System.Collections.Generic;    

namespace Aphrodite.Modules
{
    public class MainModule : NancyModule
    {
        public MainModule()
        {
            Get("", args => Root(args));
            Post("Home/AddDataStream", args => AddDataStream(args));
            Get("Home/LoadStreams", args => LoadStreams(args));
        }

        private object Root(dynamic o)
        {
            return View["Home/Index"];
        }

        private static T GetOwinEnvironmentValue<T>(IDictionary<string, object> env, string name, T defaultValue = default(T))
        {
            return env.TryGetValue(name, out object value) && value is T ? (T)value : defaultValue;
        }

        private object LoadStreams (dynamic o)
        {
            return View["Home/ChartElement", new List<StreamListener>()];
        }

        private object AddDataStream(dynamic o)
        {
            //string name, string configuration, string source, string dataType, int bufferSize, string bufferType

            Type type = Type.GetType("System." + Request.Form.dataType);
            BufferInvalidationType buffer = Request.Form.bufferType == "event" ? BufferInvalidationType.Events : BufferInvalidationType.Time;
            ServiceType service = (ServiceType)Enum.Parse(typeof(ServiceType), Request.Form.source);
            var configDictionary = GetConfiguration(Request.Form.configuration);
            StreamListener stream = new StreamListener(Request.Form.name, service, type, Request.Form.bufferSize, buffer, configDictionary);
            stream.StartListening();

            //List<StreamListener> streams = (List<StreamListener>)HttpContext.Application["streams"];
            //streams.Add(stream);

            var model = new List<StreamListener>() { stream };

            return View["Home/ChartElement", model];
        }

        private static Dictionary<string, object> GetConfiguration(string configString)
        {
            var config = configString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            Dictionary<string, object> configuration = new Dictionary<string, object>();
            foreach (var element in config)
            {
                if (element.Trim() != "")
                {
                    int i = element.IndexOf('=');
                    var key = element.Substring(0, i);
                    var value = element.Substring(i + 1, element.Length - i - 1);
                    configuration.Add(key, value);
                }
            }
            return configuration;
        }
    }
}