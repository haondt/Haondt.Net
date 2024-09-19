using FluentAssertions;
using Haondt.Identity.StorageKey;
using Haondt.Identity.Tests.TestNamespace;

namespace Haondt.Identity.Tests.TestNamespace
{
    public class TestClass3 { }
    public class TestClass4<T1, T2> { }
}
namespace Haondt.Identity.Tests
{
    internal class TestClass1 { }
    internal class TestClass2<T> { }
    public class SimpleTypeConverterTests
    {
        [Theory]
        // easy stuff 
        [InlineData(typeof(TestClass1), "Haondt.Identity.Tests.TestClass1, Haondt.Identity.Tests")]
        [InlineData(typeof(string[]), "System.String[], System.Private.CoreLib")]

        // external namespaces
        [InlineData(typeof(TestClass3), "Haondt.Identity.Tests.TestNamespace.TestClass3, Haondt.Identity.Tests")]

        // external assemblies
        [InlineData(typeof(SimpleTypeConverter), "Haondt.Identity.StorageKey.SimpleTypeConverter, Haondt.Identity")]

        // unbound generics 
        [InlineData(typeof(TestClass2<>), "Haondt.Identity.Tests.TestClass2, Haondt.Identity.Tests<>")]
        [InlineData(typeof(TestClass4<,>), "Haondt.Identity.Tests.TestNamespace.TestClass4, Haondt.Identity.Tests<|>")]

        // bound generics
        [InlineData(typeof(TestClass2<string>), "Haondt.Identity.Tests.TestClass2, Haondt.Identity.Tests<System.String, System.Private.CoreLib>")]
        [InlineData(typeof(TestClass2<string[]>), "Haondt.Identity.Tests.TestClass2, Haondt.Identity.Tests<System.String[], System.Private.CoreLib>")]
        [InlineData(typeof(TestClass2<SimpleTypeConverter>), "Haondt.Identity.Tests.TestClass2, Haondt.Identity.Tests<Haondt.Identity.StorageKey.SimpleTypeConverter, Haondt.Identity>")]
        [InlineData(typeof(TestClass4<int, SimpleTypeConverter>), "Haondt.Identity.Tests.TestNamespace.TestClass4, Haondt.Identity.Tests<System.Int32, System.Private.CoreLib|Haondt.Identity.StorageKey.SimpleTypeConverter, Haondt.Identity>")]
        [InlineData(typeof(TestClass4<int, SimpleTypeConverter[]>), "Haondt.Identity.Tests.TestNamespace.TestClass4, Haondt.Identity.Tests<System.Int32, System.Private.CoreLib|Haondt.Identity.StorageKey.SimpleTypeConverter[], Haondt.Identity>")]

        // nested generics
        [InlineData(typeof(TestClass2<TestClass2<TestClass1>>), "Haondt.Identity.Tests.TestClass2, Haondt.Identity.Tests<Haondt.Identity.Tests.TestClass2, Haondt.Identity.Tests<Haondt.Identity.Tests.TestClass1, Haondt.Identity.Tests>>")]
        [InlineData(typeof(TestClass2<TestClass4<SimpleTypeConverter, SimpleTypeConverter>>), "Haondt.Identity.Tests.TestClass2, Haondt.Identity.Tests<Haondt.Identity.Tests.TestNamespace.TestClass4, Haondt.Identity.Tests<Haondt.Identity.StorageKey.SimpleTypeConverter, Haondt.Identity|Haondt.Identity.StorageKey.SimpleTypeConverter, Haondt.Identity>>")]
        [InlineData(typeof(TestClass4<TestClass2<SimpleTypeConverter>, SimpleTypeConverter>), "Haondt.Identity.Tests.TestNamespace.TestClass4, Haondt.Identity.Tests<Haondt.Identity.Tests.TestClass2, Haondt.Identity.Tests<Haondt.Identity.StorageKey.SimpleTypeConverter, Haondt.Identity>|Haondt.Identity.StorageKey.SimpleTypeConverter, Haondt.Identity>")]
        [InlineData(typeof(TestClass4<SimpleTypeConverter, TestClass2<SimpleTypeConverter>>), "Haondt.Identity.Tests.TestNamespace.TestClass4, Haondt.Identity.Tests<Haondt.Identity.StorageKey.SimpleTypeConverter, Haondt.Identity|Haondt.Identity.Tests.TestClass2, Haondt.Identity.Tests<Haondt.Identity.StorageKey.SimpleTypeConverter, Haondt.Identity>>")]

        public void WillSerializeAndDeserializeType(Type type, string typeString)
        {
            SimpleTypeConverter.TypeToString(type).Should().BeEquivalentTo(typeString);
            SimpleTypeConverter.StringToType(typeString).Should().Be(type);
        }
    }
}
