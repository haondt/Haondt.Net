using Haondt.Web.Components;
using Haondt.Web.Core.Components;
using Haondt.Web.Core.Controllers;
using Haondt.Web.Core.Exceptions;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Haondt.Web.Controllers
{
        public class IndexController(
        IOptions<IndexSettings> options,
        IComponentFactory componentFactory) : BaseController
    {
        private readonly IndexSettings _indexSettings = options.Value;

        [Route("/")]
        public Task<IActionResult> Redirect()
        {
            return Get(_indexSettings.HomePage);
        }

        [Route("{page}")]
        public async Task<IActionResult> Get([FromRoute] string page)
        {
            var loader = await componentFactory.GetPlainComponent(new LoaderModel { Target = $"/{page}" });
            var index = await componentFactory.GetComponent(new IndexModel
            {
                Title = _indexSettings.SiteName,
                Content = loader.Value
            });

            return index.Value.CreateView(this);
        }
    }
}

