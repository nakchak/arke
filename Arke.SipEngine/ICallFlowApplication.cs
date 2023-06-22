using Arke.SipEngine.CallObjects;

namespace Arke.SipEngine
{
    public interface ICallFlowApplication
    {
        ICall<ICallInfo> CallApi { get; set; }
        ICallInfo CallInfo { get; set; }
        void HandleCall();
    }
}
