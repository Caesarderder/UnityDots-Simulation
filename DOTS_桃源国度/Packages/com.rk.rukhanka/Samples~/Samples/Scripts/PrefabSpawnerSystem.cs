using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;
using Random = Unity.Mathematics.Random;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Samples
{
public struct SpawnPrefabComponent: IComponentData
{
	public Entity prefabToSpawn;
	public float spawnRadius;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct SpawnCommandComponent: IComponentData
{
	public int spawnCount;
	public float spawnTime;
	public float3 spawnerPos;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct NetworkedPrefab: IComponentData {}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation | WorldSystemFilterFlags.ClientSimulation)]
public partial struct PrefabSpawnerSystem: ISystem
{
	uint updateCounter;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	public void OnUpdate(ref SystemState ss)
	{
		var ecbs = GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
		var ecb = ecbs.CreateCommandBuffer(ss.WorldUnmanaged);
		FastAnimatorParameter animSpeed = new FastAnimatorParameter("Crowd_AnimationSpeed");

		var t  = SystemAPI.Time.ElapsedTime;

		foreach (var (p0, _, sc, e) in Query<RefRW<SpawnPrefabComponent>, RefRO<LocalTransform>, RefRO<SpawnCommandComponent>>()
		.WithNone<NetworkedPrefab>()
		.WithEntityAccess())
		{
			var spc = p0.ValueRO;
			var scc = sc.ValueRO;
			if (scc.spawnTime > t)
				continue;

			var instances = ss.EntityManager.Instantiate(spc.prefabToSpawn, sc.ValueRO.spawnCount, Allocator.Temp);
			var random = Random.CreateFromIndex(updateCounter++);

			foreach (var entity in instances)
			{
				var randomPos = random.NextFloat2() * 2 - 1;
				var position = new float3(randomPos.x, 0, randomPos.y) * spc.spawnRadius;
				var rot = quaternion.RotateY(random.NextFloat() * math.PI * 2);
				var transform = ss.EntityManager.GetComponentData<LocalTransform>(entity);

				transform.Position += position + scc.spawnerPos;
				transform.Rotation = rot;
				ss.EntityManager.SetComponentData(entity, transform);

				if (HasComponent<AnimatorControllerParameterIndexTableComponent>(entity))
				{
					var apa = SystemAPI.GetAspect<AnimatorParametersAspect>(entity);
					var randomSpeedVal = (random.NextFloat() * 2 - 1) * 0.5f + 1;
					apa.SetParameterValue(animSpeed, randomSpeedVal);
				}
				ecb.RemoveComponent<SpawnCommandComponent>(e);
			}

		}
	}
}
}
