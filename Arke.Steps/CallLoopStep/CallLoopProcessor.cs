using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Arke.DSL.Step;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.CallLoopStep
{
    public class CallLoopProcessor : IStepProcessor
    {
        public string Name => "CallLoop";
        private ICall<ICallInfo> _call;
        private Step _step;
        
        public async Task DoStepAsync(Step step, ICall<ICallInfo> call)
        {
            var callTimer = new Timer(1000d);
            callTimer.Elapsed += CallTimer_Elapsed;
            _call = call;
            _step = step;
            _call.SipApiClient.OnLineHangupAsyncEvent += SipApiClient_OnLineHangupEvent;
            await _call.FireStateChange(Trigger.StartTalking);
        }

        private async Task SipApiClient_OnLineHangupEvent(SipEngine.Api.ISipApiClient sender, SipEngine.Events.LineHangupEvent e)
        {
            if (e.LineId == _call.CallState.IncomingSipChannel.Id as string
                || e.LineId == _call.CallState.OutgoingSipChannel.Id as string)
            {
                _call.SipApiClient.OnLineHangupAsyncEvent -= SipApiClient_OnLineHangupEvent;
                _call.AddStepToIncomingProcessQueue(_step.GetStepFromConnector("NextIncomingStep"));
                _call.AddStepToOutgoingProcessQueue(_step.GetStepFromConnector("NextOutgoingStep"));
            }
        }

        private void CallTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            
        }
    }
}
