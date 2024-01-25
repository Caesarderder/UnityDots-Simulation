using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingButtonUI : MonoBehaviour
{

	public int ID;

	public void OnButtonClick()
	{
		PlacementSystem.Instance.StartPlacement(ID);
	}

}
