using FluentAssertions;
using Haondt.Core.Models;
using Haondt.Json.Converters;
using Newtonsoft.Json;

namespace Haondt.Json.Tests
{
    public class AbsoluteDateTimeJsonConverterTests
    {
        private AbsoluteDateTime _absoluteDateTime;
        private string _json;
        private JsonSerializerSettings _serializerSettings;

        public AbsoluteDateTimeJsonConverterTests()
        {
            _absoluteDateTime = new AbsoluteDateTime(1738711459);
            _json = "1738711459";
            _serializerSettings = new();
            _serializerSettings.Converters.Add(new AbsoluteDateTimeJsonConverter());
        }


        [Fact]
        public void WillSerializeAbsoluteDateTime()
        {
            var serialized = JsonConvert.SerializeObject(_absoluteDateTime, _serializerSettings);
            serialized.Should().Be(_json);
        }
        [Fact]
        public void WillDeserializeAbsoluteDateTime()
        {
            var serialized = JsonConvert.DeserializeObject<AbsoluteDateTime>(_json, _serializerSettings);
            serialized.Should().Be(_absoluteDateTime);
        }
        [Fact]
        public void WillSerializeNullNullableAbsoluteDateTime()
        {
            var serialized = JsonConvert.SerializeObject((AbsoluteDateTime?)null, _serializerSettings);
            serialized.Should().Be("null");
        }
        [Fact]
        public void WillDeserializeNullNullableAbsoluteDateTime()
        {
            var serialized = JsonConvert.DeserializeObject<AbsoluteDateTime?>("null", _serializerSettings);
            serialized.HasValue.Should().BeFalse();
        }
        [Fact]
        public void WillSerializeNullableAbsoluteDateTime()
        {
            var serialized = JsonConvert.SerializeObject((AbsoluteDateTime?)_absoluteDateTime, _serializerSettings);
            serialized.Should().Be(_json);
        }
        [Fact]
        public void WillDeserializeNullableAbsoluteDateTime()
        {
            var serialized = JsonConvert.DeserializeObject<AbsoluteDateTime?>(_json, _serializerSettings);
            serialized.Value.Should().Be(_absoluteDateTime);
        }
    }
}
