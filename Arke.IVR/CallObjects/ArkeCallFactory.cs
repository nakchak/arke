using System;
using Arke.DependencyInjection;
using Arke.SipEngine.CallObjects;
using Arke.ARI.Models;

namespace Arke.IVR.CallObjects
{
    public static class ArkeCallFactory
    {
        public static ICall<ICallInfo> CreateArkeCall(Channel channel)
        {
            var call = ObjectContainer.GetInstance().GetObjectInstance<ICall<ICallInfo>>();
            call.CallId = Guid.NewGuid();
            call.CallState = ObjectContainer.GetInstance().GetObjectInstance<ICallInfo>();
            call.CallState.IncomingSipChannel = new ArkeSipChannel { Channel = channel };
            call.CallState.CallCanBeAbandoned = true;

            return call;
        }
    }
}
