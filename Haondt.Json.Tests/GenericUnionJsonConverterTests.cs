using FluentAssertions;
using Haondt.Core.Models;
using Haondt.Json.Converters;
using Newtonsoft.Json;

namespace Haondt.Json.Tests
{
    public class UnionJsonConverterTests
    {
        private Union<string, int> _union1;
        private Union<string, int> _union2;
        private string _json1;
        private string _json2;
        private JsonSerializerSettings _serializerSettings;

        public UnionJsonConverterTests()
        {
            _union1 = new("123");
            _union2 = new(456);
            _json1 = "\"123\"";
            _json2 = "456";
            _serializerSettings = new();
            _serializerSettings.Converters.Add(new GenericUnionJsonConverter());
        }


        [Fact]
        public void WillSerializeUnion1()
        {
            var serialized = JsonConvert.SerializeObject(_union1, _serializerSettings);
            serialized.Should().Be(_json1);
        }

        [Fact]
        public void WillSerializeUnion2()
        {
            var serialized = JsonConvert.SerializeObject(_union2, _serializerSettings);
            serialized.Should().Be(_json2);
        }

        [Fact]
        public void WillDeserializeUnion1()
        {
            var serialized = JsonConvert.DeserializeObject<Union<string, int>>(_json1, _serializerSettings);
            serialized.Should().Be(_union1);
        }

        [Fact]
        public void WillDeserializeUnion2()
        {
            var serialized = JsonConvert.DeserializeObject<Union<string, int>>(_json2, _serializerSettings);
            serialized.Should().Be(_union2);
        }

        [Fact]
        public void WillSerializeNullNullableUnion()
        {
            var serialized = JsonConvert.SerializeObject((Union<string, int>?)null, _serializerSettings);
            serialized.Should().Be("null");
        }

        [Fact]
        public void WillDeserializeNullNullableUnion()
        {
            var serialized = JsonConvert.DeserializeObject<Union<string, int>?>("null", _serializerSettings);
            serialized.HasValue.Should().BeFalse();
        }

        [Fact]
        public void WillSerializeNullableUnion()
        {
            var serialized = JsonConvert.SerializeObject((Union<string, int>?)_union2, _serializerSettings);
            serialized.Should().Be(_json2);
        }

        [Fact]
        public void WillDeserializeNullableUnion()
        {
            var serialized = JsonConvert.DeserializeObject<Union<string, int>?>(_json2, _serializerSettings);
            serialized.Value.Unwrap().Should().Be(_union2.Unwrap());
        }
    }
}
