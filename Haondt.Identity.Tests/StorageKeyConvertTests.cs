using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Haondt.Identity.StorageKey;
using Haondt.Identity.Tests.TestNamespace;

namespace Haondt.Identity.Tests
{
    public class DataEntry
    {
        public required StorageKey.StorageKey StorageKey { get; set; }
        public required string Base64EncodedValue { get; set; }
        public required string StringEncodedValue { get; set; }
    }
    public class StorageKeyConvertTests
    {
        public static IEnumerable<object[]> Data = new List<DataEntry>
        {
            new DataEntry
            {
                StorageKey = StorageKey<TestClass1>.Create("value1"),
                Base64EncodedValue = "SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3RDbGFzczEsIEhhb25kdC5JZGVudGl0eS5UZXN0cw==:dmFsdWUx",
                StringEncodedValue = "Haondt.Identity.Tests.TestClass1, Haondt.Identity.Tests{:}value1"
            },
            new DataEntry
            {
                StorageKey = StorageKey<string>.Create("value1"),
                Base64EncodedValue = "U3lzdGVtLlN0cmluZywgU3lzdGVtLlByaXZhdGUuQ29yZUxpYg==:dmFsdWUx",
                StringEncodedValue = "System.String, System.Private.CoreLib{:}value1"
            },
            new DataEntry
            {
                StorageKey = StorageKey<TestClass2<TestClass1>>.Create("value1"),
                Base64EncodedValue = "SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3RDbGFzczIsIEhhb25kdC5JZGVudGl0eS5UZXN0czxIYW9uZHQuSWRlbnRpdHkuVGVzdHMuVGVzdENsYXNzMSwgSGFvbmR0LklkZW50aXR5LlRlc3RzPg==:dmFsdWUx",
                StringEncodedValue = "Haondt.Identity.Tests.TestClass2, Haondt.Identity.Tests<Haondt.Identity.Tests.TestClass1, Haondt.Identity.Tests>{:}value1"
            },
            new DataEntry
            {
                StorageKey = StorageKey<TestClass4<int, SimpleTypeConverter[]>>.Create("value1"),
                Base64EncodedValue = "SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3ROYW1lc3BhY2UuVGVzdENsYXNzNCwgSGFvbmR0LklkZW50aXR5LlRlc3RzPFN5c3RlbS5JbnQzMiwgU3lzdGVtLlByaXZhdGUuQ29yZUxpYnxIYW9uZHQuSWRlbnRpdHkuU3RvcmFnZUtleS5TaW1wbGVUeXBlQ29udmVydGVyW10sIEhhb25kdC5JZGVudGl0eT4=:dmFsdWUx",
                StringEncodedValue = "Haondt.Identity.Tests.TestNamespace.TestClass4, Haondt.Identity.Tests<System.Int32, System.Private.CoreLib|Haondt.Identity.StorageKey.SimpleTypeConverter[], Haondt.Identity>{:}value1"
            },
            new DataEntry // 5
            {
                StorageKey = StorageKey<TestClass2<TestClass2<TestClass1>>>.Create("value1"),
                Base64EncodedValue = "SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3RDbGFzczIsIEhhb25kdC5JZGVudGl0eS5UZXN0czxIYW9uZHQuSWRlbnRpdHkuVGVzdHMuVGVzdENsYXNzMiwgSGFvbmR0LklkZW50aXR5LlRlc3RzPEhhb25kdC5JZGVudGl0eS5UZXN0cy5UZXN0Q2xhc3MxLCBIYW9uZHQuSWRlbnRpdHkuVGVzdHM+Pg==:dmFsdWUx",
                StringEncodedValue = "Haondt.Identity.Tests.TestClass2, Haondt.Identity.Tests<Haondt.Identity.Tests.TestClass2, Haondt.Identity.Tests<Haondt.Identity.Tests.TestClass1, Haondt.Identity.Tests>>{:}value1"
            },
            new DataEntry
            {
                StorageKey = StorageKey<TestClass2<TestClass4<SimpleTypeConverter, SimpleTypeConverter>>>.Create("value1"),
                Base64EncodedValue = "SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3RDbGFzczIsIEhhb25kdC5JZGVudGl0eS5UZXN0czxIYW9uZHQuSWRlbnRpdHkuVGVzdHMuVGVzdE5hbWVzcGFjZS5UZXN0Q2xhc3M0LCBIYW9uZHQuSWRlbnRpdHkuVGVzdHM8SGFvbmR0LklkZW50aXR5LlN0b3JhZ2VLZXkuU2ltcGxlVHlwZUNvbnZlcnRlciwgSGFvbmR0LklkZW50aXR5fEhhb25kdC5JZGVudGl0eS5TdG9yYWdlS2V5LlNpbXBsZVR5cGVDb252ZXJ0ZXIsIEhhb25kdC5JZGVudGl0eT4+:dmFsdWUx",
                StringEncodedValue = "Haondt.Identity.Tests.TestClass2, Haondt.Identity.Tests<Haondt.Identity.Tests.TestNamespace.TestClass4, Haondt.Identity.Tests<Haondt.Identity.StorageKey.SimpleTypeConverter, Haondt.Identity|Haondt.Identity.StorageKey.SimpleTypeConverter, Haondt.Identity>>{:}value1"
            },
            
            new DataEntry
            {
                StorageKey = StorageKey<TestClass4<int, SimpleTypeConverter[]>>.Create("value:value::value"),
                Base64EncodedValue = "SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3ROYW1lc3BhY2UuVGVzdENsYXNzNCwgSGFvbmR0LklkZW50aXR5LlRlc3RzPFN5c3RlbS5JbnQzMiwgU3lzdGVtLlByaXZhdGUuQ29yZUxpYnxIYW9uZHQuSWRlbnRpdHkuU3RvcmFnZUtleS5TaW1wbGVUeXBlQ29udmVydGVyW10sIEhhb25kdC5JZGVudGl0eT4=:dmFsdWU6dmFsdWU6OnZhbHVl",
                StringEncodedValue = "Haondt.Identity.Tests.TestNamespace.TestClass4, Haondt.Identity.Tests<System.Int32, System.Private.CoreLib|Haondt.Identity.StorageKey.SimpleTypeConverter[], Haondt.Identity>{:}value::value::::value"
            },
            new DataEntry
            {
                StorageKey = StorageKey<TestClass1>.Create("::value1::"),
                Base64EncodedValue = "SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3RDbGFzczEsIEhhb25kdC5JZGVudGl0eS5UZXN0cw==:Ojp2YWx1ZTE6Og==",
                StringEncodedValue = "Haondt.Identity.Tests.TestClass1, Haondt.Identity.Tests{:}::::value1::::"
            },
            new DataEntry
            {
                StorageKey = StorageKey<TestClass1>.Create(":value1:"),
                Base64EncodedValue = "SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3RDbGFzczEsIEhhb25kdC5JZGVudGl0eS5UZXN0cw==:OnZhbHVlMTo=",
                StringEncodedValue = "Haondt.Identity.Tests.TestClass1, Haondt.Identity.Tests{:}::value1::"
            },
            new DataEntry // 10
            {
                StorageKey = StorageKey<TestClass1>.Create("value:value"),
                Base64EncodedValue = "SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3RDbGFzczEsIEhhb25kdC5JZGVudGl0eS5UZXN0cw==:dmFsdWU6dmFsdWU=",
                StringEncodedValue = "Haondt.Identity.Tests.TestClass1, Haondt.Identity.Tests{:}value::value"
            },
            new DataEntry
            {
                StorageKey = StorageKey<TestClass1>.Create("{:}value"),
                Base64EncodedValue = "SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3RDbGFzczEsIEhhb25kdC5JZGVudGl0eS5UZXN0cw==:ezp9dmFsdWU=",
                StringEncodedValue = "Haondt.Identity.Tests.TestClass1, Haondt.Identity.Tests{:}{::}value"
            },
            new DataEntry
            {
                StorageKey = StorageKey<TestClass1>.Create("value::value"),
                Base64EncodedValue = "SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3RDbGFzczEsIEhhb25kdC5JZGVudGl0eS5UZXN0cw==:dmFsdWU6OnZhbHVl",
                StringEncodedValue = "Haondt.Identity.Tests.TestClass1, Haondt.Identity.Tests{:}value::::value"
            },
            new DataEntry
            {
                StorageKey = StorageKey<TestClass1>.Create("+value1+"),
                Base64EncodedValue = "SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3RDbGFzczEsIEhhb25kdC5JZGVudGl0eS5UZXN0cw==:K3ZhbHVlMSs=",
                StringEncodedValue = "Haondt.Identity.Tests.TestClass1, Haondt.Identity.Tests{:}++value1++"
            },
            new DataEntry
            {
                StorageKey = StorageKey<TestClass1>.Create("value{+}value"),
                Base64EncodedValue = "SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3RDbGFzczEsIEhhb25kdC5JZGVudGl0eS5UZXN0cw==:dmFsdWV7K312YWx1ZQ==",
                StringEncodedValue = "Haondt.Identity.Tests.TestClass1, Haondt.Identity.Tests{:}value{++}value"
            },
            new DataEntry // 15
            {
                StorageKey = StorageKey<TestClass1>.Create("{:}value+").Extend<int>("value2"),
                Base64EncodedValue = "SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3RDbGFzczEsIEhhb25kdC5JZGVudGl0eS5UZXN0cw==:ezp9dmFsdWUr,U3lzdGVtLkludDMyLCBTeXN0ZW0uUHJpdmF0ZS5Db3JlTGli:dmFsdWUy",
                StringEncodedValue = "Haondt.Identity.Tests.TestClass1, Haondt.Identity.Tests{:}{::}value++{+}System.Int32, System.Private.CoreLib{:}value2"
            },
            new DataEntry
            {
                StorageKey = StorageKey<TestClass1>.Create(":value").Extend<int>("{+}value2"),
                Base64EncodedValue = "SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3RDbGFzczEsIEhhb25kdC5JZGVudGl0eS5UZXN0cw==:OnZhbHVl,U3lzdGVtLkludDMyLCBTeXN0ZW0uUHJpdmF0ZS5Db3JlTGli:eyt9dmFsdWUy",
                StringEncodedValue = "Haondt.Identity.Tests.TestClass1, Haondt.Identity.Tests{:}::value{+}System.Int32, System.Private.CoreLib{:}{++}value2"
            },

            new DataEntry
            {
                StorageKey = StorageKey<TestClass1>.Create("value1").Extend<string>("value2"),
                Base64EncodedValue = "SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3RDbGFzczEsIEhhb25kdC5JZGVudGl0eS5UZXN0cw==:dmFsdWUx,U3lzdGVtLlN0cmluZywgU3lzdGVtLlByaXZhdGUuQ29yZUxpYg==:dmFsdWUy",
                StringEncodedValue = "Haondt.Identity.Tests.TestClass1, Haondt.Identity.Tests{:}value1{+}System.String, System.Private.CoreLib{:}value2"
            },
            new DataEntry
            {
                StorageKey = StorageKey<TestClass1>.Empty.Extend<string>("value2"),
                Base64EncodedValue = "SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3RDbGFzczEsIEhhb25kdC5JZGVudGl0eS5UZXN0cw==:,U3lzdGVtLlN0cmluZywgU3lzdGVtLlByaXZhdGUuQ29yZUxpYg==:dmFsdWUy",
                StringEncodedValue = "Haondt.Identity.Tests.TestClass1, Haondt.Identity.Tests{:}{+}System.String, System.Private.CoreLib{:}value2"
            },

            new DataEntry
            {
                StorageKey = StorageKey<TestClass1>.Empty.Extend<TestClass2<int[]>>("value2").Extend<TestClass3>("value3"),
                Base64EncodedValue = "SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3RDbGFzczEsIEhhb25kdC5JZGVudGl0eS5UZXN0cw==:,SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3RDbGFzczIsIEhhb25kdC5JZGVudGl0eS5UZXN0czxTeXN0ZW0uSW50MzJbXSwgU3lzdGVtLlByaXZhdGUuQ29yZUxpYj4=:dmFsdWUy,SGFvbmR0LklkZW50aXR5LlRlc3RzLlRlc3ROYW1lc3BhY2UuVGVzdENsYXNzMywgSGFvbmR0LklkZW50aXR5LlRlc3Rz:dmFsdWUz",
                StringEncodedValue = "Haondt.Identity.Tests.TestClass1, Haondt.Identity.Tests{:}{+}Haondt.Identity.Tests.TestClass2, Haondt.Identity.Tests<System.Int32[], System.Private.CoreLib>{:}value2{+}Haondt.Identity.Tests.TestNamespace.TestClass3, Haondt.Identity.Tests{:}value3"
            },

        }.Select(d => new object[] { d });


        [Theory]
        [MemberData(nameof(Data))]
        public void WillBase64SerializeStorageKey(DataEntry data)
        {
            var serializerSettings = new StorageKeySerializerSettings
            {
                KeyEncodingStrategy = KeyEncodingStrategy.Base64,
                TypeNameStrategy = TypeNameStrategy.SimpleTypeConverter
            };

            var serialized = StorageKeyConvert.Serialize(data.StorageKey, serializerSettings);
            serialized.Should().BeEquivalentTo(data.Base64EncodedValue);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void WillStringSerializeStorageKey(DataEntry data)
        {
            var serializerSettings = new StorageKeySerializerSettings
            {
                KeyEncodingStrategy = KeyEncodingStrategy.String,
                TypeNameStrategy = TypeNameStrategy.SimpleTypeConverter
            };

            var serialized = StorageKeyConvert.Serialize(data.StorageKey, serializerSettings);
            serialized.Should().BeEquivalentTo(data.StringEncodedValue);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void WillBase64DeserializeStorageKey(DataEntry data)
        {
            var serializerSettings = new StorageKeySerializerSettings
            {
                KeyEncodingStrategy = KeyEncodingStrategy.Base64,
                TypeNameStrategy = TypeNameStrategy.SimpleTypeConverter
            };

            var deserialized = StorageKeyConvert.Deserialize(data.Base64EncodedValue, serializerSettings);
            deserialized.Should().BeEquivalentTo(data.StorageKey);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void WillStringDeserializeStorageKey(DataEntry data)
        {
            var serializerSettings = new StorageKeySerializerSettings
            {
                KeyEncodingStrategy = KeyEncodingStrategy.String,
                TypeNameStrategy = TypeNameStrategy.SimpleTypeConverter
            };

            var deserialized = StorageKeyConvert.Deserialize(data.StringEncodedValue, serializerSettings);
            deserialized.Should().BeEquivalentTo(data.StorageKey);
        }
    }
}
