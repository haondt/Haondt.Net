using DotNext;
using Haondt.Web.Core.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haondt.Web.Core.Components
{
    public class Component : IComponent
    {
        public required string ViewPath { get; init; }
        public IComponentModel? Model { get; internal init; } 
        public Optional<Action<IHttpResponseMutator>> ConfigureResponse { get; internal init; }
    }

    public class Component<T> : IComponent, IComponent<T> where T : IComponentModel
    {
        public required string ViewPath { get; init; }
        public T? Model { get; internal init; } 
        public Optional<Action<IHttpResponseMutator>> ConfigureResponse { get; internal init; }
        IComponentModel? IComponent.Model => Model;
    }
}
