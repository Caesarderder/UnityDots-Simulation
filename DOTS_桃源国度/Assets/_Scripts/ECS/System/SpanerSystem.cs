using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using System.Diagnostics;
using Unity.Collections;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(StateUpdateSystem))]
public partial struct SpawnerSystem : ISystem
{

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{

	}

}
