namespace Haondt.Persistence.MongoDb.Services
{
    public class MongoDbSettings
    {
        public required string Database { get; set; }
        public required string Collection { get; set; }
        public required string ConnectionString { get; set; }
        public bool StoreKeyStrings { get; set; } = false;
        public bool LogCommands { get; set; } = false;
        public bool RegisterIndiscriminateObjectSerializer { get; set; } = true;
        public bool RegisterNewtonsoftJsonSerializers { get; set; } = true;
        public bool RegisterSimpleTypeDiscriminatorConvention { get; set; } = true;
    }
}