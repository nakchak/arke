using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Arke.IVR;
using Arke.SipEngine;
using Arke.SipEngine.Api;
using Arke.SipEngine.CallObjects;
using AsterNET.ARI;
using AsterNET.ARI.Models;
using Microsoft.Coyote;
using Microsoft.Coyote.SystematicTesting;
using Moq;
using Serilog;
using Xunit;

namespace Arke.Tests.CallTests.EventTests
{
    public class EnsureCallCleanupRunsOnCallTermination
    {

        [Fact]
        public async Task EnsureCallCleanupRunsWhenCallTerminatedByIncomingLine_WithCoyoteTesting()
        {
            var configuration = Configuration.Create().WithTestingIterations(50);
            var engine = TestingEngine.Create(configuration, EnsureCallCleanupRunsWhenCallTerminatedByIncomingLine);
            engine.Run();
        }

        [Fact]
        public async Task EnsureCallCleanupRunsWhenCallTerminatedByIncomingLine()
        {
            var ariClient = new Mock<AriClient>(new StasisEndpoint("", 0, "", "", false), "mock", true, false);
            
            ariClient.Setup(m => m.Connect(It.IsAny<bool>(), It.IsAny<int>()));

            var callState = Mock.Of<ICallInfo>();
            var call = Mock.Of<ICall<ICallInfo>>();
            var logger = Mock.Of<ILogger>();
            var sipApi = Mock.Of<ISipApiClient>();
            var callEngine = new ArkeCallFlowService<ICallInfo>(logger, ariClient.Object, sipApi);
            var cancelTokenSource = new CancellationTokenSource();
            var engineStarted = callEngine.Start(cancelTokenSource.Token);

            Assert.True(engineStarted);

            var callId = Guid.NewGuid().ToString();
            ariClient.Raise(m => m.OnStasisStartEvent += null, (ariClient, new StasisStartEvent()
            {
                Channel = new Channel()
                {
                    Id = callId
                }
            }));

            Assert.True(callEngine.ConnectedLines.ContainsKey(callId));
        }
    }
}
