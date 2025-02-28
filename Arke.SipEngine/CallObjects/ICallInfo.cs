﻿using System;
using System.Collections.Generic;
using Arke.IntegrationApi.CallObjects;
using Arke.SipEngine.Bridging;
using Arke.SipEngine.Device;

namespace Arke.SipEngine.CallObjects
{
    public interface ICallInfo
    {
        Guid CallId { get; }
        bool CallStarted { get; set; }
        string Destination { get; set; }
        DeviceConfig Device { get; set; }
        string Endpoint { get; set; }
        string FileName { get; set; }
        string InputData { get; set; }
        string LanguageCode { get; set; }
        string PortId { get; set; }
        bool ProcessOutgoingQueue { get; set; }
        string TerminationCode { get; set; }
        DateTimeOffset TimeOffHook { get; set; }
        int InputRetryCount { get; set; }
        bool CallCanBeAbandoned { get; set; }
        int AttemptCount { get; set; }
        Queue<string> OutboundEndpoint { get; set; }
        DateTimeOffset? TalkTimeStart { get; set; }
        DateTimeOffset? TalkTimeEnd { get; set; }
        DateTimeOffset? CalledPartyAnswerTime { get; set; }
        DateTimeOffset? TrunkOffHookTime { get; set; }
        DateTimeOffset? CalledPartyAcceptTime { get; set; }
        DateTimeOffset? TimeDeviceConnected { get; set; }
        string OutboundUri { get; set; }
        IBridge Bridge { get; set; }
        string HoldPrompt { get; set; }
        string OutboundCallerId { get; set; }
        bool CallCleanupRun { get; set; }
        DateTimeOffset OutgoingRecordingStartTime { get; set; }
        DateTimeOffset IncomingRecordingStartTime { get; set; }
        DateTimeOffset OutgoingRecordingEndTime { get; set; }
        DateTimeOffset IncomingRecordingEndTime { get; set; }
        long ChannelIncomingRecordingStartTimeTicks { get; set; }
        long ChannelIncomingRecordingEndTimeTicks { get; set; }
        long ChannelOutgoingRecordingStartTimeTicks { get; set; }
        long ChannelOutgoingRecordingEndTimeTicks { get; set; }
        long ChannelBridgeRecordingStartTimeTicks { get; set; }
        long ChannelBridgeRecordingEndTimeTicks { get; set; }
        Queue<int> IncomingLineQueue { get; set; }
        Queue<int> OutgoingLineQueue { get; set; }

        ISipChannel IncomingSipChannel { get; set; }
        ISipChannel MonitoringSipChannel { get; set; }
        ISipChannel OutgoingSipChannel { get; set; }
        ISipChannel TransferSipChannel { get; set; }
        int StepAttempts { get; set; }
    }
}
