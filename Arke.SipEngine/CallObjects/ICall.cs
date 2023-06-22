using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.IntegrationApi.CallObjects;
using Arke.SipEngine.Api;
using Arke.SipEngine.Bridging;
using Arke.SipEngine.Device;
using Arke.SipEngine.Events;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;
using Arke.SipEngine.Prompts;
using Serilog;

namespace Arke.SipEngine.CallObjects
{
    public interface ICall<T>
    {
        Guid CallId { get; set; }
        T CallState { get; set; }
        Dictionary<string,string> LogData { get; }
        ILogger Logger { get; }
        NodeProperties StepSettings { get; set; }
        IPromptPlayer PromptPlayer { get; }
        IPhoneInputHandler InputProcessor { get; }
        ILanguageSelectionPromptPlayer LanguageSelectionPromptPlayer { get; }
        IRecordingManager RecordingManager { get; }
        ISipApiClient SipApiClient { get; }
        ISipBridgingApi SipBridgingApi { get; }
        ISipLineApi SipLineApi { get; }
        string SipProviderId { get; set; }

        Task RunCallScriptAsync(CancellationToken cancellationToken);
        Task HangupAsync();
        Task ProcessCallLogicAsync();
        Task StartCallRecordingAsync();
        Task StopCallRecordingAsync();
        Task FireStateChange(Trigger trigger);
        void AddStepToIncomingProcessQueue(int stepNumber);
        void AddStepToOutgoingProcessQueue(int stepNumber);
        State GetCurrentState();
        event Action<ICall<T>, OnWorkflowStepEvent> OnWorkflowStep;
        void SetCallLanguage(LanguageData languageData);
        Task<IBridge> CreateBridgeAsync(BridgeType bridgeType);
        Task StopHoldingBridgeAsync();
        Task AddLineToBridgeAsync(string lineId, string bridgeId);
        Task StartRecordingOnLineAsync(string lineId, string direction);
        Task StartRecordingOnBridgeAsync(string bridgeId);
        void SetWorkflow(Workflow deviceWorkflow);
        Task ForceCallEndAsync();
        Task<ISipChannel> CreateTransferLine(object transferLine);
        Task<ISipChannel> CreateOutgoingLine(object outgoingLine);
    }
}
