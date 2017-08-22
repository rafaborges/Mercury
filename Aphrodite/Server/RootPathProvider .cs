using Nancy;
using System.IO;

namespace Aphrodite.Server
{
    class RootPathProvider : IRootPathProvider
    {
        public string GetRootPath()
        {
            var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\"));
            return path;
        }
    }
}
