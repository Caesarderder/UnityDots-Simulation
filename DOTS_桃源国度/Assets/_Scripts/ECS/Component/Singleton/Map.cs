using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(8)]
public struct BufferPathInfo : IBufferElementData
{
	public float3 Postion;
	public int ID;
	public int Cost;
}
