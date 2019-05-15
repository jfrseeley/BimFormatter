using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BimFormatter.Text.Json
{
    public class JsonFormatter : ITextFormatter
    {
        private readonly Dictionary<string, JsonSortArrayOptions> _sortArrayOptions;
        private readonly HashSet<string> _registeredPaths;

        public JsonFormatter(IEnumerable<JsonSortArrayOptions> sortArrayOptions)
        {
            _sortArrayOptions = sortArrayOptions.ToDictionary(jsao => jsao.Path);
            _registeredPaths = new HashSet<string>(_sortArrayOptions.Keys.SelectMany(ExtractPaths));
        }

        public string Format(string text)
        {
            var root = JsonConvert.DeserializeObject(text) as JObject;
            SortChildrenRecursive(root);

            return JsonConvert.SerializeObject(root, Formatting.Indented);
        }

        private void SortChildrenRecursive(JObject source)
        {
            foreach (var child in source.Children().ToArray())
            {
                SortTokenRecursive(child);
            }
        }

        private void SortChildrenRecursive(JArray source, JsonSortArrayOptions sortArrayOptions)
        {
            var children = source.Children().ToArray();
            foreach (var child in children)
            {
                SortTokenRecursive(child);
            }

            if (sortArrayOptions != null)
            {
                foreach (var child in children)
                {
                    child.Remove();
                }

                var orderByPropertyName = sortArrayOptions.PropertyNames.First();
                var ordered = children.OrderBy(jt => jt[orderByPropertyName]);

                foreach (var thenByPropertyName in sortArrayOptions.PropertyNames.Skip(1))
                {
                    ordered = ordered.ThenBy(jt => jt[thenByPropertyName]);
                }

                foreach (var child in ordered)
                {
                    source.Add(child);
                }
            }
        }

        private void SortTokenRecursive(JToken source)
        {
            var sourcePath = Regex.Replace(source.Path, @"\[\d+\]", string.Empty);
            if (_registeredPaths.Contains(sourcePath))
            {
                var token = source.Type == JTokenType.Property
                    ? ((JProperty)source).Value
                    : source;

                switch (token.Type)
                {
                    case JTokenType.Object:
                        SortChildrenRecursive(token as JObject);
                        break;
                    case JTokenType.Array:
                        _sortArrayOptions.TryGetValue(sourcePath, out JsonSortArrayOptions sortArrayOptions);
                        SortChildrenRecursive(token as JArray, sortArrayOptions);
                        break;
                }
            }
        }

        private static IEnumerable<string> ExtractPaths(string path)
        {
            yield return path;
            for (var index = path.Length - 1; index > -1; index--)
            {
                if (path[index] == '.')
                {
                    yield return path.Substring(0, index);
                }
            }
        }
    }
}
