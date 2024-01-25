using System.Collections.Generic;
using Unity.Entities;
#if RUKHANKA_WITH_NETCODE
using Unity.NetCode;
#endif
using Unity.Rendering;
using UnityEngine;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Samples
{
public class PrefabSpawner: MonoBehaviour
{
	public GameObject[] prefabPool;
	public int count;
	public float spawnRadius;
	public float spawnDelayInSeconds;
	public bool spawnUntityObjects;
	public bool initialSpawn;

	void Start()
	{
		if (spawnUntityObjects)
			Spawn();
	}

	public void Spawn()
	{
		for  (int i = 0; i < prefabPool.Length; ++i)
		{
			var scc = new SpawnCommandComponent() { spawnCount = count / prefabPool.Length };
			var p = prefabPool[i];
			for (int l = 0; l < scc.spawnCount; ++l)
			{
				var g = GameObject.Instantiate(p);
				var randomPos = new Vector2(Random.value, Random.value) * 2 - new Vector2(1, 1);
				var position = new Vector3(randomPos.x, 0, randomPos.y) * spawnRadius;
				var rot = Quaternion.Euler(new Vector3(0, Random.value * 360, 9));
				g.transform.position += position;
				g.transform.rotation = rot;

				var a = g.GetComponent<Animator>();
				var randomSpeedVal = (Random.value * 2 - 1) * 0.5f + 1;
				a.SetFloat("Crowd_AnimationSpeed", randomSpeedVal);
			}
		}
	}
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public class PrefabSpawnerBaker: Baker<PrefabSpawner>
{ 
	public override void Bake(PrefabSpawner a)
	{
		for  (int i = 0; i < a.prefabPool.Length; ++i)
		{
			var p = a.prefabPool[i];

			var spc = new SpawnPrefabComponent()
			{
				prefabToSpawn = GetEntity(p, TransformUsageFlags.Dynamic),
				spawnRadius = a.spawnRadius,
			};

			var scc = new SpawnCommandComponent()
			{
				spawnCount = a.count / a.prefabPool.Length,
				spawnTime = Time.time + a.spawnDelayInSeconds,
				spawnerPos = a.transform.position
			};

			if (!a.spawnUntityObjects)
			{
				var e = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
				AddComponent(e, spc);
				if (a.initialSpawn)
					AddComponent(e, scc);

			#if RUKHANKA_WITH_NETCODE
				if (p.GetComponent<GhostAuthoringComponent>() != null)
				{
					var netObjTag = default(NetworkedPrefab);
					AddComponent(e, netObjTag);
				}
			#endif

			}
		}
	}
}
}
