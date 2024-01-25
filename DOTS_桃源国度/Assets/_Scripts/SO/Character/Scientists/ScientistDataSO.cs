using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Data/Knoledge/ScientistDataSO"), Serializable]
public class ScientistDataSO : ScriptableObject
{
	public string Name;
	[TextArea]
	public string Description;
	public float KnoledgeCost;
	public bool IsUnlocked;

	public List<BookSO> Books;

}

