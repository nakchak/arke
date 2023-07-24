using System.Collections.Generic;
using System.Threading;
using Arke.IntegrationApi.CallObjects;
using Arke.SipEngine.CallObjects;

namespace Arke.SipEngine
{
    public interface ICallFlowService<T> where T : ICallInfo
    {
        Dictionary<string, ICall<T>> ConnectedLines { get; }
        bool Start(CancellationToken cancellationToken);
        bool Stop();
        bool Continue();
        bool Pause();
    }
}
