using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/GBuilding")]
public class GBuildingSO : DescriptionBaseSO
{
	#region Home
	public GHomeData Home;
	#endregion

	#region Farmland
	public GPlant4Data Wheat;
	#endregion

	#region institute
	public GInstitueData Institute;
	#endregion

	#region Farmlan
	#endregion

	#region Farmlan
	#endregion

	#region Farmlan
	#endregion

	#region Farmlan
	#endregion

	#region Farmlan
	#endregion
}

[Serializable]
public struct GHomeData
{
	public float Stability;
	public float WoodCost;
	public float FoodCost;

	public int Capacity;
	public float RestTime;
}
[Serializable]
public struct GPlant4Data
{
	public float Stability;
	public float WoodCost;

	public int GrowStage;
	public float GrowTime0;
	public float GrowTime1;
	public float GrowTime2;
	public float GrowTime3;

	public float WorkingTime0;
	public float WorkingTime1;
	public float WorkingTime2;
	public float WorkingTime3;

	public float StaminaCost0;
	public float StaminaCost1;
	public float StaminaCost2;
	public float StaminaCost3;

	public float SourceGain;
}


[Serializable]
public struct GInstitueData
{
	public int Capacity;
	public float WorkingTime;
	public float SourceGain;
	public float DuationCost;
	public float MaxGrowUpTime;
}
