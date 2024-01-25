#if RUKHANKA_WITH_NETCODE

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;
using Random = Unity.Mathematics.Random;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Samples
{

public struct ServerSpawnPrefabCommand: IRpcCommand
{
	public int spawnCount;
	public float3 spawnerPos;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ServerSpawnerSystem: ISystem
{
	uint updateCounter;
	BufferLookup<AnimatorControllerParameterComponent> paramBufLookup;
	ComponentLookup<NetworkId> networkIdLookup;

	EntityQuery prefabSpawners;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	public void OnCreate(ref SystemState ss)
	{
		paramBufLookup = ss.GetBufferLookup<AnimatorControllerParameterComponent>();
		networkIdLookup = ss.GetComponentLookup<NetworkId>();

		var eqb0 = new EntityQueryBuilder(Allocator.Temp)
		.WithAll<SpawnPrefabComponent, NetworkedPrefab>();
		prefabSpawners = ss.GetEntityQuery(eqb0);
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState ss)
	{
		var ecbs = GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
		var ecb = ecbs.CreateCommandBuffer(ss.WorldUnmanaged);
		FastAnimatorParameter animSpeed = new FastAnimatorParameter("Crowd_AnimationSpeed");

		paramBufLookup.Update(ref ss);
		networkIdLookup.Update(ref ss);

		var spawners = prefabSpawners.ToComponentDataArray<SpawnPrefabComponent>(Allocator.Temp);

		foreach (var (c0, c1, e) in Query<RefRO<ServerSpawnPrefabCommand>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
		{
			var spc = c0.ValueRO;
			var rpc = c1.ValueRO;
			var random = Random.CreateFromIndex(updateCounter++);

			for (var i = 0; i < spc.spawnCount; ++i)
			{
				var spawner = spawners[(int)(random.NextUInt() % spawners.Length)];
				var entity = ss.EntityManager.Instantiate(spawner.prefabToSpawn);

				var randomPos = random.NextFloat2() * 2 - 1;
				var position = new float3(randomPos.x, 0, randomPos.y) * spawner.spawnRadius;
				var rot = quaternion.RotateY(random.NextFloat() * math.PI * 2);
				var transform = ss.EntityManager.GetComponentData<LocalTransform>(entity);

				transform.Position += position + spc.spawnerPos;
				transform.Rotation = rot;
				ecb.SetComponent(entity, transform);

				var netId = networkIdLookup[rpc.SourceConnection];
				var go = new GhostOwner() { NetworkId = netId.Value };
				ecb.AddComponent(entity, go);
				ecb.AppendToBuffer(rpc.SourceConnection, new LinkedEntityGroup { Value = entity });

				if (HasComponent<AnimatorControllerParameterIndexTableComponent>(entity))
				{
					var acpit = GetComponent<AnimatorControllerParameterIndexTableComponent>(entity);
					paramBufLookup.TryGetBuffer(entity, out var acpc);
					var randomSpeedVal = (random.NextFloat() * 2 - 1) * 0.5f + 1;
					animSpeed.SetRuntimeParameterData(acpit.seedTable, acpc, new ParameterValue() { floatValue = randomSpeedVal } );
				}
			}
			ecb.DestroyEntity(e);
		}
	}
}
}

#endif
