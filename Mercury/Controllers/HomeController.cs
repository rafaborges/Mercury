using System.Collections.Generic;
using System.Web.Mvc;
using Mercury.Models;
using System;
using StreamServices;
using StreamServices.Buffer;

namespace Mercury.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult LoadStreams()
        {
            var model = GetDataStream();
            return PartialView("ChartElement", model);
        }

        /// <summary>
        /// Get all StreamListeners saved on the main repository
        /// </summary>
        /// <returns></returns>
        private List<StreamListener> GetDataStream()
        {
            var streams = new List<StreamListener>
            {
                new StreamListener("Random Stream #1", ServiceType.Random, "", typeof(double), 25, BufferInvalidationType.Events)
            };

            // Starting all streams
            streams.ForEach(s => s.StartListening());
            return streams;
        }

        /// <summary>
        /// Entrypoint for adding a new stream to the collection of streams
        /// </summary>
        /// <param name="name">A name for the stream</param>
        /// <param name="connectionString">the connection string for the stream</param>
        /// <param name="source">The service provider that will be uses</param>
        /// <returns>A partialView with the chart that will be displayed</returns>
        [HttpPost]
        public ActionResult AddDataStream(string name, string connectionString, string source, string dataType, int bufferSize, string bufferType)
        {
            Type type = Type.GetType("System." + dataType);
            StreamListener stream = new StreamListener(name, ServiceType.Random, "", type, bufferSize, BufferInvalidationType.Events);
            var model = new List<StreamListener>() { stream };
            stream.StartListening();
            return PartialView("ChartElement", model);
        }


        [HttpPost]
        public bool RemoveDataStream(Guid id)
        {
            // Remove from Collection

            return true;
        }
    }
}