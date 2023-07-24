using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arke.SipEngine.Api;
using Microsoft.AspNetCore.Mvc;

namespace Arke.SampleProject
{
    [Produces("application/json")]
    [Route("api/sample")]
    public class SampleController : ControllerBase, IManagementAPIController
    {
        public SampleController() { }

        public async Task<JsonResult> Get() => new JsonResult(new {Property = "Hello World"});
    }
}
