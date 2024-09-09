using Haondt.Core.Models;
using Haondt.Web.Core.Http;

namespace Haondt.Web.Core.Components
{
    public class ComponentDescriptor : IComponentDescriptor
    {
        public required string Identity { get; init; }
        public required string ViewPath { get; init; }
        public Optional<Action<IHttpResponseMutator>> ConfigureResponse { get; init; } = default;

        public Optional<Func<IComponentFactory, IRequestData, Task<IComponentModel>>> DefaultModelFactory { get; private init; }
        public Optional<Func<IComponentFactory, Task<IComponentModel>>> DefaultNoRequestDataModelFactory { get; private init; }

        public ComponentDescriptor(Func<IComponentFactory, IRequestData, IComponentModel> defaultModelFactory)
        {
            DefaultModelFactory = new((f, r) => Task.FromResult(defaultModelFactory(f, r)));
        }
        public ComponentDescriptor(Func<IComponentFactory, IRequestData, Task<IComponentModel>> defaultModelFactory)
        {
            DefaultModelFactory = new(defaultModelFactory);
        }
        public ComponentDescriptor(Func<IComponentFactory, IComponentModel> defaultModelFactory)
        {
            DefaultNoRequestDataModelFactory = new((f) => Task.FromResult(defaultModelFactory(f)));
        }
        public ComponentDescriptor(Func<IComponentFactory, Task<IComponentModel>> defaultModelFactory)
        {
            DefaultNoRequestDataModelFactory = new((f) => defaultModelFactory(f));
        }
        public ComponentDescriptor(Func<IComponentModel> defaultModelFactory)
        {
            DefaultNoRequestDataModelFactory = new((_) => Task.FromResult(defaultModelFactory()));
        }
        public ComponentDescriptor(Func<Task<IComponentModel>> defaultModelFactory)
        {
            DefaultNoRequestDataModelFactory = new((_) => defaultModelFactory());
        }
        public ComponentDescriptor(IComponentModel defaultModel)
        {
            DefaultNoRequestDataModelFactory = new((_) => Task.FromResult(defaultModel));
        }

        public ComponentDescriptor()
        {
        }
    }
    public class ComponentDescriptor<T> : IComponentDescriptor, IComponentDescriptor<T> where T : IComponentModel
    {
        public static string TypeIdentity { get; } = typeof(T).FullName ?? typeof(T).Name;
        public string Identity => TypeIdentity;
        public required string ViewPath { get; init; }
        public Optional<Action<IHttpResponseMutator>> ConfigureResponse { get; init; } = default;
        public Optional<Func<IComponentFactory, IRequestData, Task<T>>> DefaultModelFactory { get; private init; }
        public Optional<Func<IComponentFactory, Task<T>>> DefaultNoRequestDataModelFactory { get; private init; }
        Optional<Func<IComponentFactory, IRequestData, Task<IComponentModel>>> IComponentDescriptor.DefaultModelFactory => 
            DefaultModelFactory.HasValue ? new (async (f, d) => (await DefaultModelFactory.Value(f, d))) : default;
        Optional<Func<IComponentFactory, Task<IComponentModel>>> IComponentDescriptor.DefaultNoRequestDataModelFactory => 
            DefaultNoRequestDataModelFactory.HasValue ? new (async (f) => (await DefaultNoRequestDataModelFactory.Value(f))) : default;


        public ComponentDescriptor(Func<IComponentFactory, IRequestData, T> defaultModelFactory)
        {
            DefaultModelFactory = new((f, r) => Task.FromResult(defaultModelFactory(f, r)));
        }
        public ComponentDescriptor(Func<IComponentFactory, IRequestData, Task<T>> defaultModelFactory)
        {
            DefaultModelFactory = new(defaultModelFactory);
        }
        public ComponentDescriptor(Func<IComponentFactory, T> defaultModelFactory)
        {
            DefaultNoRequestDataModelFactory = new((f) => Task.FromResult(defaultModelFactory(f)));
        }
        public ComponentDescriptor(Func<IComponentFactory, Task<T>> defaultModelFactory)
        {
            DefaultNoRequestDataModelFactory = new((f) => defaultModelFactory(f));
        }
        public ComponentDescriptor(Func<T> defaultModelFactory)
        {
            DefaultNoRequestDataModelFactory = new((_) => Task.FromResult(defaultModelFactory()));
        }
        public ComponentDescriptor(Func<Task<T>> defaultModelFactory)
        {
            DefaultNoRequestDataModelFactory = new((_) => defaultModelFactory());
        }
        public ComponentDescriptor(T defaultModel)
        {
            DefaultNoRequestDataModelFactory = new((_) => Task.FromResult(defaultModel));
        }
        public ComponentDescriptor()
        {
        }
    }
}
