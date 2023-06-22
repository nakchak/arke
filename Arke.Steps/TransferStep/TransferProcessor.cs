using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.IntegrationApi.CallObjects;
using Arke.SipEngine.Api;
using Arke.SipEngine.BridgeName;
using Arke.SipEngine.Bridging;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Events;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.TransferStep
{
    public class TransferProcessor : IStepProcessor
    {
        public string Name => "Transfer";
        private const string NextStep = "NextStep";
        private ISipChannel transferChannel;
        private string outboundLineId;
        private ICall<ICallInfo> _call;
        private Step _step;

        public async Task DoStepAsync(Step step, ICall<ICallInfo> call)
        {
            _call = call;
            _step = step;
            var transferSettings = step.NodeData.Properties as TransferSettings;
            call.CallState.CallCanBeAbandoned = false;
            var transferLine =
                await call.SipLineApi.CreateOutboundCallAsync(transferSettings.DialString, transferSettings.Endpoint);

            transferChannel = await call.CreateTransferLine(transferLine);
            await Task.Delay(500);
            call.SipApiClient.OnLineHangupAsyncEvent += SipApiClientOnOnLineHangupEvent;

            switch (transferSettings.Direction)
            {
                case Direction.Outgoing:
                {
                    var transferBridge = await call.CreateBridgeAsync(BridgeType.WithDTMF);
                    outboundLineId = call.CallState.OutgoingSipChannel.Id as string;
                    await Task.Delay(500);
                    await call.AddLineToBridgeAsync(call.CallState.OutgoingSipChannel.Id as string, transferBridge.Id);
                    await Task.Delay(500);
                    await call.AddLineToBridgeAsync(transferChannel.Id.ToString(), transferBridge.Id);
                    break;
                }
                case Direction.Incoming:
                {
                    var transferBridge = await call.CreateBridgeAsync(BridgeType.WithDTMF);
                    await call.AddLineToBridgeAsync(call.CallState.IncomingSipChannel.Id as string, transferBridge.Id);
                    await call.AddLineToBridgeAsync(transferChannel.Id.ToString(), transferBridge.Id);
                    break;
                }
                case Direction.Both:
                    // bad config here?
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task SipApiClientOnOnLineHangupEvent(ISipApiClient sender, LineHangupEvent e)
        {
            if (e.LineId != transferChannel.Id.ToString() && e.LineId != outboundLineId) return;
            if (e.LineId != transferChannel.Id.ToString())
                await _call.SipLineApi.HangupLineAsync(transferChannel.Id.ToString());
            _call.CallState.CallCanBeAbandoned = true;
            GoToNextStep(_call, _step);
            await _call.FireStateChange(Trigger.NextCallFlowStep);
        }

        public void GoToNextStep(ICall<ICallInfo> call, Step step)
        {
            if (step.NodeData.Properties.Direction != Direction.Outgoing)
                call.AddStepToIncomingProcessQueue(step.GetStepFromConnector(NextStep));
            else
                call.AddStepToOutgoingProcessQueue(step.GetStepFromConnector(NextStep));
        }
    }
}
