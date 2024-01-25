
using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Knoledge/BookSO"), Serializable]
public class BookSO : ScriptableObject
{
	public string Name;
	[TextArea]
	public string Description;

	public List<BookKnoledge> BookKnoledges;

}

[Serializable]
public class BookKnoledge
{
	public float Cost=20f;
	public bool IsUnlocked;
	[TextArea]
	public string Description;
	public List<SBookKnoledgeImprovement> Improvements;
}

[Serializable]
public struct SBookKnoledgeImprovement
{
	public EImporoveMent Index;
	public float Value;
}

public enum EImporoveMent
{
	None,
	FarmGrowSpeed,
	FarmGainValue,
	FarmerWorkTime,
	CharacterMoveSpeed,
	ScientistGainValue,
	ScientistWorkTime,
}
