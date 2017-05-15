using StreamServices.Buffer;
using StreamServices.Interfaces;

namespace StreamServices
{
    /// <summary>
    /// A simple factory for all available services.
    /// </summary>
    public static class StreamServiceFactory
    {
        /// <summary>
        /// Instantiates the appropriate IStreamConsumer object
        /// </summary>
        /// <typeparam name="T">A class that implements IStreamConsumer</typeparam>
        /// <returns>An instance of T</returns>
        public static T CreateConsumer<T>() where T : IStreamConsumer, new()
        {
            return new T();
        }

        /// <summary>
        /// Instantiates the appropriate IBuffer object
        /// </summary>
        /// <typeparam name="T">A class that implements IBuffer</typeparam>
        /// <returns>An instance of T</returns>
        public static T CreateBuffer<T>(int capacity) where T : IBuffer, new()
        {
            var buffer = new T();
            buffer.Capacity = capacity;
            return buffer;
        }
    }
}
