using Haondt.UI.Shared.ModelBinders;
using Microsoft.AspNetCore.Mvc;

namespace Haondt.Web.Core.Extensions
{
    public static class MvcOptionsExtensions
    {
        public static MvcOptions AddHaondtWebCoreOptions(this MvcOptions options)
        {
            options.ModelBinderProviders.Insert(0, new AbsoluteDateTimeModelBinderProvider());
            return options;
        }
    }
}
