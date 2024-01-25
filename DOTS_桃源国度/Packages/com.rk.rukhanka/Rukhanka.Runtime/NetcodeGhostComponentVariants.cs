#if RUKHANKA_WITH_NETCODE

using Unity.Entities;
using Unity.NetCode;

/////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{ 

[GhostComponentVariation(typeof(AnimatorControllerLayerComponent), "Animator Controller Layer")]
[GhostComponent()]
public struct AnimatorControllerLayerVariant
{
	[GhostField(SendData = false)]
	public BlobAssetReference<ControllerBlob> controller;
	[GhostField()]
	public int layerIndex;
	[GhostField()]
	public RuntimeAnimatorData rtd;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////

[GhostComponentVariation(typeof(AnimatorControllerParameterComponent), "Animator Controller Parameter")]
[GhostComponent()]
public struct AnimatorControllerParameterVariant
{
	[GhostField()]
	public uint hash;
	[GhostField()]
	public ControllerParameterType type;
	[GhostField()]
	public ParameterValue value;
}

}

#endif // RUKHANKA_WITH_NETCODE