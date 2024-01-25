#if RUKHANKA_WITH_NETCODE

using Unity.NetCode;

/////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhaka.Samples
{

[UnityEngine.Scripting.Preserve]
public class GameBootstrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        AutoConnectPort = 7979; // Enabled auto connect
        return base.Initialize(defaultWorldName); // Use the regular bootstrap
    }
}

}

#endif