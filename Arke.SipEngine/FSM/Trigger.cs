namespace Arke.SipEngine.FSM
{
    //public enum TriggerEnum
    //{
    //    Answered
    //    BadConfig
    //    PlayLanguagePrompts
    //    GetLanguageInput
    //    StartCallFlow
    //    ConnectedOutbound
    //    DialingOutbound
    //    InvalidCallFlowChoice
    //    CouldNotConnectOutbound
    //    FailedCallFlow
    //    FinishCall
    //    CallRejected
    //    InputReceived
    //    FinishedPrompt
    //    PromptInterrupted
    //    PlayInterruptiblePrompt
    //    PlayPrompt
    //    PlayNextPrompt
    //    CaptureInput
    //    FailedInputCapture
    //    NextCallFlowStep
    //    PlaceOnHold
    //    StartRecording
    //    StartTalking
    //}

    public class Trigger : ExtensibleEnum, IStateMachineTrigger
    {
        public Trigger(string value) : base(value) { }
        public static Trigger Answered => IStateMachineTrigger.Answered;

        public static Trigger BadConfig => IStateMachineTrigger.BadConfig;

        public static Trigger PlayLanguagePrompts => IStateMachineTrigger.PlayLanguagePrompts;

        public static Trigger GetLanguageInput => IStateMachineTrigger.GetLanguageInput;

        public static Trigger StartCallFlow => IStateMachineTrigger.StartCallFlow;

        public static Trigger ConnectedOutbound => IStateMachineTrigger.ConnectedOutbound;

        public static Trigger DialingOutbound => IStateMachineTrigger.DialingOutbound;

        public static Trigger InvalidCallFlowChoice => IStateMachineTrigger.InvalidCallFlowChoice;

        public static Trigger CouldNotConnectOutbound => IStateMachineTrigger.CouldNotConnectOutbound;

        public static Trigger FailedCallFlow => IStateMachineTrigger.FailedCallFlow;

        public static Trigger FinishCall => IStateMachineTrigger.FinishCall;

        public static Trigger CallRejected => IStateMachineTrigger.CallRejected;

        public static Trigger InputReceived => IStateMachineTrigger.InputReceived;

        public static Trigger FinishedPrompt => IStateMachineTrigger.FinishedPrompt;

        public static Trigger PromptInterrupted => IStateMachineTrigger.PromptInterrupted;

        public static Trigger PlayInterruptiblePrompt => IStateMachineTrigger.PlayInterruptiblePrompt;

        public static Trigger PlayPrompt => IStateMachineTrigger.PlayPrompt;

        public static Trigger PlayNextPrompt => IStateMachineTrigger.PlayNextPrompt;

        public static Trigger CaptureInput => IStateMachineTrigger.CaptureInput;

        public static Trigger FailedInputCapture => IStateMachineTrigger.FailedInputCapture;

        public static Trigger NextCallFlowStep => IStateMachineTrigger.NextCallFlowStep;

        public static Trigger PlaceOnHold => IStateMachineTrigger.PlaceOnHold;

        public static Trigger StartRecording => IStateMachineTrigger.StartRecording;

        public static Trigger StartTalking => IStateMachineTrigger.StartTalking;

    }
}
