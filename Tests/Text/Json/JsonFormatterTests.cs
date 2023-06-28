using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace BimFormatter.Text.Json
{
    [TestClass]
    public class JsonFormatterTests
    {
        [TestMethod]
        public void Format_MixedCase_OrderByCaseInsensitive()
        {
            // Arrange
            var target = new JsonFormatter(new[] { JsonSortArrayOptions.SortBy("model.tables", "name") })
            {
                Formatting = Formatting.None
            };

            const string text = @"{""model"":{""tables"":[{""name"":""xx""},{""name"":""aa""},{""name"":""AB""}]}}";
            const string expected = @"{""model"":{""tables"":[{""name"":""aa""},{""name"":""AB""},{""name"":""xx""}]}}";

            // Act
            var actual = target.Format(text);

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}
