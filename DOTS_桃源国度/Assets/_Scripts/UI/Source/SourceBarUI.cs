using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;

public class SourceBarUI : Singleton<SourceBarUI>
{
	public GSource source;

	public TMP_Text PeopleText;
	public TMP_Text FoodText;
	public TMP_Text WoodText;
	public TMP_Text KnoledgeText;
	public TMP_Text DateText;

	public void Start()
	{
		World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SourceSystem>().CannalSource+=SourceLinstener;
	
	}

	public void SourceLinstener(GSource source)
	{
		this.source = source;
		PeopleText.text = source.PeopleCur + "/" + source.PeopleMax;
		FoodText.text = source.FoodCur + "/" + source.FoodMax;
		WoodText.text = source.WoodCur + "/" + source.WoodMax;
		KnoledgeText.text = source.KnowledgeCur + "";

		DateText.text = source.Year + "Äê" + source.Month + "ÔÂ" + source.Week + "ÖÜ";

	}

	

	private void Update()
	{

	}

}
