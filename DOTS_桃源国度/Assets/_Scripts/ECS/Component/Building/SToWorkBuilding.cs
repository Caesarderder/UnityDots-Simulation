using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct SFarm : IComponentData
{
	public float CurGrowTime;
}
public struct SInstitute : IComponentData
{
	public int CurPeople;
	public float GrowUpTime;
}
[InternalBufferCapacity(16)]
public struct BufferBuildingWorkPoint: IBufferElementData
{
	public float2 pos;
	public bool NeedWorking;  
}
[InternalBufferCapacity(16)]
public struct BufferBuildingHasWorkedPoint : IBufferElementData
{
	public float2 pos;
}

public struct  SPointToWork
{
	public float2 pos;
	public Entity entity;
}
