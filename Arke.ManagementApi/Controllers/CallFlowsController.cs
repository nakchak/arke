using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Arke.ManagementApi.Controllers
{
    [Produces("application/json")]
    [Route("api/callflows")]
    [ApiController]
    public class CallFlowsController : ControllerBase
    {
        public CallFlowsController() { }

        [HttpGet]
        public List<object> Get()
        {
            return new List<object>();
        }
    }
}
