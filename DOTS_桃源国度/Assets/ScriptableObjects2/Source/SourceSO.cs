using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Source")]
public class SourceSO : DescriptionBaseSO
{
	public float FoodCur;
	public float FoodMax;

	public float WoodCur;
	public float WoodMax;

	public float KnowledgeCur;

	public int PeopleCur;
	public int PeopleMax;

	public int ScientistCur=0;
	public int ScientistMax=0;
}
