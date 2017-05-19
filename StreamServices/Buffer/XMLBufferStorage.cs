using System;
using System.IO;
using System.Collections.Generic;
using StreamServices.Services;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StreamServices.Buffer
{
    /// <summary>
    /// Class that stores <see cref="IBuffer"/> data into XML files
    /// </summary>
    class XMLBufferStorage : IBufferPersistence
    {
        /// <summary>
        /// Load all saved data
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<EventData> GetAllStoredValues(Guid id)
        {
            List<EventData> bufferedData = new List<EventData>();
            if (Directory.Exists(id.ToString()))
            {
                var directory = new DirectoryInfo(id.ToString());
                foreach (var file in directory.GetFiles())
                {
                    XElement.Load(file.FullName);
                }
            }

            return bufferedData;
        }

        public void RemoveData(EventData data)
        {
            // Separated task to not block the main buffer thread
            // IO is very thread consuming!
            Task.Factory.StartNew(() =>
            {
                var dirPath = "Buffer\\" + data.Source.ToString();
                if (Directory.Exists(dirPath))
                {
                    var directory = new DirectoryInfo(dirPath);
                    var path = Path.Combine(directory.FullName, data.GetHashCode().ToString());
                    File.Delete(path);
                }
            });
        }

        public void StoreData(EventData data)
        {
            // Separated task to not block the main buffer thread
            // IO is very thread consuming!
            Task.Factory.StartNew(() =>
           {
               var dirPath = "Buffer\\" + data.Source.ToString();
               DirectoryInfo directory;
               if (!Directory.Exists(dirPath))
               {
                   directory =
                       Directory.CreateDirectory(dirPath);
               }
               else
               {
                   directory = new DirectoryInfo(dirPath);
               }
               var path = Path.Combine(directory.FullName, data.GetHashCode().ToString());
               data.GetXML().Save(path);
           });
        }
    }
}