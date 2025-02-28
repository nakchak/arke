﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arke.IntegrationApi.CallObjects;
using Arke.IVR.CallObjects;
using Arke.SipEngine;
using Arke.SipEngine.Api;
using Arke.SipEngine.CallObjects;
using Arke.ARI;
using Arke.ARI.Models;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Arke.IVR
{
    public class ArkeCallFlowService<T> : ICallFlowService<T> where T : ICallInfo
    {
        private readonly IAriClient _ariClient;
        private readonly ILogger _logger;
        private CancellationToken _cancellationToken;
        private readonly ISipApiClient _sipApi;

        public Dictionary<string, ICall<T>> ConnectedLines { get; }
        public static IConfiguration Configuration { get; set; }

        public ArkeCallFlowService(ILogger logger, IAriClient ariClient, ISipApiClient sipApi)
        {
            _logger = logger;
            _logger.Information("ArkeCallFlowService Created");
            ConnectedLines = new Dictionary<string, ICall<T>>();
            _ariClient = ariClient;
            _sipApi = sipApi;
        }
        
        public static string GetConfigValue(string key)
        {
            return Configuration?[key];
        }

        public bool Start(CancellationToken token)
        {
            _cancellationToken = token;
            _cancellationToken.Register(OnTokenCancellation);
            try
            {
                _logger.Information("ArkeCallFlowService Start() Initiated");
                SetupAriEvents();
                ConnectAri();
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error Setting up CallFlowServices.");
                return false;
            }
        }

        private async void OnTokenCancellation()
        {
            await EndAllCallsAsync();
        }

        public virtual void ConnectAri()
        {
            ((AriClient)_ariClient).Connect();
        }
        
        public virtual void SetupAriEvents()
        {
            _ariClient.OnStasisStartEvent += AriClientOnStasisStartEvent;
            _ariClient.OnStasisEndEvent += AriClientOnStasisEndEvent;
            _logger.Information("Registering Events");
            _sipApi.SubscribeToDtmfReceivedEvents();
            _sipApi.SubscribeToLineHangupEvents();
            _sipApi.SubscribeToPlaybackFinishedEvents();
        }
        
        public bool Stop()
        {
            _logger.Information("Unregistering events...");
            var disconnectionTask = DisconnectAri();
            disconnectionTask.Wait(_cancellationToken);
            _logger.Information("Shutdown complete.");
            return true;
        }

        public virtual async Task DisconnectAri()
        {
            _sipApi.UnsubscribeToDtmfReceivedEvents();
            _sipApi.UnsubscribeToPlaybackFinishedEvents();
            if (!((AriClient) _ariClient).Connected)
                return;
            await EndAllCallsAsync();
            ((AriClient) _ariClient).Disconnect();
            _sipApi.UnsubscribeToDtmfReceivedEvents();
            _sipApi.UnsubscribeToPlaybackFinishedEvents();
        }

        public bool Continue()
        {
            ConnectAri();
            return true;
        }

        public bool Pause()
        {
            var disconnectTask = DisconnectAri();
            disconnectTask.Wait(_cancellationToken);
            return true;
        }

        
        public virtual async void AriClientOnStasisStartEvent(IAriClient sender, StasisStartEvent e)
        {
            _logger.Debug($"Line Connecting: {e.Channel.Name}");
            
            if (e.Args.Contains("dialed") || e.Args.Contains("SnoopChannel"))
                return;
            _logger.Information("Line Offhook", new
            {
                ChannelId = e.Channel.Id,
                CallerIdName = e.Channel.Caller.Number,
                CallerIdNumber = e.Channel.Caller.Name
            });
            var line = ArkeCallFactory.CreateArkeCall(e.Channel);
            ConnectedLines.Add(e.Channel.Id, line as ICall<T>);
            _logger.Information("Starting Call Script", new
            {
                ChannelId = e.Channel.Id
            });
        
            // call answered and started
#pragma warning disable CS4014 // We don't want to wait for this thread to complete, we need to keep processing further calls
            Task.Run(async () => await line.RunCallScriptAsync(_cancellationToken), _cancellationToken);
#pragma warning restore CS4014
            _logger.Information("Call Setup Script Complete", new { ChannelId = e.Channel.Id });
        }

        public virtual async void AriClientOnStasisEndEvent(IAriClient sender, StasisEndEvent stasisEndEvent)
        {
            _logger.Information("Channel {channelId} hungup", new { channelId = stasisEndEvent.Channel.Id});
            if (!ConnectedLines.ContainsKey(stasisEndEvent.Channel.Id))
                return;

            try
            {
                await ConnectedLines[stasisEndEvent.Channel.Id].HangupAsync();
            }
            catch (AriException e)
            {
                _logger.Warning($"Exception while hanging up, most likely ok: {e.Message}");
            }
            
            while (!ConnectedLines[stasisEndEvent.Channel.Id].CallState.CallCanBeAbandoned)
            {
                await Task.Delay(1000, _cancellationToken);
            }
            ConnectedLines.Remove(stasisEndEvent.Channel.Id);
        }

        protected virtual async Task EndAllCallsAsync()
        {
            foreach (var line in ConnectedLines.Where(c => c.Value.CallState.CallCanBeAbandoned))
            {
                await line.Value.HangupAsync();
            }
            foreach (var line in ConnectedLines.Where(c => !c.Value.CallState.CallCanBeAbandoned))
            {
                while (!line.Value.CallState.CallCanBeAbandoned)
                    await Task.Delay(1000, _cancellationToken);
                await line.Value.HangupAsync();
            }
        }
    }
}