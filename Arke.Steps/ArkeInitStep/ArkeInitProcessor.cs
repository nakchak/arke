using System;
using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.SipEngine.Api;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Exceptions;
using Arke.SipEngine.Processors;

namespace Arke.Steps.ArkeInitStep
{
    public class ArkeInitProcessor : IStepProcessor
    {
        private const string FailStep = "FailStep";
        private const string NextStep = "NextStep";
        private readonly ISipLineApi _sipLineApi;
        private ICall<ICallInfo> _call;
        private ArkeInitSettings _settings;

        public string Name => "ArkeInit";

        public ArkeInitProcessor(ISipLineApi sipLineApi)
        {
            _sipLineApi = sipLineApi;
        }

        public async Task DoStepAsync(Step step, ICall<ICallInfo> call)
        {
            _settings = (ArkeInitSettings) step.NodeData.Properties;
            _call = call;
            try
            {
                await SetEndpointFromChannelVariable();
            }
            catch (Exception e) when (e is NotFoundException)
            {
                _call.CallState.TerminationCode = TerminationCode.InvalidDeviceConfig;
                _call.AddStepToIncomingProcessQueue(step.GetStepFromConnector(FailStep));
                await _call.FireStateChange(SipEngine.FSM.Trigger.FailedCallFlow);
                return;
            }
            
            _call.AddStepToIncomingProcessQueue(step.GetStepFromConnector(NextStep));
            await _call.FireStateChange(SipEngine.FSM.Trigger.NextCallFlowStep);
        }

        
        public async Task SetEndpointFromChannelVariable()
        {
            await GetEndpointFromAri();
            _call.Logger.Debug("Phone connected at Endpoint {Endpoint} PortID: {PortId} {@Call}", _call.CallState.Endpoint, _call.CallState.PortId, _call.CallState);
        }

        private async Task GetEndpointFromAri()
        {
            try
            {
                _call.CallState.Endpoint =
                    await _sipLineApi.GetEndpointAsync(_call.CallState.IncomingSipChannel.Id as string);
                _call.CallState.PortId =
                    await _sipLineApi.GetLineVariableAsync(_call.CallState.IncomingSipChannel.Id as string,
                    "CALLERID(num)");
            }
            catch (Exception ex)
            {
                _call.Logger.Error(ex, "Exception getting the Endpoint from ARI {@Call}", _call.CallState);
                throw new NotFoundException("Exception getting the Endpoint from ARI", ex);
            }
        }
    }
}
