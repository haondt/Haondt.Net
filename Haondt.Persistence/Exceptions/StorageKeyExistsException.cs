using Haondt.Identity.StorageKey;

namespace Haondt.Persistence.Exceptions
{
    public class StorageKeyExistsException : StorageException
    {
        public StorageKeyExistsException() : base("The StorageKey already exists.")
        {
        }
        public StorageKeyExistsException(Exception innerException) : base("The StorageKey already exists.", innerException)
        {
        }
        public StorageKeyExistsException(StorageKey storageKey) : base($"The StorageKey {storageKey} is already present.")
        {
        }
        public StorageKeyExistsException(StorageKey storageKey, Exception innerException) : base($"The StorageKey {storageKey} is already present.", innerException)
        {
        }

        public StorageKeyExistsException(string? message) : base(message)
        {
        }

        public StorageKeyExistsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
