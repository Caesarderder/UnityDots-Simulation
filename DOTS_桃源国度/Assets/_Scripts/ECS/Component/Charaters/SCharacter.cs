using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct SCharacter : IComponentData
{
	public Entity Home;
	public Entity MoveTarget;
	public float2 HomeGirdPos;
	public float2 TargetPos;


	public float MaxDuration;
	public float CurDuration;
	public float MoveSpeed;
	public Unity.Mathematics.Random Random;
}

public struct BufferPath:IBufferElementData
{
	public float2 Position;
}
