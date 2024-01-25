using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;

public class KnowledgeGraphUI : MonoBehaviour
{
	public SourceSO sourceSO;
	public GBuildingSO buildingSO;
	public CharacterGlobalDataSO characterSO;


	public GameObject ScientHeadSlot;

	public ScientistListDataSO ScientistListData;

	public Transform HeadBarTransform;


	//具体人物信息
	public TMP_Text Name;
	public TMP_Text Description;
	public TMP_Text BookDescription;

	public GameObject KnowledgeGo;
	public Transform KnowledgeParent;

	private ScientistDataSO curScientist;

	private void OnEnable()
	{
		int flag = 0;
		foreach (var scientist in ScientistListData.Scientists)
		{
			if (scientist.IsUnlocked)
			{
				if (flag == 0)
				{
					flag = 1;
					curScientist = scientist;
				
				}
		

				var go = Instantiate(ScientHeadSlot, HeadBarTransform);
				go.transform.GetChild(0).GetComponent<TMP_Text>().text = scientist.Name;
				go.GetComponent<ScientistHeadBtn>().Scientist = scientist;
				go.GetComponent<ScientistHeadBtn>().KnowledgeGraphUI = this;
			}
		}
		if(flag==1)
		{
			ChangeDetailedText();
		}
		

	}

	private void OnDisable()
	{

		for (int i = 0; i < HeadBarTransform.childCount; i++)
		{
			Destroy(HeadBarTransform.GetChild(i).gameObject);
		}
	}

	public void ChangeCurScientist(ScientistDataSO scientist)
	{
		curScientist = scientist;
		ChangeDetailedText();
	}

	public void ChangeDetailedText()
	{
		Name.text = curScientist.Name;
		Description.text = curScientist.Description;
		BookDescription.text = curScientist.Books[0].Description;

		for (int j = 0; j < KnowledgeParent.childCount; j++)
		{
			Destroy(KnowledgeParent.GetChild(j).gameObject);
		}
		int i = 0;
		foreach (var BookKnoledge in curScientist.Books[0].BookKnoledges)
		{

			var go=Instantiate(KnowledgeGo, KnowledgeParent);
			var btn = go.GetComponent<UnlockKnowledgeBtn>();
			btn.Init(i++, BookKnoledge.Description, BookKnoledge.Cost, curScientist, this);

			if (BookKnoledge.IsUnlocked)
			{
				btn.UnlockedKnoledge();
			}
		}
		StartCoroutine(Fix());
	}

	public bool UnlockedKnoledge(int index)
	{
		var book = curScientist.Books[0];
		if (SourceBarUI.Instance.source.KnowledgeCur >= book.BookKnoledges[index].Cost)
		{
			book.BookKnoledges[index].IsUnlocked = true;

			World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SourceSystem>().missions.Add(new SourceSystem.SMission
			{
				index = 6,
				value = -book.BookKnoledges[index].Cost,
			});

			foreach (var improvement in book.BookKnoledges[index].Improvements)
			{
				var sImprove = new ImprovementSystem.SMission
				{
					Index = improvement.Index,
					Value = improvement.Value,
				};
				//改变ECS
				World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ImprovementSystem>().missions.Add(sImprove);
				//改变SO
				UnityEngine.Debug.Log("Mono:" + improvement.Index + " " + improvement.Value);
				switch (improvement.Index)
				{
					case EImporoveMent.None:
						break;
					case EImporoveMent.FarmGrowSpeed:
						buildingSO.Wheat.GrowTime0 += improvement.Value;
						break;
					case EImporoveMent.FarmGainValue:
						buildingSO.Wheat.SourceGain += improvement.Value;
						break;
					case EImporoveMent.FarmerWorkTime:
						buildingSO.Wheat.WorkingTime0 += improvement.Value;
						break;
					case EImporoveMent.CharacterMoveSpeed:
						characterSO.MoveSpeed += improvement.Value;
						break;
					case EImporoveMent.ScientistGainValue:
						buildingSO.Institute.SourceGain += improvement.Value;
						break;
					case EImporoveMent.ScientistWorkTime:
						buildingSO.Institute.WorkingTime += improvement.Value;
						break;
					default:
						break;
				}
			}
			return true;

		}
		return false;


	}

	public IEnumerator Fix()
	{
		KnowledgeParent.gameObject.SetActive(false);
		yield return null;
		KnowledgeParent.gameObject.SetActive(true);
	}
}
