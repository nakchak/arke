using System;
using System.Threading.Tasks;
using Arke.SipEngine.CallObjects;
using Newtonsoft.Json.Linq;
using Stateless;

namespace Arke.SipEngine.FSM
{
    public interface IStateMachine<TState, TTrigger>
    {
        StateMachine<TState, TTrigger> StateMachine { get; set; }
        Action<StateMachine<TState, TTrigger>, ICall<ICallInfo>, IPromptPlayer> Configure { get; set; }
        Task FireAsync(TTrigger trigger);
        void SetupFiniteStateMachine(ICall<ICallInfo> call, IPromptPlayer promptPlayer);
    }
}