namespace Arke.SipEngine.FSM;

public interface IStateMachineState
{
    static State OffHook => new("OffHook");
    static State Initialization => new("Initialization");
    static State LanguagePrompts => new("LanguagePrompts");
    static State LanguageInput => new("LanguageInput");
    static State CallFlow => new("CallFlow");
    static State InCall => new("InCall");
    static State OnHold => new("OnHold");
    static State HangUp => new("HangUp");
    static State PlayingPrompt => new("PlayingPrompt");
    static State PlayingInterruptiblePrompt => new("PlayingInterruptiblePrompt");
    static State CapturingInput => new("CapturingInput");
    static State StoppingPlayback => new("StoppingPlayback");
    static State StartingRecording => new("StartingRecording");
    static State StoppingRecording => new("StoppingRecording");
    static State PlayingPromptOnCall => new("PlayingPromptOnCall");


}