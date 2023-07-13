using Arke.SipEngine.Bridging;
using Arke.ARI.Models;

namespace Arke.IVR.Bridging
{
    public class ArkeBridge : IBridge
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public Bridge Bridge { get; set; }
        public string Name { get; set; }
    }
}