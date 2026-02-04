using Haondt.Core.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Haondt.Persistence.EntityFrameworkCore.Converters
{
    public class AbsoluteDateTimeConverter() : ValueConverter<AbsoluteDateTime, long>(
        v => v.UnixTimeSeconds,
        v => AbsoluteDateTime.Create(v))
    {
    }
}
