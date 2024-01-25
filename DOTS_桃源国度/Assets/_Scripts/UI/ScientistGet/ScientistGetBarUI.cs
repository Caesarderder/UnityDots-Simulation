using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScientistGetBarUI : MonoBehaviour
{
	public GameObject ScientSlot;

	public ScientistListDataSO ScientistListData;

	[ContextMenu("Init")]
	public void Init()
	{
		foreach (var scientist in ScientistListData.Scientists)
		{
			scientist.IsUnlocked = false;
			foreach (var item in scientist.Books[0].BookKnoledges)
			{
				item.IsUnlocked = false;
			}
		}
	}

	public void OnEnable()
	{
		foreach (var scientist in ScientistListData.Scientists)
		{
			if(scientist.IsUnlocked)
			{ continue; }	
			var go=Instantiate(ScientSlot, transform);
			go.GetComponent<ScientistGetSlotUI>().scientistData = scientist;
		}
		
	}
	public void OnDisable()
	{
		for (int i = 0; i <transform.childCount;i++)
		{
			Destroy(transform.GetChild(i).gameObject);
		}
	}
}
