using FluentAssertions;
using Haondt.Core.Models;

namespace Haondt.Core.Tests
{
    public class FooClass()
    {
        public required string Value { get; set; }
    }
    public class UnionTests
    {
        [Fact]
        public void WillUnwrap2TypeUnion()
        {
            var union1 = new Union<string, int>("123");
            var union2 = new Union<string, int>(456);

            union1.Unwrap().Should().Be("123");
            union2.Unwrap().Should().Be(456);
        }

        [Fact]
        public void WillUnwrap6TypeUnion()
        {
            var union1 = new Union<string, int, bool, Optional<string>, FooClass, Optional<FooClass>>("123");
            var union2 = new Union<string, int, bool, Optional<string>, FooClass, Optional<FooClass>>(123);
            var union3 = new Union<string, int, bool, Optional<string>, FooClass, Optional<FooClass>>(true);
            var union4 = new Union<string, int, bool, Optional<string>, FooClass, Optional<FooClass>>(new Optional<string>());
            var union5 = new Union<string, int, bool, Optional<string>, FooClass, Optional<FooClass>>(new FooClass { Value = "foo" });
            var union6 = new Union<string, int, bool, Optional<string>, FooClass, Optional<FooClass>>(new Optional<FooClass>(new FooClass { Value = "bar" }));

            union1.Unwrap().Should().Be("123");
            union2.Unwrap().Should().Be(123);
            union3.Unwrap().Should().Be(true);
            union4.Unwrap().Should().BeEquivalentTo(new Optional<string>());
            union5.Unwrap().Should().BeEquivalentTo(new FooClass { Value = "foo" });
            union6.Unwrap().Should().BeOfType<Optional<FooClass>>();
            union6.Unwrap().As<Optional<FooClass>>().Value.Value.Should().Be("bar");
        }

        [Fact]
        public void WillIsCast2TypeUnion()
        {
            var union1 = new Union<string, int>("123");
            var union2 = new Union<string, int>(456);

            union1.Is<string>(out var stringValue).Should().BeTrue();
            union2.Is<string>(out _).Should().BeFalse();

            union1.Is<int>(out _).Should().BeFalse();
            union2.Is<int>(out var intValue).Should().BeTrue();

            stringValue.Should().Be("123");
            intValue.Should().Be(456);
        }

        [Fact]
        public void WillIsCast6TypeUnion()
        {
            var union1 = new Union<string, int, bool, Optional<string>, FooClass, Optional<FooClass>>("123");
            var union2 = new Union<string, int, bool, Optional<string>, FooClass, Optional<FooClass>>(123);
            var union3 = new Union<string, int, bool, Optional<string>, FooClass, Optional<FooClass>>(true);
            var union4 = new Union<string, int, bool, Optional<string>, FooClass, Optional<FooClass>>(new Optional<string>());
            var union5 = new Union<string, int, bool, Optional<string>, FooClass, Optional<FooClass>>(new FooClass { Value = "foo" });
            var union6 = new Union<string, int, bool, Optional<string>, FooClass, Optional<FooClass>>(new Optional<FooClass>(new FooClass { Value = "bar" }));

            union1.Is<string>(out var stringValue).Should().BeTrue();
            union2.Is<string>(out _).Should().BeFalse();
            union3.Is<string>(out _).Should().BeFalse();
            union4.Is<string>(out _).Should().BeFalse();
            union5.Is<string>(out _).Should().BeFalse();
            union6.Is<string>(out _).Should().BeFalse();

            union1.Is<int>(out _).Should().BeFalse();
            union2.Is<int>(out var intValue).Should().BeTrue();
            union3.Is<int>(out _).Should().BeFalse();
            union4.Is<int>(out _).Should().BeFalse();
            union5.Is<int>(out _).Should().BeFalse();
            union6.Is<int>(out _).Should().BeFalse();

            union1.Is<bool>(out _).Should().BeFalse();
            union2.Is<bool>(out _).Should().BeFalse();
            union3.Is<bool>(out var boolValue).Should().BeTrue();
            union4.Is<bool>(out _).Should().BeFalse();
            union5.Is<bool>(out _).Should().BeFalse();
            union6.Is<bool>(out _).Should().BeFalse();

            union1.Is<Optional<string>>(out _).Should().BeFalse();
            union2.Is<Optional<string>>(out _).Should().BeFalse();
            union3.Is<Optional<string>>(out _).Should().BeFalse();
            union4.Is<Optional<string>>(out var optionalStringValue).Should().BeTrue();
            union5.Is<Optional<string>>(out _).Should().BeFalse();
            union6.Is<Optional<string>>(out _).Should().BeFalse();

            union1.Is<FooClass>(out _).Should().BeFalse();
            union2.Is<FooClass>(out _).Should().BeFalse();
            union3.Is<FooClass>(out _).Should().BeFalse();
            union4.Is<FooClass>(out _).Should().BeFalse();
            union5.Is<FooClass>(out var fooValue).Should().BeTrue();
            union6.Is<FooClass>(out _).Should().BeFalse();

            union1.Is<Optional<FooClass>>(out _).Should().BeFalse();
            union2.Is<Optional<FooClass>>(out _).Should().BeFalse();
            union3.Is<Optional<FooClass>>(out _).Should().BeFalse();
            union4.Is<Optional<FooClass>>(out _).Should().BeFalse();
            union5.Is<Optional<FooClass>>(out _).Should().BeFalse();
            union6.Is<Optional<FooClass>>(out var optionalFooValue).Should().BeTrue();

            stringValue.Should().Be("123");
            intValue.Should().Be(123);
            boolValue.Should().Be(true);
            optionalStringValue.Should().BeEquivalentTo(new Optional<string>());
            fooValue.Should().BeEquivalentTo(new FooClass { Value = "foo" });
            optionalFooValue.Value.Value.Should().Be("bar");
        }

        [Fact]
        public void WillAsCast2TypeUnion()
        {
            var union1 = new Union<string, int>("123");
            var union2 = new Union<string, int>(456);

            union1.As<string>().Value.Should().Be("123");
            union2.As<string>().HasValue.Should().BeFalse();

            union1.As<int>().HasValue.Should().BeFalse();
            union2.As<int>().Value.Should().Be(456);
        }

        [Fact]
        public void WillAsCast6TypeUnion()
        {
            var union1 = new Union<string, int, bool, Optional<string>, FooClass, Optional<FooClass>>("123");
            var union2 = new Union<string, int, bool, Optional<string>, FooClass, Optional<FooClass>>(123);
            var union3 = new Union<string, int, bool, Optional<string>, FooClass, Optional<FooClass>>(true);
            var union4 = new Union<string, int, bool, Optional<string>, FooClass, Optional<FooClass>>(new Optional<string>());
            var union5 = new Union<string, int, bool, Optional<string>, FooClass, Optional<FooClass>>(new FooClass { Value = "foo" });
            var union6 = new Union<string, int, bool, Optional<string>, FooClass, Optional<FooClass>>(new Optional<FooClass>(new FooClass { Value = "bar" }));

            union1.As<string>().Value.Should().Be("123");
            union2.As<string>().HasValue.Should().BeFalse();
            union3.As<string>().HasValue.Should().BeFalse();
            union4.As<string>().HasValue.Should().BeFalse();
            union5.As<string>().HasValue.Should().BeFalse();
            union6.As<string>().HasValue.Should().BeFalse();

            union1.As<int>().HasValue.Should().BeFalse();
            union2.As<int>().Value.Should().Be(123);
            union3.As<int>().HasValue.Should().BeFalse();
            union4.As<int>().HasValue.Should().BeFalse();
            union5.As<int>().HasValue.Should().BeFalse();
            union6.As<int>().HasValue.Should().BeFalse();

            union1.As<bool>().HasValue.Should().BeFalse();
            union2.As<bool>().HasValue.Should().BeFalse();
            union3.As<bool>().Value.Should().Be(true);
            union4.As<bool>().HasValue.Should().BeFalse();
            union5.As<bool>().HasValue.Should().BeFalse();
            union6.As<bool>().HasValue.Should().BeFalse();

            union1.As<Optional<string>>().HasValue.Should().BeFalse();
            union2.As<Optional<string>>().HasValue.Should().BeFalse();
            union3.As<Optional<string>>().HasValue.Should().BeFalse();
            union4.As<Optional<string>>().Value.HasValue.Should().BeFalse();
            union5.As<Optional<string>>().HasValue.Should().BeFalse();
            union6.As<Optional<string>>().HasValue.Should().BeFalse();

            union1.As<FooClass>().HasValue.Should().BeFalse();
            union2.As<FooClass>().HasValue.Should().BeFalse();
            union3.As<FooClass>().HasValue.Should().BeFalse();
            union4.As<FooClass>().HasValue.Should().BeFalse();
            union5.As<FooClass>().Value.Should().BeEquivalentTo(new FooClass { Value = "foo" });
            union6.As<FooClass>().HasValue.Should().BeFalse();

            union1.As<Optional<FooClass>>().HasValue.Should().BeFalse();
            union2.As<Optional<FooClass>>().HasValue.Should().BeFalse();
            union3.As<Optional<FooClass>>().HasValue.Should().BeFalse();
            union4.As<Optional<FooClass>>().HasValue.Should().BeFalse();
            union5.As<Optional<FooClass>>().HasValue.Should().BeFalse();
            union6.As<Optional<FooClass>>().Value.Value.Value.Should().Be("bar");
        }

        [Fact]
        public void WillCast2TypeUnion()
        {
            var union1 = new Union<string, int>("123");
            var union2 = new Union<string, int>(456);

            union1.Cast<string>().Should().Be("123");
            ((string)union1).Should().Be("123");
            union2.Invoking(u => u.Cast<string>())
                .Should().Throw<InvalidCastException>();

            union1.Invoking(u => u.Cast<int>())
                .Should().Throw<InvalidCastException>();
            union2.Cast<int>().Should().Be(456);
            ((int)union2).Should().Be(456);
        }

        [Fact]
        public void WillImplicitCreate2TypeUnion()
        {
            Union<string, int> union1 = "123";
            Union<string, int> union2 = 456;
            union1.Unwrap().Should().Be("123");
            union2.Unwrap().Should().Be(456);
        }
    }
}