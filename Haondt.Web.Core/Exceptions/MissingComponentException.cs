namespace Haondt.Web.Core.Exceptions
{
    public class MissingComponentException : Exception
    {
        public MissingComponentException(string missingComponentName) : base($"Component '{missingComponentName}' not registered")
        {
            
        }
    }
}
