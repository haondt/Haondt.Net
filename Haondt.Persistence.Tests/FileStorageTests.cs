using Haondt.Persistence.Services;
using Microsoft.Extensions.Options;

namespace Haondt.Persistence.Tests
{
    public class FileStorageTests : AbstractStorageTests
    {
        public FileStorageTests() : base(new TransientTransactionalBatchStorage(new FileStorage(Options.Create(new HaondtFileStorageSettings
        {
            DataFile = "./test-data.json"
        }))))
        {
        }
    }
}
