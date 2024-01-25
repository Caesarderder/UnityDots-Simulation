using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct SHome : IComponentData
{
	
}
[InternalBufferCapacity(8)]
public struct BufferSHomeLiver:IBufferElementData
{
	public Entity entity;
}

public struct SFarmerHome:IComponentData
{

}
public struct SScientistHome : IComponentData
{

}
