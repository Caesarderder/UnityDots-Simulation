using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
#if RUKHANKA_WITH_NETCODE
using Unity.NetCode;
#endif
using UnityEngine;
using UnityEngine.UI;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Samples
{
class UIController_NetcodeDemo: MonoBehaviour
{
	public TextMeshProUGUI spawnCountLabel;
	public TextMeshProUGUI localObjectsCountLabel;
	public TextMeshProUGUI predictedGhostCountLabel;
	public TextMeshProUGUI interpolatedGhostCountLabel;
	public TextMeshProUGUI descriptionLabel;
	public Slider spawnCountSlider;
	public Button spawnNetworkedBtn;
	public Button spawnLocalBtn;

	EntityQuery spawnerQuery, connectionQuery, localObjectsQuery, predictedGhostObjectsQuery, interpolatedGhostObjectsQuery;
	EntityManager em;

/////////////////////////////////////////////////////////////////////////////////

	void Start()
	{
	#if !RUKHANKA_WITH_NETCODE
		descriptionLabel.text += $"\n\n<color=red>This sample is intended to work with 'Netcode for Entites' package and RUKHANKA_WITH_NETCODE script symbol defined! </color>";
	#endif

		spawnNetworkedBtn.onClick.AddListener(SpawnNetworkedPrefabs);
		spawnLocalBtn.onClick.AddListener(SpawnLocalPrefabs);

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
		.WithAll<SpawnPrefabComponent>()
		.WithNone<NetworkedPrefab>();
		spawnerQuery = em.CreateEntityQuery(ecb0);

		var ecb1 = new EntityQueryBuilder(Allocator.Temp)
	#if RUKHANKA_WITH_NETCODE
		.WithAll<NetworkId>()
	#endif
		;
		connectionQuery = em.CreateEntityQuery(ecb1);

		var ecb2 = new EntityQueryBuilder(Allocator.Temp)
		.WithAll<AnimatorControllerLayerComponent>()
	#if RUKHANKA_WITH_NETCODE
		.WithNone<GhostInstance>()
	#endif
		;
		localObjectsQuery = em.CreateEntityQuery(ecb2);

		var ecb3 = new EntityQueryBuilder(Allocator.Temp)
		.WithAll<AnimatorControllerLayerComponent>()
	#if RUKHANKA_WITH_NETCODE
		.WithAll<PredictedGhost>()
	#endif
		;
		predictedGhostObjectsQuery = em.CreateEntityQuery(ecb3);

		var ecb4 = new EntityQueryBuilder(Allocator.Temp)
		.WithAll<AnimatorControllerLayerComponent>()
	#if RUKHANKA_WITH_NETCODE
		.WithNone<PredictedGhost>()
		.WithAll<GhostInstance>()
	#endif
		;
		interpolatedGhostObjectsQuery = em.CreateEntityQuery(ecb4);

	#if !RUKHANKA_WITH_NETCODE
		predictedGhostCountLabel.enabled = false;
		interpolatedGhostCountLabel.enabled = false;
	#endif
	}

/////////////////////////////////////////////////////////////////////////////////

	void SpawnLocalPrefabs()
	{
		var spawners = spawnerQuery.ToEntityArray(Allocator.Temp);

		var alreadySpawned = 0;
		var spawnCount = (int)math.max(1, spawnCountSlider.value / spawners.Length);
		for (var i = 0; i < spawners.Length && alreadySpawned < spawnCountSlider.value; ++i)
		{
			var scc = new SpawnCommandComponent()
			{
				spawnCount = i == spawners.Length - 1 ? (int)spawnCountSlider.value - alreadySpawned : spawnCount
			};
			alreadySpawned += scc.spawnCount;

			em.AddComponentData(spawners[i], scc);
		}
	}

/////////////////////////////////////////////////////////////////////////////////

	void SpawnNetworkedPrefabs()
	{
	#if RUKHANKA_WITH_NETCODE
		var connection = connectionQuery.ToEntityArray(Allocator.Temp);

		if (!connection.IsCreated || connection.Length == 0)
		{
			Debug.LogError($"Cannot send spawn command! No server connection!");
			return;
		}

		var scc = new ServerSpawnPrefabCommand()
		{
			spawnerPos = float3.zero,
			spawnCount = (int)spawnCountSlider.value
		};

		var e = em.CreateEntity();
		em.AddComponentData(e, scc);
		var rpc = new SendRpcCommandRequest() { TargetConnection = connection[0]};
		em.AddComponentData(e, rpc);
	#endif
	}

/////////////////////////////////////////////////////////////////////////////////

	void Update()
	{
		spawnCountLabel.text = $"{spawnCountSlider.value}";
		var localObjectsCount = localObjectsQuery.CalculateEntityCount();
		localObjectsCountLabel.text = $"Local objects count: {localObjectsCount}";
	#if RUKHANKA_WITH_NETCODE
		var predictedGhostCount = predictedGhostObjectsQuery.CalculateEntityCount();
		predictedGhostCountLabel.text = $"Predicted ghosts count: {predictedGhostCount}";
		var interpolatedGhostsCount = interpolatedGhostObjectsQuery.CalculateEntityCount();
		interpolatedGhostCountLabel.text = $"Interpolated ghosts count: {interpolatedGhostsCount}";
	#endif
	}
}
}

