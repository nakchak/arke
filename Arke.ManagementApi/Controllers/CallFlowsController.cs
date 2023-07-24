using System;
using System.Collections.Generic;
using System.Text;
using Arke.SipEngine.Api;
using Microsoft.AspNetCore.Mvc;

namespace Arke.ManagementApi.Controllers
{
    [Produces("application/json")]
    [Route("api/callflows")]
    [ApiController]
    public class CallFlowsController : ControllerBase, IManagementAPIController
    {
    {
        public CallFlowsController() { }

        [HttpGet]
        public List<object> Get()
        {
            return new List<object>();
        }
    }
}
