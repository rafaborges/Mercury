using System.Collections.Generic;
using System.Web.Mvc;
using Mercury.Models;
using System;
using StreamServices;
using StreamServices.Buffer;
using System.Linq;
using System.IO;
using System.Xml.Linq;

namespace Mercury.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // This will hold StreamListeners accross all instances of this controller
            HttpContext.Application["streams"] = new List<StreamListener>();
            return View();
        }

        /// <summary>
        /// This loader can be used in the future to always load a set of
        /// streams on start
        /// </summary>
        /// <returns></returns>
        public ActionResult LoadStreams()
        {            
            return PartialView("ChartElement", new List<StreamListener>());
        }

        /// <summary>
        /// Entrypoint for adding a new stream to the collection of streams
        /// </summary>
        /// <param name="name">A name for the stream</param>
        /// <param name="connectionString">the connection string for the stream</param>
        /// <param name="source">The service provider that will be uses</param>
        /// <returns>A partialView with the chart that will be displayed</returns>
        [HttpPost]
        public ActionResult AddDataStream(string name, string configuration, string source, string dataType, int bufferSize, string bufferType)
        {
            Type type = Type.GetType("System." + dataType);
            BufferInvalidationType buffer = bufferType == "event" ? BufferInvalidationType.Events : BufferInvalidationType.Time;
            ServiceType service = (ServiceType)Enum.Parse(typeof(ServiceType), source);
            var configDictionary = GetConfiguration(configuration);
            StreamListener stream = new StreamListener(name, service, type, bufferSize, buffer, configDictionary);
            stream.StartListening();
            List<StreamListener> streams = (List<StreamListener>)HttpContext.Application["streams"];
            streams.Add(stream);

            return PartialView("ChartElement", new List<StreamListener>() { stream });
        }

        private Dictionary<string, object> GetConfiguration(string configString)
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

        #region View Controls
        /// <summary>
        /// Stop listening to a given stream
        /// </summary>
        /// <param name="id">ID of a registered stream</param>
        [HttpPost]
        public void StopDataStream(Guid id)
        {
            List<StreamListener> streams = (List<StreamListener>)HttpContext.Application["streams"];
            streams.SingleOrDefault(s => s.ID == id)?.StopListening();
        }

        /// <summary>
        /// Start listening to a given stream
        /// </summary>
        /// <param name="id">ID of a registered stream</param>
        [HttpPost]
        public void StartDataStream(Guid id)
        {
            List<StreamListener> streams = (List<StreamListener>)HttpContext.Application["streams"];
            streams.SingleOrDefault(s => s.ID == id)?.StartListening();
        }

        /// <summary>
        /// Stop listening to all streams
        /// </summary>
        [HttpPost]
        public void StopAllStreams()
        {
            List<StreamListener> streams = (List<StreamListener>)HttpContext.Application["streams"];
            streams.ForEach(s => s.StopListening());
        }

        /// <summary>
        /// Start listening to all streams
        /// </summary>
        [HttpPost]
        public void StartAllStreams()
        {
            List<StreamListener> streams = (List<StreamListener>)HttpContext.Application["streams"];
            streams.ForEach(s => s.StartListening());
        }

        /// <summary>
        /// Save the current streams to a configuration file
        /// </summary>
        /// <param name="name"></param>
        [HttpPost]
        public void SaveConfiguration(string name)
        {
            List<StreamListener> streams = (List<StreamListener>)HttpContext.Application["streams"];
            XElement config = new XElement("Streams");
            streams.ForEach(s => config.Add(s.GetConfiguration()));
            config.Save(name + ".vcfg");
        }

        /// <summary>
        /// Load a given configuration
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Rendered partial view of the charts of the configuration</returns>
        [HttpPost]
        public ActionResult LoadConfiguration(string name)
        {
            var streams = new List<StreamListener>();
            XElement configurations = XElement.Load(name + ".vcfg");
            foreach (var config in configurations.Elements())
            {
                streams.Add(StreamListener.GetStreamFromConfiguration(config));
            }
            streams.ForEach(s => s.StartListening());
            HttpContext.Application["streams"] = streams;
            return PartialView("ChartElement", streams);
        }

        /// <summary>
        /// List all available configurations
        /// </summary>
        /// <returns>Rendered partial view of the popup menu</returns>
        [HttpGet]
        public ActionResult GetAllConfigurations()
        {
            var files = Directory.EnumerateFiles(".", "*.vcfg").ToArray();
            var configs = files.Select(f => f
                                            .Replace(".\\", "")
                                            .Replace(".vcfg", "")
                                            ).ToArray();
            return PartialView("Configurations", configs);
        }
        #endregion
    }
}