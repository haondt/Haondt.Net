using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haondt.Identity.StorageKey
{
    public enum TypeNameStrategy
    {
        AssemblyQualifiedName,
        StringName,
        Name,
        FullName,
        LookupTable,
        SimpleTypeConverter,
    }
}
