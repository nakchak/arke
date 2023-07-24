using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Arke.IntegrationApi.CallObjects;
using Arke.SipEngine.CallObjects;
using Microsoft.Coyote.Actors;
using Stateless;
using Stateless.Graph;

namespace Arke.SipEngine.FSM
{
    public class CallStateMachine<TState, TTrigger> : IStateMachine<IStateMachineState, IStateMachineTrigger>

    {
        private ICall<ICallInfo> _call;
        private IPromptPlayer _promptPlayer;

        public CallStateMachine(IStateMachineState initialState)
        {
            StateMachine = new StateMachine<IStateMachineState, IStateMachineTrigger>(initialState);
        }
        public StateMachine<IStateMachineState, IStateMachineTrigger> StateMachine { get; set; }

        public async Task FireAsync(IStateMachineTrigger trigger)
        {
            await StateMachine.FireAsync(trigger);
        }

        public Action<StateMachine<IStateMachineState, IStateMachineTrigger>, ICall<ICallInfo>, IPromptPlayer> Configure { get; set; }

        public void SetupFiniteStateMachine(ICall<ICallInfo> call, IPromptPlayer promptPlayer)
        {
            _call = call;
            _promptPlayer = promptPlayer;
            if (Configure == null)
            {
                throw new Exception("Must provide statemachine config delegate");

            }
            Configure(StateMachine, call, promptPlayer);
            Console.WriteLine(UmlDotGraph.Format(StateMachine.GetInfo()));
        }

    }
}