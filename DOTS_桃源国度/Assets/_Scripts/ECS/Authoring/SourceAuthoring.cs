using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SourceAuthoring : MonoBehaviour
{
	public SourceSO Source;

	private SourceBarUI sourceBarUI;

	private class Baker : Baker<SourceAuthoring>
	{
		public override void Bake(SourceAuthoring authoring)
		{
			var data = authoring.Source;

			GSource source = new GSource
			{
				FoodCur = data.FoodCur,
				FoodMax = data.FoodMax,

				WoodCur = data.WoodCur,
				WoodMax = data.WoodMax,

				KnowledgeCur = data.KnowledgeCur,


				PeopleCur = data.PeopleCur,
				PeopleMax = data.PeopleMax,

				ScientistCur= data.ScientistCur,
				//ToDo:
				ScientistMax=1,
	};
			var Entity = GetEntity(TransformUsageFlags.None);
			AddComponent(Entity, source);
			AddBuffer<BufferGSourceChange>(Entity);
		}
	}
}
