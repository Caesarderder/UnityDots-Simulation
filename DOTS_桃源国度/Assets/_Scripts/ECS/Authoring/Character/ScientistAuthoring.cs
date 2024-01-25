using System.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class ScientistAuthoring : MonoBehaviour
{
	public  CharacterGlobalDataSO data;

	private class Baker : Baker<ScientistAuthoring>
	{
		public override void Bake(ScientistAuthoring authoring)
		{
			var data = authoring.data;
			var entity = GetEntity(TransformUsageFlags.Dynamic);

			var sCharacter = new SCharacter
			{
				MaxDuration = data.MaxDuration,
				CurDuration = 0f,
				MoveSpeed = data.MoveSpeed,
				Random = Unity.Mathematics.Random.CreateFromIndex((uint)UnityEngine.Random.Range(0,10000)),
			};

			AddComponent(entity, sCharacter);

			AddBuffer<BufferPath>(entity);

			AddComponent(entity, new SScientist { });

			AddComponent(entity, new RestState { });
			SetComponentEnabled<RestState>(entity, true);

			AddComponent(entity, new DurationCheckState { });
			SetComponentEnabled<DurationCheckState>(entity, false);
			AddComponent(entity, new GoToWorkState { });
			SetComponentEnabled<GoToWorkState>(entity, false);
			AddComponent(entity, new WorkState { });
			SetComponentEnabled<WorkState>(entity, false);
			AddComponent(entity, new GoToRestState { });
			SetComponentEnabled<GoToRestState>(entity, false);
		}
	}
}

