using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.IntegrationApi.CallObjects;
using Arke.IVR.Bridging;
using Arke.IVR.DSL;
using Arke.IVR.Input;
using Arke.IVR.Prompts;
using Arke.IVR.Recording;
using Arke.SipEngine.Api;
using Arke.SipEngine.Bridging;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Device;
using Arke.SipEngine.Events;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;
using Arke.SipEngine.Prompts;
using AsterNET.ARI.Models;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;

namespace Arke.IVR.CallObjects
{
    public class ArkeCall<T> : ICall<T> where T : class, ICallInfo
    {
        private readonly ArkeBridgeFactory _arkeBridgeFactory;
        private readonly AsteriskPhoneInputHandler _asteriskPhoneInputHandler;
        private readonly Dictionary<string, string> _logFields;
        private readonly ArkePromptPlayer _promptPlayer;
        private T _callState;
        private CancellationToken _cancellationToken;
        
        public ArkeCall(ISipApiClient sipApiClient, ISipLineApi sipLineApi, ISipBridgingApi sipBridgeApi,
            ISipPromptApi sipPromptApi, ISipRecordingApi sipRecordingApi, ILogger logger)
        {
            Logger = logger;
            SipApiClient = sipApiClient;
            SipLineApi = sipLineApi;
            SipBridgingApi= sipBridgeApi;
            _logFields = new Dictionary<string, string>();
            _promptPlayer = new ArkePromptPlayer(this as ICall<ICallInfo>, sipPromptApi);
            _asteriskPhoneInputHandler = new AsteriskPhoneInputHandler(this as ICall<ICallInfo>, _promptPlayer);
            RecordingManager = new ArkeRecordingManager(sipRecordingApi, this as ICall<ICallInfo>);
            DslProcessor = new DslProcessor(this as ICall<ICallInfo>);
            _arkeBridgeFactory = new ArkeBridgeFactory(SipBridgingApi);
            CallStateMachine = new CallStateMachine(this as ICall<ICallInfo>, _promptPlayer);
            CallStateMachine.SetupFiniteStateMachine();
            LanguageSelectionPromptPlayer = new AsteriskLanguageSelectionPromptPlayer(this as ICall<ICallInfo>, sipPromptApi, sipApiClient);
        }
        
        public Guid CallId { get; set; }
        public virtual T CallState
        {
            get => _callState;
            set
            {
                var state = value;
                if (state != null)
                    _callState = state;
            }
        }

        public CallStateMachine CallStateMachine { get; set; }
        public DslProcessor DslProcessor { get; set; }
        public IPhoneInputHandler InputProcessor => _asteriskPhoneInputHandler;
        public ILanguageSelectionPromptPlayer LanguageSelectionPromptPlayer { get; private set; }
        public Dictionary<string, string> LogData => _logFields;
        public ILogger Logger { get; }
        public IPromptPlayer PromptPlayer => _promptPlayer;
        public IRecordingManager RecordingManager { get; set; }
        public NodeProperties StepSettings { get; set; }
        public ISipApiClient SipApiClient { get; set; }
        public ISipBridgingApi SipBridgingApi { get; set; }
        public ISipLineApi SipLineApi { get; set; }
        public string SipProviderId { get; set; }

        public async Task AddLineToBridgeAsync(string lineId, string bridgeId)
        {
            await _arkeBridgeFactory.AddLineToBridge(lineId, bridgeId);
        }

        public void AddOrUpdateStepIdToLogFields(int stepId)
        {
            if (_logFields.ContainsKey("StepId"))
                _logFields["StepId"] = stepId.ToString();
            else
                _logFields.Add("StepId", stepId.ToString());
        }

        public void AddStepToIncomingProcessQueue(int stepNumber)
        {
            _callState.IncomingLineQueue.Enqueue(stepNumber);
        }

        public void AddStepToOutgoingProcessQueue(int stepNumber)
        {
            _callState.OutgoingLineQueue.Enqueue(stepNumber);
        }

        public async Task Answer()
        {
            _logFields.Add("ChannelID", _callState.IncomingSipChannel.Id as string);
            Logger.Debug("Answering Channel");
            var answerCall = SipLineApi.AnswerLineAsync(_callState.IncomingSipChannel.Id as string);
            await answerCall;
            Logger.Debug("Channel Answered");
            _callState.TimeOffHook = DateTimeOffset.Now;
            _logFields.Add("TimeDeviceOffHook", _callState.TimeOffHook.ToString("s"));
            await CallStateMachine.FireAsync(Trigger.Answered);
            Logger.Debug("New Call Answered");
        }

        private async Task AriClient_OnStasisEndEvent(ISipApiClient sipApiClient, LineHangupEvent e)
        {
            if (_callState.IncomingSipChannel == null)
                return;
            if (e.LineId != _callState.IncomingSipChannel.Id as string)
                return;
            Logger.Debug("OnStasisEndEvent");
            await CallStateMachine.FireAsync(Trigger.FinishCall);
            SipApiClient.OnDtmfReceivedEvent -= _asteriskPhoneInputHandler.AriClient_OnChannelDtmfReceivedEvent;
            SipApiClient.OnPromptPlaybackFinishedAsyncEvent -= _promptPlayer.AriClient_OnPlaybackFinishedEvent;
            
        }

        public async Task<IBridge> CreateBridgeAsync(BridgeType bridgeType)
        {
            return await _arkeBridgeFactory.CreateBridge(bridgeType);
        }

        private async Task DisposeOfBridgeApi()
        {
            if (CallState.Bridge?.Id != null)
            {
                try
                {
                    await SipBridgingApi.DestroyBridgeAsync(CallState.Bridge.Id);
                }
                catch (Exception e)
                {
                    Logger.Warning($"Problem destroying bridge: {e.Message}");
                }
            }
        }

        private async Task DisposeOfCallServices()
        {
            await DisposeOfBridgeApi();
            await DisposeOfOutgoingSipLineChannel();
            await DisposeOfIncomingSipLineChannel();
        }

        private async Task DisposeOfIncomingSipLineChannel()
        {
            if (_callState.IncomingSipChannel == null)
            {
                return;
            }
            try
            {
                await SipLineApi.HangupLineAsync(_callState.IncomingSipChannel.Id as string);
            }
            catch (Exception e)
            {
                Logger.Warning($"Problem while hanging up incoming line: {e.Message}");
            }
        }
        
        private async Task DisposeOfOutgoingSipLineChannel()
        {
            if (_callState.OutgoingSipChannel != null)
            {
                try
                {
                    await SipLineApi.HangupLineAsync(_callState.OutgoingSipChannel.Id as string);
                }
                catch (Exception e)
                {
                    Logger.Warning($"Problem while hanging up outgoing line: {e.Message}");
                }
            }
        }

        public async Task FireStateChange(Trigger trigger)
        {
            await CallStateMachine.FireAsync(trigger);
        }

        public State GetCurrentState()
        {
            return CallStateMachine.StateMachine.State;
        }

        public async Task HangupAsync()
        {
            Logger.Information("HangupAsync");

            await DisposeOfCallServices();
        }

        public event Action<ICall<T>, OnWorkflowStepEvent> OnWorkflowStep;

        public virtual async Task ProcessCallLogicAsync()
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                await ForceCallEndAsync();
                return;
            }

            if (_callState.IncomingLineQueue.Count > 0)
            {
                Logger.Debug("Processing next step on incoming line.");
                var step = _callState.IncomingLineQueue.Dequeue();
                AddOrUpdateStepIdToLogFields(step);
                Logger.Debug($"Processing Step ID {step}");
                await DslProcessor.ProcessStepAsync(step).ConfigureAwait(false);
                OnWorkflowStep?.Invoke(this, new OnWorkflowStepEvent()
                {
                    LineId = _callState.IncomingSipChannel.Id as string,
                    StepId = step
                });
            }
            if (_callState.ProcessOutgoingQueue &&
                _callState.OutgoingLineQueue.Count > 0)
            {
                Logger.Debug("Processing next step on outgoing line.");
                var step = _callState.OutgoingLineQueue.Dequeue();
                AddOrUpdateStepIdToLogFields(step);
                await DslProcessor.ProcessStepAsync(step).ConfigureAwait(false);
                OnWorkflowStep?.Invoke(this, new OnWorkflowStepEvent()
                {
                    LineId = _callState.OutgoingSipChannel.Id as string,
                    StepId = step
                });
            }
        }

        public async Task RunCallScriptAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _cancellationToken.Register(async () => await HangupAsync());
            await Answer();
            SetupSuccessfulCallStartEvents();
            await StartCallFlowDslProcessor();
            SetFileNameForCall();
            await StartConnectionStep();
            await StartTheCallFlow();
        }

        public void SetCallLanguage(LanguageData languageData)
        {
            _promptPlayer.SetLanguageCode(languageData);
        }

        private async Task StartConnectionStep()
        {
            await DslProcessor.ProcessStepAsync(0);
        }

        private void SetFileNameForCall()
        {
            _callState.FileName =
                $"{ArkeCallFlowService<T>.GetConfigValue("appSettings:AsteriskServerID")}_{_callState.Endpoint}_{_callState.TimeOffHook:yyyyMMddHHmmss}";
        }

        private void SetupSuccessfulCallStartEvents()
        {
            CallState.TerminationCode = TerminationCode.NormalCallCompletion;
            _callState.StepAttempts = 0;
            SipApiClient.OnDtmfReceivedEvent += _asteriskPhoneInputHandler.AriClient_OnChannelDtmfReceivedEvent;
            SipApiClient.OnPromptPlaybackFinishedAsyncEvent += _promptPlayer.AriClient_OnPlaybackFinishedEvent;
            SipApiClient.OnLineHangupAsyncEvent += AriClient_OnStasisEndEvent;
            SipApiClient.OnRecordingFinishedAsyncEvent += RecordingManager.AriClient_OnRecordingFinishedEvent;
        }

        private async Task StartCallFlowDslProcessor()
        {
            try
            {
                var jsonObject =
                    File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(),
                                     $"{ArkeCallFlowService<T>.GetConfigValue("appSettings:Application")}.json"));
                DslProcessor.Dsl = await Task.Factory
                    .StartNew(() => JsonConvert.DeserializeObject<CallFlowDsl>(
                        jsonObject).GetStepsFromDsl())
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Invalid CallFlow DSL");
            }
        }

        public void SetWorkflow(Workflow deviceWorkflow)
        {
            DslProcessor.Dsl = deviceWorkflow.Value as Dictionary<int, Step>;
        }

        public async Task ForceCallEndAsync()
        {
            if (!CallState.CallCleanupRun)
            {
                // hack - needs to be cleaned up later
                try
                {
                    var step = DslProcessor.Dsl.First(d => d.Value.NodeData.Category == "CleanupStep");
                    await DslProcessor.ProcessStepAsync(step.Key);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Error trying to call cleanup step.");
                }
            }

            _callState.CallCanBeAbandoned = true;
            await HangupAsync();
        }

        public async Task StartCallRecordingAsync()
        {
            if (_callState.IncomingSipChannel != null)
                await RecordingManager.StartRecordingOnLine(_callState.IncomingSipChannel.Id as string, "I",
                    CallState);
            if (_callState.OutgoingSipChannel != null)
                await RecordingManager.StartRecordingOnLine(_callState.OutgoingSipChannel.Id as string, "O",
                    CallState);
        }

        public async Task StartRecordingOnBridgeAsync(string bridgeId)
        {
            await RecordingManager.StartRecordingOnBridge(bridgeId, CallState);
        }

        public async Task StartRecordingOnLineAsync(string lineId, string direction)
        {
            await RecordingManager.StartRecordingOnLine(lineId, direction, CallState);
        }

        private async Task StartTheCallFlow()
        {
            await CallStateMachine.FireAsync(Trigger.StartCallFlow);
        }

        public async Task StopHoldingBridgeAsync()
        {
            await _arkeBridgeFactory.StopHoldingBridge(CallState).ConfigureAwait(false);
        }

        public async Task StopCallRecordingAsync()
        {
            if (_callState.IncomingSipChannel != null)
                await RecordingManager.StopRecordingOnLine(_callState.IncomingSipChannel.Id as string);
            if (_callState.OutgoingSipChannel != null)
                await RecordingManager.StopRecordingOnLine(_callState.OutgoingSipChannel.Id as string);
        }

        public async Task<ISipChannel> CreateTransferLine(object sipLine)
        {
            if (!(sipLine is Channel channel)) return null;
            
            var arkeChannel = new ArkeSipChannel { Channel = channel, CurrentState = State.Initialization };
            _callState.TransferSipChannel = arkeChannel;
            return arkeChannel;

        }

        public async Task<ISipChannel> CreateOutgoingLine(object sipLine)
        {
            if (!(sipLine is Channel channel)) return null;

            var arkeChannel = new ArkeSipChannel { Channel = channel, CurrentState = State.Initialization };
            _callState.OutgoingSipChannel = arkeChannel;
            return arkeChannel;
        }
    }
}