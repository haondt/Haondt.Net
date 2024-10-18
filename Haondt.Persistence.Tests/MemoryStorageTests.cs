using Haondt.Persistence.Services;

namespace Haondt.Persistence.Tests
{
    public class MemoryStorageTests : AbstractStorageTests
    {
        public MemoryStorageTests() : base(new MemoryStorage())
        {
        }

    }
}