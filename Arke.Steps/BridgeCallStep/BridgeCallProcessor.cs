﻿using System;
using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.Bridging;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.BridgeCallStep
{
    public class BridgeCallProcessor : IStepProcessor
    {
        private const string NextStep = "NextStep";
        private IBridge _callBridge;
        private ICall<ICallInfo> _call;

        public string Name => "BridgeCall";

        public string GetBridgeId()
        {
            return _callBridge?.Id;
        }

        public async Task DoStepAsync(Step step, ICall<ICallInfo> call)
        {
            _call = call;
            if (!string.IsNullOrEmpty(call.CallState.Bridge.Id))
                await call.StopHoldingBridgeAsync().ConfigureAwait(false);
            _callBridge = await call.CreateBridgeAsync(BridgeType.NoDTMF).ConfigureAwait(false);
            call.CallState.CalledPartyAcceptTime = DateTimeOffset.Now;
            call.CallState.Bridge = _callBridge;
            call.InputProcessor.ChangeInputSettings(null);

            if (!await AreBothLinesStillConnected())
            {
                await call.FireStateChange(Trigger.FailedCallFlow);
                return;
            }
            await call.AddLineToBridgeAsync(call.CallState.IncomingSipChannel.Id as string, _callBridge.Id).ConfigureAwait(false);
            //await call.SipBridgingApi.MuteLineAsync(call.CallState.GetIncomingLineId());
            await call.AddLineToBridgeAsync(call.CallState.OutgoingSipChannel.Id as string, _callBridge.Id).ConfigureAwait(false);
            //await call.SipBridgingApi.MuteLineAsync(call.CallState.GetOutgoingLineId());

            call.AddStepToIncomingProcessQueue(step.GetStepFromConnector(NextStep));
            await call.FireStateChange(Trigger.NextCallFlowStep);
        }

        private async Task<bool> AreBothLinesStillConnected()
        {
            return await IsIncomingLineConnected() && await IsOutgoingLineConnected();
        }

        private async Task<bool> IsIncomingLineConnected()
        {
            try
            {
                var lineState = await _call.SipLineApi.GetLineStateAsync(_call.CallState.IncomingSipChannel.Id as string).ConfigureAwait(false);
                return lineState.ToLower() == "up";
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> IsOutgoingLineConnected()
        {
            try
            {
                var lineState = await _call.SipLineApi.GetLineStateAsync(_call.CallState.OutgoingSipChannel.Id as string).ConfigureAwait(false);
                return lineState.ToLower() == "up";
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
