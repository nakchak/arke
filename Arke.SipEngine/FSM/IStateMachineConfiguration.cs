using System;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Stateless;

namespace Arke.SipEngine.FSM;

public interface IStateMachineConfiguration
{
    Action<StateMachine<IStateMachineState, IStateMachineTrigger>, ICall<ICallInfo>, IPromptPlayer> Configuration { get; }
    IStateMachine<IStateMachineState, IStateMachineTrigger> StateMachine { get; }
}