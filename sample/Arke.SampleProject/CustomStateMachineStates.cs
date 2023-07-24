using Arke.SipEngine.FSM;

namespace Arke.SampleProject;

public class CustomStateMachineStates : ExtensibleEnum, IStateMachineState
{
    public CustomStateMachineStates(string value) : base(value) { }
    public static CustomStateMachineStates CustomState => new(nameof(CustomState));
    public static State OffHook => IStateMachineState.OffHook;
    public static State Initialization => IStateMachineState.Initialization;
    public static State LanguagePrompts => IStateMachineState.LanguagePrompts;
    public static State LanguageInput => IStateMachineState.LanguageInput;
    public static State CallFlow => IStateMachineState.CallFlow;
    public static State InCall => IStateMachineState.InCall;
    public static State OnHold => IStateMachineState.OnHold;
    public static State HangUp => IStateMachineState.HangUp;
    public static State PlayingPrompt => IStateMachineState.PlayingPrompt;
    public static State PlayingInterruptiblePrompt => IStateMachineState.PlayingInterruptiblePrompt;
    public static State CapturingInput => IStateMachineState.CapturingInput;
    public static State StoppingPlayback => IStateMachineState.StoppingPlayback;
    public static State StartingRecording => IStateMachineState.StartingRecording;
    public static State StoppingRecording => IStateMachineState.StoppingRecording;
    public static State PlayingPromptOnCall => IStateMachineState.PlayingPromptOnCall;

}