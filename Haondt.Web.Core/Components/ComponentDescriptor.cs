using DotNext;
using Haondt.Web.Core.Http;

namespace Haondt.Web.Core.Components
{
    public class ComponentDescriptor : IComponentDescriptor
    {
        public required string Identity { get; init; }
        public required string ViewPath { get; init; }
        public Optional<Action<IHttpResponseMutator>> ConfigureResponse { get; init; } = default;

        public Optional<Func<IComponentFactory, IRequestData, Task<Result<IComponentModel>>>> DefaultModelFactory { get; private init; }
        public Optional<Func<IComponentFactory, Task<Result<IComponentModel>>>> DefaultNoRequestDataModelFactory { get; private init; }

        public ComponentDescriptor(Func<IComponentFactory, IRequestData, Result<IComponentModel>> defaultModelFactory)
        {
            DefaultModelFactory = new((f, r) => Task.FromResult(defaultModelFactory(f, r)));
        }
        public ComponentDescriptor(Func<IComponentFactory, IRequestData, Task<Result<IComponentModel>>> defaultModelFactory)
        {
            DefaultModelFactory = new(defaultModelFactory);
        }
        public ComponentDescriptor(Func<IComponentFactory, Result<IComponentModel>> defaultModelFactory)
        {
            DefaultNoRequestDataModelFactory = new((f) => Task.FromResult(defaultModelFactory(f)));
        }
        public ComponentDescriptor(Func<IComponentFactory, Task<Result<IComponentModel>>> defaultModelFactory)
        {
            DefaultNoRequestDataModelFactory = new((f) => defaultModelFactory(f));
        }
        public ComponentDescriptor(Func<Result<IComponentModel>> defaultModelFactory)
        {
            DefaultNoRequestDataModelFactory = new((_) => Task.FromResult(defaultModelFactory()));
        }
        public ComponentDescriptor(Func<Task<Result<IComponentModel>>> defaultModelFactory)
        {
            DefaultNoRequestDataModelFactory = new((_) => defaultModelFactory());
        }
        public ComponentDescriptor(IComponentModel defaultModel)
        {
            DefaultNoRequestDataModelFactory = new((_) => Task.FromResult(new Result<IComponentModel>(defaultModel)));
        }
        public ComponentDescriptor(Result<IComponentModel> defaultModel)
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
        public Optional<Func<IComponentFactory, IRequestData, Task<Result<T>>>> DefaultModelFactory { get; private init; }
        public Optional<Func<IComponentFactory, Task<Result<T>>>> DefaultNoRequestDataModelFactory { get; private init; }
        Optional<Func<IComponentFactory, IRequestData, Task<Result<IComponentModel>>>> IComponentDescriptor.DefaultModelFactory => 
            DefaultModelFactory ? new (async (f, d) => (await DefaultModelFactory.Value(f, d)).Convert<IComponentModel>(m => m)) : default;
        Optional<Func<IComponentFactory, Task<Result<IComponentModel>>>> IComponentDescriptor.DefaultNoRequestDataModelFactory => 
            DefaultNoRequestDataModelFactory ? new (async (f) => (await DefaultNoRequestDataModelFactory.Value(f)).Convert<IComponentModel>(m => m)) : default;


        public ComponentDescriptor(Func<IComponentFactory, IRequestData, Result<T>> defaultModelFactory)
        {
            DefaultModelFactory = new((f, r) => Task.FromResult(defaultModelFactory(f, r)));
        }
        public ComponentDescriptor(Func<IComponentFactory, IRequestData, Task<Result<T>>> defaultModelFactory)
        {
            DefaultModelFactory = new(defaultModelFactory);
        }
        public ComponentDescriptor(Func<IComponentFactory, Result<T>> defaultModelFactory)
        {
            DefaultNoRequestDataModelFactory = new((f) => Task.FromResult(defaultModelFactory(f)));
        }
        public ComponentDescriptor(Func<IComponentFactory, Task<Result<T>>> defaultModelFactory)
        {
            DefaultNoRequestDataModelFactory = new((f) => defaultModelFactory(f));
        }
        public ComponentDescriptor(Func<Result<T>> defaultModelFactory)
        {
            DefaultNoRequestDataModelFactory = new((_) => Task.FromResult(defaultModelFactory()));
        }
        public ComponentDescriptor(Func<Task<Result<T>>> defaultModelFactory)
        {
            DefaultNoRequestDataModelFactory = new((_) => defaultModelFactory());
        }
        public ComponentDescriptor(Result<T> defaultModel)
        {
            DefaultNoRequestDataModelFactory = new((_) => Task.FromResult(defaultModel));
        }
        public ComponentDescriptor(T defaultModel)
        {
            DefaultNoRequestDataModelFactory = new((_) => Task.FromResult(new Result<T>(defaultModel)));
        }
        public ComponentDescriptor()
        {
        }
    }
}
