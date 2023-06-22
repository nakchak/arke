﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arke.DependencyInjection;
using Arke.DSL.Step;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Processors;

namespace Arke.IVR.DSL
{
    public class DslProcessor
    {
        public Dictionary<int,Step> Dsl { get; set; }
        private readonly ICall<ICallInfo> _call;

        public DslProcessor(ICall<ICallInfo> call)
        {
            _call = call;
            Dsl = new Dictionary<int, Step>();
        }

        public async Task ProcessStepAsync(int stepIndex)
        {
            if (!Dsl.ContainsKey(stepIndex))
                throw new ArgumentOutOfRangeException(nameof(stepIndex), "Step Index does not exist");
            var step = Dsl.Single(i => i.Key == stepIndex).Value;
            
            var stepName = step.NodeData.Category + "Processor";
            var stepType = AppDomain.CurrentDomain.GetAssemblies()
                // Xunit assemblies cause issues during unit tests, so omit from assembly search.
                // and apparently datadog serialization is causing issues on load too so leave those out.
                .Where(assembly => !assembly.FullName.Contains("xunit")
                && !assembly.FullName.Contains("MsgPack.Serialization"))
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IStepProcessor).IsAssignableFrom(type))
                .Single(type => type.Name == stepName);

            var stepProcessor = ObjectContainer.GetInstance().GetObjectInstance(stepType);
            _call.Logger.Debug($"{_call.CallId}: Processing Step: {stepType.FullName}");
            await ((IStepProcessor) stepProcessor).DoStepAsync(step, _call);
        }
    }
}
