using System.Threading.Tasks;
using Arke.SipEngine.Api;
using Arke.SipEngine.BridgeName;
using Arke.SipEngine.Bridging;
using Arke.SipEngine.CallObjects;

namespace Arke.IVR.Bridging
{
    public class ArkeBridgeFactory
    {
        private readonly ISipBridgingApi _ariClient;
        public ArkeBridgeFactory(ISipBridgingApi ariClient)
        {
            _ariClient = ariClient;
        }

        public async Task AddLineToBridge(string lineId, string bridgeId)
        {
            await _ariClient.AddLineToBridgeAsync(lineId, bridgeId);
        }

        public async Task StopHoldingBridge(ICallInfo callInfo)
        {
            await _ariClient.RemoveLineFromBridgeAsync(callInfo.IncomingSipChannel.Id as string, callInfo.Bridge.Id);
            await _ariClient.DestroyBridgeAsync(callInfo.Bridge.Id);
        }

        public async Task<IBridge> CreateBridge(BridgeType bridgeType)
        {
            return bridgeType switch
            {
                BridgeType.NoMedia => await _ariClient.CreateBridgeAsync((new MixingBridgeType()).Type,
                    new BridgeNameGenerator().GetRandomBridgeName()),
                BridgeType.Holding => await _ariClient.CreateBridgeAsync((new HoldingBridgeType()).Type,
                    new BridgeNameGenerator().GetRandomBridgeName()),
                BridgeType.NoDTMF => await _ariClient.CreateBridgeAsync((new ProxyMediaBridgeType()).Type,
                    new BridgeNameGenerator().GetRandomBridgeName()),
                BridgeType.WithDTMF => await _ariClient.CreateBridgeAsync(
                    $"{(new DtmfBridgeType()).Type},{(new MixingBridgeType()).Type}",
                    new BridgeNameGenerator().GetRandomBridgeName()),
                _ => await _ariClient.CreateBridgeAsync((new DtmfBridgeType()).Type,
                    new BridgeNameGenerator().GetRandomBridgeName())
            };
        }
    }
}
