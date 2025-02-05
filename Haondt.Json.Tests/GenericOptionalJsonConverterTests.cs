using FluentAssertions;
using Haondt.Core.Converters;
using Haondt.Core.Models;
using Newtonsoft.Json;
using System.Reflection;

namespace Haondt.Json.Tests
{
    public class GenericOptionalData
    {
        public required List<Optional<List<string>>> Strings { get; set; }
    }

    public class GenericOptionalJsonConverterTests
    {
        private Optional<List<string>> _optional;
        private Optional<List<string>> _optional2;
        private string _json;
        private string _json2;
        private JsonSerializerSettings _serializerSettings;

        public GenericOptionalJsonConverterTests()
        {
            _optional = new Optional<List<string>>(["1", "2"]);
            _optional2 = new Optional<List<string>>();

            var testDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var filePath = Path.Combine(testDir, "optional1.json");
            _json = File.ReadAllText(filePath).Trim();
            filePath = Path.Combine(testDir, "optional2.json");
            _json2 = File.ReadAllText(filePath).Trim();
            _serializerSettings = new();
            _serializerSettings.Converters.Add(new GenericOptionalJsonConverter());
        }


        [Fact]
        public void WillSerializeOptional()
        {
            var serialized = JsonConvert.SerializeObject(_optional, _serializerSettings);
            serialized.Should().Be(_json);
        }
        [Fact]
        public void WillSerializeEmptyOptional()
        {
            var serialized = JsonConvert.SerializeObject(_optional2, _serializerSettings);
            serialized.Should().Be(_json2);
        }

        [Fact]
        public void WillDeserializeOptional()
        {
            var serialized = JsonConvert.DeserializeObject<Optional<List<string>>>(_json, _serializerSettings);
            serialized.Value.Should().BeEquivalentTo(_optional.Value);
        }
        [Fact]
        public void WillDeserializeEmptyOptional()
        {
            var serialized = JsonConvert.DeserializeObject<Optional<List<string>>>(_json2, _serializerSettings);
            serialized.HasValue.Should().BeFalse();
        }

        [Fact]
        public void WillSerializeNullNullableGenericOptional()
        {
            var serialized = JsonConvert.SerializeObject((Optional<List<string>>?)null, _serializerSettings);
            serialized.Should().Be("null");
        }
        [Fact]
        public void WillDeserializeNullNullableGenericOptional()
        {
            var serialized = JsonConvert.DeserializeObject<Optional<List<string>>?>("null", _serializerSettings);
            serialized.HasValue.Should().BeFalse();
        }

        [Fact]
        public void WillSerializeNullableGenericOptional()
        {
            var serialized = JsonConvert.SerializeObject((Optional<List<string>>?)_optional, _serializerSettings);
            serialized.Should().Be(_json);
        }
        [Fact]
        public void WillDeserializeNullableGenericOptional()
        {
            var serialized = JsonConvert.DeserializeObject<Optional<List<string>>?>(_json, _serializerSettings);
            serialized.Value.Value.Should().BeEquivalentTo(_optional.Value);
        }

    }
}
