using Haondt.Web.Core.Http;
using Microsoft.AspNetCore.Components;

namespace Haondt.Web.Core.Components
{
    public abstract class Component : ComponentBase, IComponent
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        [Parameter, CascadingParameter]
        public IResponseData Response { get; set; }
        [Parameter, CascadingParameter]
        public IRequestData Request { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }

}
