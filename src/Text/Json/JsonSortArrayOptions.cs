using System.Collections.Generic;
using System.Linq;

namespace BimFormatter.Text.Json
{
    public class JsonSortArrayOptions
    {
        public string Path { get; set; }
        public IReadOnlyCollection<string> PropertyNames { get; set; }

        public static JsonSortArrayOptions SortBy(string path, string propertyName, params string[] otherPropertyNames)
        {
            return new JsonSortArrayOptions
            {
                Path = path,
                PropertyNames = new[] { propertyName }.Concat(otherPropertyNames).ToArray()
            };
        }
    }
}
