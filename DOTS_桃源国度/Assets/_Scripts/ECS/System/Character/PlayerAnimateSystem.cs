using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]
public partial struct PlayerAnimateSystem : ISystem
{
	public void OnUpdate(ref SystemState state)
	{
		var ecb = new EntityCommandBuffer(Allocator.Temp);

		foreach (var (playerGameObjectPrefab, entity) in
				 SystemAPI.Query<PlayerGameObjectPrefab>().WithNone<PlayerAnimatorReference>().WithEntityAccess())
		{
			var newCompanionGameObject = Object.Instantiate(playerGameObjectPrefab.Value);
			var newAnimatorReference = new PlayerAnimatorReference
			{
				Value = newCompanionGameObject.GetComponent<Animator>()
			};
			ecb.AddComponent(entity, newAnimatorReference);
		}

		ecb.Playback(state.EntityManager);
		ecb.Dispose();
	}
}
