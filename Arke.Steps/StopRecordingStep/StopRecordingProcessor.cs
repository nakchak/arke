using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.StopRecordingStep
{
    public class StopRecordingProcessor : IStepProcessor
    {
        public string Name => "StopRecording";
        private const string NextStep = "NextStep";

        public async Task DoStepAsync(Step settings, ICall<ICallInfo> call)
        {
            call.Logger.Information("Stop recording {LineId} {@Call}", call.CallState, call.CallState.IncomingSipChannel.Id);
            await call.RecordingManager.StopRecordingOnLine(call.CallState.IncomingSipChannel.Id as string);
            GoToNextStep(call, settings);
        }
        
        public void GoToNextStep(ICall<ICallInfo> call, Step step)
        {
            if (step.NodeData.Properties.Direction != Direction.Outgoing)
                call.AddStepToIncomingProcessQueue(step.GetStepFromConnector(NextStep));
            else
                call.AddStepToOutgoingProcessQueue(step.GetStepFromConnector(NextStep));
            call.FireStateChange(Trigger.NextCallFlowStep);
        }
    }
}
