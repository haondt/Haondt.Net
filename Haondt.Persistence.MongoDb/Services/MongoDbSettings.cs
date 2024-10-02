using Haondt.Identity.StorageKey;

namespace Haondt.Persistence.MongoDb.Services
{
    public class MongoDbSettings
    {
        public required string ConnectionString { get; set; }
        public bool LogCommands { get; set; } = false;
    }
}