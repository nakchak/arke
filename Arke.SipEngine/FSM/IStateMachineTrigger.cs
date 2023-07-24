namespace Arke.SipEngine.FSM;

public interface IStateMachineTrigger
{
    static Trigger Answered => new("Answered");

    static Trigger BadConfig => new("BadConfig");

    static Trigger PlayLanguagePrompts => new("PlayLanguagePrompts");

    static Trigger GetLanguageInput => new("GetLanguageInput");

    static Trigger StartCallFlow => new("StartCallFlow");

    static Trigger ConnectedOutbound => new("ConnectedOutbound");

    static Trigger DialingOutbound => new("DialingOutbound");

    static Trigger InvalidCallFlowChoice => new("InvalidCallFlowChoice");

    static Trigger CouldNotConnectOutbound => new("CouldNotConnectOutbound");

    static Trigger FailedCallFlow => new("FailedCallFlow");

    static Trigger FinishCall => new("FinishCall");

    static Trigger CallRejected => new("CallRejected");

    static Trigger InputReceived => new("InputReceived");

    static Trigger FinishedPrompt => new("FinishedPrompt");

    static Trigger PromptInterrupted => new("PromptInterrupted");

    static Trigger PlayInterruptiblePrompt => new("PlayInterruptiblePrompt");

    static Trigger PlayPrompt => new("PlayPrompt");

    static Trigger PlayNextPrompt => new("PlayNextPrompt");

    static Trigger CaptureInput => new("CaptureInput");

    static Trigger FailedInputCapture => new("FailedInputCapture");

    static Trigger NextCallFlowStep => new("NextCallFlowStep");

    static Trigger PlaceOnHold => new("PlaceOnHold");

    static Trigger StartRecording => new("StartRecording");

    static Trigger StartTalking => new("StartTalking");
}