using FluentAssertions;
using Haondt.Core.Converters;
using Haondt.Core.Models;

namespace Haondt.Core.Tests
{
    public class StringConverterTests
    {
        public enum FooEnum
        {
            One,
            Two,
            Three = 99
        }
        [Fact]
        public void WillParseNullables()
        {
            StringConverter.Parse<int?>("5").Should().Be((int?)5);
            StringConverter.Parse<string?>("5").Should().Be((string?)"5");
            StringConverter.Parse<int?>("").Should().Be(null);
            StringConverter.Parse<string?>("").Should().Be("");
            StringConverter.Parse<FooEnum?>("").Should().Be(null);
            StringConverter.Parse<FooEnum?>("Two").Should().Be(FooEnum.Two);
            StringConverter.Parse<FooEnum?>("Three").Should().Be(FooEnum.Three);
            StringConverter.Parse<FooEnum?>("1").Should().Be(FooEnum.Two);
            StringConverter.Parse<FooEnum?>("99").Should().Be(FooEnum.Three);
        }

        [Fact]
        public void WillParseBoolean()
        {
            StringConverter.Parse<bool>("true").Should().Be(true);
            StringConverter.Parse<bool>("false").Should().Be(false);
            StringConverter.Parse<bool>("True").Should().Be(true);
            StringConverter.Parse<bool>("False").Should().Be(false);
            StringConverter.Parse<bool>("TRUE").Should().Be(true);
            StringConverter.Parse<bool>("FALSE").Should().Be(false);
        }

        [Fact]
        public void WillParseString()
        {
            StringConverter.Parse<string>("").Should().Be("");
            StringConverter.Parse<string>("X").Should().Be("X");
        }

        [Fact]
        public void WillParseChar()
        {
            StringConverter.Parse<char>("0").Should().Be('0');
            StringConverter.Parse<char>("X").Should().Be('X');
        }

        [Fact]
        public void WillParseNumericTypes()
        {
            StringConverter.Parse<sbyte>("3").Should().Be((sbyte)3);
            StringConverter.Parse<sbyte>("-3").Should().Be((sbyte)-3);
            StringConverter.Parse<byte>("3").Should().Be((byte)3);
            StringConverter.Parse<short>("12").Should().Be((short)12);
            StringConverter.Parse<short>("-12").Should().Be((short)-12);
            StringConverter.Parse<ushort>("12").Should().Be((ushort)12);
            StringConverter.Parse<int>("12").Should().Be((int)12);
            StringConverter.Parse<int>("-12").Should().Be((int)-12);
            StringConverter.Parse<uint>("12").Should().Be((uint)12);
            StringConverter.Parse<long>("12").Should().Be((long)12);
            StringConverter.Parse<long>("-12").Should().Be((long)-12);
            StringConverter.Parse<ulong>("12").Should().Be((ulong)12);
            StringConverter.Parse<double>("12").Should().Be((double)12);
            StringConverter.Parse<double>("-12").Should().Be((double)-12);
            StringConverter.Parse<double>("12.34").Should().Be((double)12.34);
            StringConverter.Parse<float>("12").Should().Be((float)12);
            StringConverter.Parse<float>("-12").Should().Be((float)-12);
            StringConverter.Parse<float>("12.34").Should().Be((float)12.34);
            StringConverter.Parse<decimal>("12").Should().Be((decimal)12);
            StringConverter.Parse<decimal>("-12").Should().Be((decimal)-12);
            StringConverter.Parse<decimal>("12.34").Should().Be((decimal)12.34);
        }

        [Fact]
        public void WillParseDateTime()
        {
            var dateTime = new DateTime(2006, 1, 2, 15, 4, 5);
            StringConverter.Parse<DateTime>("2006-01-02T15:04:05.000-07:00").Should().Be(dateTime);
            StringConverter.Parse<DateTime>("2006-01-02T22:04:05.00000Z").Should().Be(dateTime);
            StringConverter.Parse<DateTime>("2006-01-02 15:04:05").Should().Be(dateTime);
            StringConverter.Parse<DateTime>("2006-01-02").Should().Be(dateTime.Date);
        }

        [Fact]
        public void WillParseAbsoluteDateTime()
        {
            var dateTime = AbsoluteDateTime.Create(new DateTime(2006, 1, 2, 15, 4, 5));
            var date = AbsoluteDateTime.Create(new DateTime(2006, 1, 2));
            StringConverter.Parse<AbsoluteDateTime>("1136239445").Should().Be(dateTime);
            StringConverter.Parse<AbsoluteDateTime>("2006-01-02T15:04:05.000-07:00").Should().Be(dateTime);
            StringConverter.Parse<AbsoluteDateTime>("2006-01-02T22:04:05.00000Z").Should().Be(dateTime);
            StringConverter.Parse<AbsoluteDateTime>("2006-01-02 15:04:05").Should().Be(dateTime);
            StringConverter.Parse<AbsoluteDateTime>("2006-01-02").Should().Be(date);
        }

        [Fact]
        public void WillParseGuid()
        {
            var guid = Guid.Parse("e3100b42-883f-4fb4-86c6-c051e8d95743");
            StringConverter.Parse<Guid>("e3100b42-883f-4fb4-86c6-c051e8d95743").Should().Be(guid);
            StringConverter.Parse<Guid>("E3100B42-883F-4FB4-86C6-C051E8D95743").Should().Be(guid);
        }


        [Fact]
        public void WillParseEnum()
        {
            StringConverter.Parse<FooEnum>("One").Should().Be(FooEnum.One);
            StringConverter.Parse<FooEnum>("Two").Should().Be(FooEnum.Two);
            StringConverter.Parse<FooEnum>("Three").Should().Be(FooEnum.Three);
            StringConverter.Parse<FooEnum>("0").Should().Be(FooEnum.One);
            StringConverter.Parse<FooEnum>("1").Should().Be(FooEnum.Two);
            StringConverter.Parse<FooEnum>("99").Should().Be(FooEnum.Three);
        }
    }
}
