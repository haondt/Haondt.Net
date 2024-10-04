using Haondt.Identity.StorageKey;

namespace Haondt.Persistence.MongoDb.Services
{
    public class MongoDbSettings
    {
        public required string ConnectionString { get; set; }
        public bool LogCommands { get; set; } = false;
        public bool RegisterIndiscriminateObjectSerializer { get; set; } = true;
        public bool RegisterNewtonsoftJsonSerializers { get; set; } = true;
        public bool RegisterSimpleTypeDiscriminatorConvention { get; set; } = true;
    }
}