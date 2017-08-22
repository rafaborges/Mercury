using Nancy.ViewEngines.Razor;
using System.Collections.Generic;

namespace Aphrodite.Server
{
    class RazorConfig : IRazorConfiguration
    {
        public IEnumerable<string> GetAssemblyNames()
        {
            yield return "Aphrodite.Models";
            yield return "Aphrodite.Views";
        }

        public IEnumerable<string> GetDefaultNamespaces()
        {
            yield return "Aphrodite.Models";
        }

        public bool AutoIncludeModelNamespace
        {
            get { return true; }
        }
    }
}
