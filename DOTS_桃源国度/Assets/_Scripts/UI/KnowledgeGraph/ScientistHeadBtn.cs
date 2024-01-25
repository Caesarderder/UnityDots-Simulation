using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScientistHeadBtn : MonoBehaviour
{
	public ScientistDataSO Scientist;
	public KnowledgeGraphUI KnowledgeGraphUI;
	public void OnClick()
	{
		KnowledgeGraphUI.ChangeCurScientist(Scientist);
	}
}
