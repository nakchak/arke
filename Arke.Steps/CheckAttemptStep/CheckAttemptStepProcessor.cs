using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.CheckAttemptStep
{
    public class CheckAttemptStepProcessor : IStepProcessor
    {
        private const string MaxAttemptsStep = "MaxAttemptsStep";
        private const string NextStep = "NextStep";
        public string Name => "CheckAttemptStep";
        public Task DoStepAsync(Step step, ICall<ICallInfo> call)
        {
            var stepSettings = step.NodeData.Properties as CheckAttemptStepSettings;

            if (call.CallState.AttemptCount >= stepSettings.MaxAttempts)
            {
                if (stepSettings.Direction != Direction.Outgoing)
                    call.AddStepToIncomingProcessQueue(step.GetStepFromConnector(MaxAttemptsStep));
                else
                    call.AddStepToOutgoingProcessQueue(step.GetStepFromConnector(MaxAttemptsStep));
            }
            else
            {
                call.CallState.AttemptCount++;
                if (stepSettings.Direction != Direction.Outgoing)
                    call.AddStepToIncomingProcessQueue(step.GetStepFromConnector(NextStep));
                else
                    call.AddStepToOutgoingProcessQueue(step.GetStepFromConnector(NextStep));
            }
            call.FireStateChange(Trigger.NextCallFlowStep);
            return Task.CompletedTask;
        }
    }
}
