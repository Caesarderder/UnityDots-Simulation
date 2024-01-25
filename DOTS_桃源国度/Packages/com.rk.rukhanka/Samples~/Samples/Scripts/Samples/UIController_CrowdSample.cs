using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Samples
{
[BurstCompile]
class UIController_CrowdSample: MonoBehaviour
{
	public TextMeshProUGUI spawnCountLabel;
	public TextMeshProUGUI counterLabel;
	public TextMeshProUGUI bonesCountLabel;
	public Slider spawnCountSlider;
	public Button spawnBtn;
	public Toggle visualizeSkeletonsToggle;

	EntityQuery spawnerQuery, animatedObjectsQuery, debugConfigSingletonQuery, rigsQuery;
	EntityManager em;

/////////////////////////////////////////////////////////////////////////////////

	void Start()
	{
		spawnBtn.onClick.AddListener(SpawnPrefabs);

		var worlds = World.All;
		foreach (var w in worlds)
		{
			if (RukhankaSystemsBootstrap.IsClientOrLocalSimulationWorld(w))
			{
				em = w.EntityManager;
				break;
			}
		}
		
		var ecb0 = new EntityQueryBuilder(Allocator.Temp)
		.WithAll<SpawnPrefabComponent>();
		spawnerQuery = em.CreateEntityQuery(ecb0);

		var ecb1 = new EntityQueryBuilder(Allocator.Temp)
		.WithAll<AnimatorControllerLayerComponent>();
		animatedObjectsQuery = em.CreateEntityQuery(ecb1);

		var ecb2 = new EntityQueryBuilder(Allocator.Temp)
		.WithAllRW<DebugConfigurationComponent>();
		debugConfigSingletonQuery = em.CreateEntityQuery(ecb2);

		var ecb3 = new EntityQueryBuilder(Allocator.Temp)
		.WithAllRW<RigDefinitionComponent>();
		rigsQuery = em.CreateEntityQuery(ecb3);

	#if !RUKHANKA_DEBUG_INFO
		visualizeSkeletonsToggle.enabled = false;
		visualizeSkeletonsToggle.isOn = false;
		var tmp = visualizeSkeletonsToggle.GetComponentInChildren<TextMeshProUGUI>();
		tmp.text += " (RUKHANKA_DEBUG_INFO is not defined)";
		tmp.color = Color.gray;
	#endif

	}

/////////////////////////////////////////////////////////////////////////////////

	void SpawnPrefabs()
	{
		var spawners = spawnerQuery.ToEntityArray(Allocator.Temp);

		foreach (var s in spawners)
		{
			var scc = new SpawnCommandComponent()
			{
				spawnCount = (int)(spawnCountSlider.value / spawners.Length)
			};

			em.AddComponentData(s, scc);
		}
	}

/////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	static int CalculateTotalBonesCount(ref EntityQuery eq)
	{
		var rv = 0;
		var rigs = eq.ToComponentDataArray<RigDefinitionComponent>(Allocator.Temp);
		foreach (var r in rigs)
		{
			rv += r.rigBlob.Value.bones.Length;
		}

		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////

	void Update()
	{
		spawnCountLabel.text = $"{spawnCountSlider.value}";
		var animatedObjectsCount = animatedObjectsQuery.CalculateEntityCount();
		counterLabel.text = $"Total Instance Count: {animatedObjectsCount}";
		var totalBonesCount = CalculateTotalBonesCount(ref rigsQuery);
		bonesCountLabel.text = $"Total Bone Count: {totalBonesCount}";

	#if RUKHANKA_DEBUG_INFO
		if (debugConfigSingletonQuery.TryGetSingletonRW<DebugConfigurationComponent>(out var dc))
		{
			dc.ValueRW.visualizeAllRigs = visualizeSkeletonsToggle.isOn;
		}
	#endif

	}
}
}

