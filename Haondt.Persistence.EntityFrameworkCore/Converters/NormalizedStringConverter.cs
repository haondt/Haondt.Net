using Haondt.Core.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Haondt.Persistence.EntityFrameworkCore.Converters
{
    public class NormalizedStringConverter : ValueConverter<NormalizedString, string>
    {
        public NormalizedStringConverter() : base(
            normalizedString => normalizedString.Value,
            value => NormalizedString.Create(value))
        {
        }
    }
}

