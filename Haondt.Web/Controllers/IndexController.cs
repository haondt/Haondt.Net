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
    public class IndexController(IOptions<IndexSettings> options, IIndexModelComponentFactory indexFactory) : BaseController
    {

        [Route("/")]
        public Task<IActionResult> Redirect()
        {
            return Get(options.Value.HomePage);
        }

        [Route("{page}")]
        public async Task<IActionResult> Get([FromRoute] string page)
        {
            var indexComponent = await indexFactory.GetComponent(page);
            return indexComponent.Value.CreateView(this);
        }
    }
}

