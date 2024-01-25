using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Gbuilding : IComponentData
{
	public int2 MapSize;

	#region Home
	public GHomeData Home;
	#endregion

	#region Farmland
	//Wheat
	public GPlant4Data Wheat;
	#endregion



	#region Institute
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
