using DotNext;
using Haondt.Web.Core.Http;

namespace Haondt.Web.Core.Components
{
    public interface IComponent
    {
        public string ViewPath { get; }
        public IComponentModel Model { get; }
        public Optional<Action<IHttpResponseMutator>> ConfigureResponse { get; }
    }
    public interface IComponent<T> where T : IComponentModel
    {
        public string ViewPath { get; }
        public T Model { get; } 
        public Optional<Action<IHttpResponseMutator>> ConfigureResponse { get; }
    }
}
