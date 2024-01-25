using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


	public class PlayerGameObjectAuthoring : MonoBehaviour
	{
		public GameObject PlayerGameObjectPrefab;

		public class PlayerGameObjectPrefabBaker : Baker<PlayerGameObjectAuthoring>
		{
			public override void Bake(PlayerGameObjectAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.Dynamic);
				AddComponentObject(entity, new PlayerGameObjectPrefab { Value = authoring.PlayerGameObjectPrefab });
			}
		}
	}
	public class PlayerGameObjectPrefab : IComponentData
	{
		public GameObject Value;
	}

	public class PlayerAnimatorReference : ICleanupComponentData
	{
		public Animator Value;
	}


