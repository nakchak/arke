using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arke.SipEngine;
using Arke.SipEngine.CallObjects;
using Microsoft.AspNetCore.Mvc;

namespace Arke.ManagementApi.Controllers
{
    [Produces("application/json")]
    [Route("api/monitor")]
    [ApiController]
    public class MonitorController : ControllerBase
    {
        private readonly ICallFlowService<ICallInfo> _engine;

        public MonitorController(ICallFlowService<ICallInfo> callFlowEngine)
        {
            _engine = callFlowEngine;
        }

        [HttpGet]
        public async Task<JsonResult> GetAllCalls()
        {
            return new JsonResult(_engine.ConnectedLines.Select(c => c.Value.CallState));
        }


        [HttpGet("{callId}")]
        public async Task<JsonResult> GetCall(string callId)
        {
            return new JsonResult(_engine.ConnectedLines[callId].CallState);
        }
    }
}
