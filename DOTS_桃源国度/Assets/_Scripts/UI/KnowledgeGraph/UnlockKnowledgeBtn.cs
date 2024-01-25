using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

public class UnlockKnowledgeBtn : MonoBehaviour
{
	public TMP_Text Cost;
	public TMP_Text Description;
	public ScientistDataSO Scientist;
	public KnowledgeGraphUI KnowledgeGraphUI;
	public int Index;



	public void OnClick()
	{
		
		if(KnowledgeGraphUI.UnlockedKnoledge(Index))
			UnlockedKnoledge();
	}
	public void UnlockedKnoledge()
	{
		
		transform.GetChild(0).gameObject.SetActive(false);
		transform.GetChild(1).gameObject.SetActive(false);
	}
	public void Init(int index,string Description,float Cost,ScientistDataSO curScientist,KnowledgeGraphUI ui)
	{
		this.Cost.text = "-" + Cost;
		this.Description.text = Description;
		KnowledgeGraphUI = ui;
		Scientist = curScientist;
		Index = index;
	}
}
