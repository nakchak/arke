﻿using System;
using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;

namespace Arke.Steps.StartRecordingStep
{
    public class StartRecordingLineProcessor : IStepProcessor
    {
        private ICall<ICallInfo> _call;
        private Step _step;
        private const string NextStep = "NextStep";

        public string Name => "StartRecordingLine";
        
        public async Task DoStepAsync(Step step, ICall<ICallInfo> call)
        {
            _call = call;
            _step = step;
            try
            {
                await StartAllRecordings();
                GoToNextStep();
            }
            catch (Exception ex)
            {
                if (ex.InnerException?.InnerException != null)
                    _call.Logger.Debug(ex.InnerException.InnerException.Message);
                _call.Logger.Error(ex, "Error processing step {stepId} {@Call}", step.NodeData.Key, call.CallState);
            }

        }

        protected virtual async Task StartAllRecordings()
        {
            foreach (var recordingItem in ((StartRecordingLineSettings)_step.NodeData.Properties).ItemsToRecord)
            {
                await StartRecording(recordingItem);
            }
        }

        protected virtual async Task StartRecording(RecordingItems recordingItem)
        {
            switch (recordingItem)
            {
                case RecordingItems.InboundLine:
                    await StartRecordingOnInboundLine();
                    break;
                case RecordingItems.OutboundLine:
                    await StartRecordingOnOuboundLine();
                    break;
                case RecordingItems.Bridge:
                    await StartRecordingOnBridge();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void GoToNextStep()
        {
            var stepSettings = _step.NodeData.Properties as StartRecordingLineSettings;
            if (stepSettings.Direction != Direction.Outgoing)
                _call.AddStepToIncomingProcessQueue(_step.GetStepFromConnector(NextStep));
            else
                _call.AddStepToOutgoingProcessQueue(_step.GetStepFromConnector(NextStep));
            _call.FireStateChange(Trigger.NextCallFlowStep);
        }

        public async Task StartRecordingOnInboundLine()
        {
            if (_call.CallState.IncomingSipChannel?.Id != null)
            {
                _call.Logger.Information("Start recording on inbound line {@Call} {LineId}", _call.CallState, _call.CallState.IncomingSipChannel.Id);
                await _call.StartRecordingOnLineAsync(_call.CallState.IncomingSipChannel.Id as string, "I");
            }
        }

        public async Task StartRecordingOnOuboundLine()
        {
            if (_call.CallState.OutgoingSipChannel?.Id != null)
            {
                _call.Logger.Information("Start recording on outbound line {@Call} {LineId}", _call.CallState, _call.CallState.OutgoingSipChannel.Id);
                await _call.StartRecordingOnLineAsync(_call.CallState.OutgoingSipChannel.Id as string, "O");
            }            
        }

        public async Task StartRecordingOnBridge()
        {
            if (_call.CallState.Bridge?.Id != null)
            {
                _call.Logger.Information("Start recording on bridge: {@Call} {BridgeId}", _call.CallState, _call.CallState.Bridge.Id);
                await _call.StartRecordingOnBridgeAsync(_call.CallState.Bridge.Id);
            }
        }
    }
}
