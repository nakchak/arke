using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arke.IntegrationApi.CallObjects;
using Arke.SipEngine.Bridging;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.Device;

namespace Arke.SampleProject
{
    public class SampleCallState : ICallInfo
    {
        public Guid CallId { get; }
        public bool CallStarted { get; set; }
        public string Destination { get; set; }
        public DeviceConfig Device { get; set; }
        public string Endpoint { get; set; }
        public string FileName { get; set; }
        public string InputData { get; set; }
        public string LanguageCode { get; set; }
        public string PortId { get; set; }
        public bool ProcessOutgoingQueue { get; set; }
        public string TerminationCode { get; set; }
        public DateTimeOffset TimeOffHook { get; set; }
        public int InputRetryCount { get; set; }
        public bool CallCanBeAbandoned { get; set; }
        public int AttemptCount { get; set; }
        public Queue<string> OutboundEndpoint { get; set; }
        public DateTimeOffset? TalkTimeStart { get; set; }
        public DateTimeOffset? TalkTimeEnd { get; set; }
        public DateTimeOffset? CalledPartyAnswerTime { get; set; }
        public DateTimeOffset? TrunkOffHookTime { get; set; }
        public DateTimeOffset? CalledPartyAcceptTime { get; set; }
        public DateTimeOffset? TimeDeviceConnected { get; set; }
        public string OutboundUri { get; set; }
        public IBridge Bridge { get; set; }
        public string HoldPrompt { get; set; }
        public string OutboundCallerId { get; set; }
        public bool CallCleanupRun { get; set; }
        public DateTimeOffset OutgoingRecordingStartTime { get; set; }
        public DateTimeOffset IncomingRecordingStartTime { get; set; }
        public DateTimeOffset OutgoingRecordingEndTime { get; set; }
        public DateTimeOffset IncomingRecordingEndTime { get; set; }
        public long ChannelIncomingRecordingStartTimeTicks { get; set; }
        public long ChannelIncomingRecordingEndTimeTicks { get; set; }
        public long ChannelOutgoingRecordingStartTimeTicks { get; set; }
        public long ChannelOutgoingRecordingEndTimeTicks { get; set; }
        public long ChannelBridgeRecordingStartTimeTicks { get; set; }
        public long ChannelBridgeRecordingEndTimeTicks { get; set; }
        public Queue<int> IncomingLineQueue { get; set; }
        public Queue<int> OutgoingLineQueue { get; set; }
        public ISipChannel IncomingSipChannel { get; set; }
        public ISipChannel MonitoringSipChannel { get; set; }
        public ISipChannel OutgoingSipChannel { get; set; }
        public ISipChannel TransferSipChannel { get; set; }
        public int StepAttempts { get; set; }
    }
}
