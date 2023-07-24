using System.Collections.Generic;
//using Microsoft.Coyote.Rewriting.Types.Collections.Generic;

namespace Arke.SipEngine.FSM
{
    //public enum State
    //{
    //    OffHook,
    //    Initialization,
    //    LanguagePrompts,
    //    LanguageInput,
    //    CallFlow,
    //    InCall,
    //    OnHold,
    //    HangUp,
    //    PlayingPrompt,
    //    PlayingInterruptiblePrompt,
    //    CapturingInput,
    //    StoppingPlayback,
    //    StartingRecording,
    //    StoppingRecording,
    //    PlayingPromptOnCall
    //}
    public class State : ExtensibleEnum, IStateMachineState
    {
        public State(string value):base(value){}

        #region Static Members

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
        #endregion

        #region Instance

    }

    #endregion


}
