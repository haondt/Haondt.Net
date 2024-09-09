using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haondt.Core.Exceptions
{
    public class UnknownReasonException<TReason>(TReason reason)
        : Exception($"Received an unkown reason '{reason}' from a result.") { }
}
