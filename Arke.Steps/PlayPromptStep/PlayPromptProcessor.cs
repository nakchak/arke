using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.PlayPromptStep
{
    public class PlayPromptProcessor : IStepProcessor
    {
        private const string NextStep = "NextStep";
        public string Name => "PlayPrompt";
        private Direction _direction;

        public async Task DoStepAsync(Step step, ICall<ICallInfo> call)
        {
            var stepSettings = (PlayPromptSettings) step.NodeData.Properties;
            call.StepSettings = stepSettings;
            _direction = stepSettings.Direction;
            var nextStep = step.GetStepFromConnector(NextStep);
            call.Logger.Debug("Next step {stepId} {@Call}",nextStep, call.CallState);
            await call.PromptPlayer.DoStepAsync(stepSettings.GetPromptPlayerSettings(step, stepSettings.Direction));
            AddStepToProperQueue(nextStep, call);
            await call.FireStateChange(Trigger.PlayInterruptiblePrompt);
        }

        public void AddStepToProperQueue(int step, ICall<ICallInfo> call)
        {
            switch (_direction)
            {
                case Direction.Both:
                    call.AddStepToIncomingProcessQueue(step);
                    //call.CallState.AddStepToOutgoingQueue(step); // might not be needed, could be causing our double queue issue.
                    break;
                case Direction.Incoming:
                    call.AddStepToIncomingProcessQueue(step);
                    break;
                case Direction.Outgoing:
                    call.AddStepToOutgoingProcessQueue(step);
                    break;
                default:
                    call.AddStepToIncomingProcessQueue(step);
                    break;
            }
        }
    }
}
