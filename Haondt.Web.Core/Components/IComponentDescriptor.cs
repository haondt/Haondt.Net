using DotNext;
using Haondt.Web.Core.Http;

namespace Haondt.Web.Core.Components
{
    public interface IComponentDescriptor
    {
        public string Identity { get; }
        public string ViewPath { get; }
        public Optional<Action<IHttpResponseMutator>> ConfigureResponse { get; }
        public Optional<Func<IComponentFactory, IRequestData, Task<Result<IComponentModel>>>> DefaultModelFactory { get; }
        public Optional<Func<IComponentFactory, Task<Result<IComponentModel>>>> DefaultNoRequestDataModelFactory { get; }
    }
    public interface IComponentDescriptor<T> where T : IComponentModel
    {
        public string Identity { get; }
        public string ViewPath { get; }
        public Optional<Action<IHttpResponseMutator>> ConfigureResponse { get; }

        public Optional<Func<IComponentFactory, IRequestData, Task<Result<T>>>> DefaultModelFactory { get; }
        public Optional<Func<IComponentFactory, Task<Result<T>>>> DefaultNoRequestDataModelFactory { get; }
    }
}
