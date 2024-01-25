using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;

public class ScientistGetSlotUI : MonoBehaviour
{
	public TMP_Text Name;
	public TMP_Text Description;
	public TMP_Text Cost;

	public ScientistDataSO scientistData;
	public GameObject BookGo;
	public Transform BookParent;


	public void Start()
	{
		Name.text = scientistData.Name;
		Description.text = scientistData.Description;
		Cost.text ="-"+ scientistData.KnoledgeCost+"";

		//生成书籍资料
		foreach (var book in scientistData.Books)
		{
			var go=Instantiate(BookGo,BookParent);
			var text= go.GetComponent<TMP_Text>() ;
			text.text = book.Name + ":"+'\n';
			text.text = book.Description + ":" + '\n';
			foreach (var knoledge in book.BookKnoledges)
			{
				text.text += knoledge.Description + '\n';
			}
		}
	}

	public void CheckIfCanGetScientist()
	{
		if(SourceBarUI.Instance.source.KnowledgeCur>=scientistData.KnoledgeCost)
		{
			scientistData.IsUnlocked = true;
			World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SourceSystem>().missions.Add(new SourceSystem.SMission {
				index=6,
				value= -scientistData.KnoledgeCost,
			});
			World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SourceSystem>().missions.Add(new SourceSystem.SMission
			{
				index = 8,
				value = 1,
			});
			Destroy(gameObject);
		}
	}
}
