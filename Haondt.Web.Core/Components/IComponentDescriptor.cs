using Haondt.Core.Models;
using Haondt.Web.Core.Http;

namespace Haondt.Web.Core.Components
{
    public interface IComponentDescriptor
    {
        public string Identity { get; }
        public string ViewPath { get; }
        public Optional<Action<IHttpResponseMutator>> ConfigureResponse { get; }
        public Optional<Func<IComponentFactory, Task<IComponentModel>>> DefaultNoRequestDataModelFactory { get; }
        public Optional<Func<IComponentFactory, IRequestData, Task<(IComponentModel, Optional<Action<IHttpResponseMutator>>)>>> DefaultModelFactory { get; }
    }
    public interface IComponentDescriptor<T> where T : IComponentModel
    {
        public string Identity { get; }
        public string ViewPath { get; }
        public Optional<Action<IHttpResponseMutator>> ConfigureResponse { get; }

        public Optional<Func<IComponentFactory, Task<T>>> DefaultNoRequestDataModelFactory { get; }
        public Optional<Func<IComponentFactory, IRequestData, Task<(T, Optional<Action<IHttpResponseMutator>>)>>> DefaultModelFactory { get; }
    }
}
