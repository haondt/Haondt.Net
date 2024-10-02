using Haondt.Core.Collections;
using Haondt.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haondt.Identity.StorageKey
{
    public class StorageKeySerializerSettings
    {
        public TypeNameStrategy TypeNameStrategy { get; set; } = TypeNameStrategy.AssemblyQualifiedName;
        public KeyEncodingStrategy KeyEncodingStrategy { get; set; } = KeyEncodingStrategy.Base64;
        public Optional<BiDictionary<Type, string>> LookupTable { get; set; } = new();
    }
}
