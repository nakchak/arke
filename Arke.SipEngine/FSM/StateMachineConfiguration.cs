using System;
using Arke.SipEngine.CallObjects;
using Stateless;

namespace Arke.SipEngine.FSM;

public class StateMachineConfiguration : IStateMachineConfiguration
{
    public Action<StateMachine<IStateMachineState, IStateMachineTrigger>, ICall<ICallInfo>, IPromptPlayer> Configuration => Definition;

    public IStateMachine<IStateMachineState,IStateMachineTrigger> StateMachine => new CallStateMachine<State, Trigger>(IStateMachineState.OffHook);

    private static readonly Action<StateMachine<IStateMachineState, IStateMachineTrigger>, ICall<ICallInfo>, IPromptPlayer> Definition = (sm, call, prompt) =>
    {
        sm.Configure(IStateMachineState.OffHook)
            .Permit(IStateMachineTrigger.Answered, IStateMachineState.Initialization);

        sm.Configure(State.Initialization)
            .Permit(Trigger.BadConfig, State.HangUp)
            .Permit(Trigger.StartCallFlow, State.CallFlow);

        sm.Configure(State.LanguagePrompts)
            .Permit(Trigger.GetLanguageInput, State.LanguageInput)
            .Permit(Trigger.NextCallFlowStep, State.CallFlow)
            .Permit(Trigger.FailedCallFlow, State.HangUp)
            .Permit(Trigger.FinishCall, State.HangUp)
            .OnEntryAsync(call.ProcessCallLogicAsync);

        sm.Configure(State.LanguageInput)
            .Permit(Trigger.PlayLanguagePrompts, State.LanguagePrompts)
            .Permit(Trigger.NextCallFlowStep, State.CallFlow)
            .Permit(Trigger.FailedCallFlow, State.HangUp)
            .Permit(Trigger.FinishCall, State.HangUp);
               
                
        sm.Configure(State.CallFlow)
            .Permit(Trigger.PlayPrompt, State.PlayingPrompt)
            .Permit(Trigger.PlayInterruptiblePrompt, State.PlayingInterruptiblePrompt)
            .Permit(Trigger.CaptureInput, State.CapturingInput)
            .Permit(Trigger.PlaceOnHold, State.OnHold)
            .Permit(Trigger.PlayLanguagePrompts, State.LanguagePrompts)
            .PermitReentry(Trigger.NextCallFlowStep)
            .PermitReentry(Trigger.InputReceived)
            .PermitReentry(Trigger.FailedInputCapture)
            .Ignore(Trigger.FinishedPrompt)
            .Ignore(Trigger.PlayNextPrompt)
            .Permit(Trigger.FailedCallFlow, State.HangUp)
            .Permit(Trigger.FinishCall, State.HangUp)
            .Permit(Trigger.StartRecording, State.StartingRecording)
            .Permit(Trigger.StartTalking, State.InCall)
            .OnEntryAsync(call.ProcessCallLogicAsync);

        sm.Configure(State.OnHold)
            .Permit(Trigger.FinishCall, State.HangUp)
            .Permit(Trigger.NextCallFlowStep, State.CallFlow)
            .Permit(Trigger.FailedCallFlow, State.HangUp)
            .Permit(Trigger.StartTalking, State.InCall)
            .Ignore(Trigger.FinishedPrompt);

        sm.Configure(State.PlayingInterruptiblePrompt)
            .PermitReentry(Trigger.PlayNextPrompt)
            .Permit(Trigger.FinishedPrompt, State.CallFlow)
            .Permit(Trigger.PromptInterrupted, State.StoppingPlayback)
            .Permit(Trigger.FinishCall, State.HangUp)
            .Permit(Trigger.FailedCallFlow, State.HangUp)
            .Permit(Trigger.InputReceived, State.CallFlow)
            .Permit(Trigger.StartTalking, State.InCall)
            .Ignore(Trigger.PlayInterruptiblePrompt)
            .OnEntryAsync(prompt.PlayPromptsInQueueAsync);

        sm.Configure(State.StoppingPlayback)
            .Permit(Trigger.FinishedPrompt, State.CallFlow);

        sm.Configure(State.PlayingPromptOnCall)
            .Permit(Trigger.FinishedPrompt, State.InCall)
            .Permit(Trigger.FinishCall, State.HangUp)
            .Permit(Trigger.FailedCallFlow, State.HangUp)
            .Ignore(Trigger.InputReceived)
            .Ignore(Trigger.PlayInterruptiblePrompt)
            .Ignore(Trigger.PlayPrompt)
            .OnEntryAsync(prompt.PlayPromptsInQueueAsync);

        sm.Configure(State.PlayingPrompt)
            .PermitReentry(Trigger.PlayNextPrompt)
            .Ignore(Trigger.NextCallFlowStep)
            .Permit(Trigger.InputReceived, State.CallFlow)
            .Permit(Trigger.FinishedPrompt, State.CallFlow)
            .Permit(Trigger.FinishCall, State.HangUp)
            .Permit(Trigger.StartTalking, State.InCall)
            .Permit(Trigger.FailedCallFlow, State.HangUp)
            .Ignore(Trigger.PlayPrompt)
            .OnEntryAsync(prompt.PlayPromptsInQueueAsync);
        sm.Configure(State.CapturingInput)
            .Permit(Trigger.InputReceived, State.CallFlow)
            .Permit(Trigger.NextCallFlowStep, State.CallFlow)
            .Permit(Trigger.FailedInputCapture, State.CallFlow)
            .Permit(Trigger.FailedCallFlow, State.HangUp)
            .Permit(Trigger.FinishCall, State.HangUp)
            .Permit(Trigger.StartTalking, State.InCall)
            .Ignore(Trigger.FinishedPrompt)
            .Ignore(Trigger.PlayNextPrompt)
            .Ignore(Trigger.PromptInterrupted);
        sm.Configure(State.HangUp)
            .Ignore(Trigger.FinishCall)
            .Ignore(Trigger.PlayInterruptiblePrompt)
            .Ignore(Trigger.PlayPrompt)
            .Ignore(Trigger.PlayNextPrompt)
            .Ignore(Trigger.FailedCallFlow)
            .Ignore(Trigger.FinishedPrompt)
            .Ignore(Trigger.StartCallFlow)
            .Ignore(Trigger.PlaceOnHold)
            .Ignore(Trigger.PlayLanguagePrompts)
            .PermitIf(Trigger.NextCallFlowStep, State.CallFlow, () => !call.CallState.CallCanBeAbandoned,
                "Cannot abandon call yet.")
            .IgnoreIf(Trigger.NextCallFlowStep, () => call.CallState.CallCanBeAbandoned, "Abandoning Call")
            .Ignore(Trigger.CaptureInput)
            .OnEntryAsync(call.HangupAsync);

        sm.Configure(State.InCall)
            .Permit(Trigger.NextCallFlowStep, State.CallFlow)
            .Permit(Trigger.PlayPrompt, State.PlayingPromptOnCall)
            .Ignore(Trigger.InputReceived)
            .Ignore(Trigger.FinishedPrompt)
            .Permit(Trigger.FailedCallFlow, State.HangUp)
            .Permit(Trigger.FinishCall, State.HangUp);

        sm.Configure(State.StartingRecording)
            .Permit(Trigger.NextCallFlowStep, State.CallFlow)
            .Permit(Trigger.StartTalking, State.InCall)
            .Permit(Trigger.FinishCall, State.HangUp);
    };
}