using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Stateless;

namespace Arke.SampleProject;

public class CustomStateMachineConfig : IStateMachineConfiguration
{
    public Action<StateMachine<IStateMachineState, IStateMachineTrigger>, ICall<ICallInfo>, IPromptPlayer> Configuration => Definition;
    public IStateMachine<IStateMachineState, IStateMachineTrigger> StateMachine => new CallStateMachine<CustomStateMachineStates, CustomStateMachineTriggers>(CustomStateMachineStates.OffHook);

    private static Action<StateMachine<IStateMachineState, IStateMachineTrigger>, ICall<ICallInfo>, IPromptPlayer>
        Definition =>
        (sm, call, prompt) =>
        {
            sm.Configure(CustomStateMachineStates.OffHook)
                .Permit(IStateMachineTrigger.Answered, IStateMachineState.Initialization);

            sm.Configure(CustomStateMachineStates.CustomState)
                .Permit(CustomStateMachineTriggers.CustomTrigger, IStateMachineState.Initialization);
        };
}