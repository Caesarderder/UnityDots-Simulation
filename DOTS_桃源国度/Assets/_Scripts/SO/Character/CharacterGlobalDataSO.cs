using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Data/CharacterGlobalDataSO")]
public class CharacterGlobalDataSO : ScriptableObject
{
	public float MoveSpeed;
	public float RestTime;
	public float MaxDuration;

	public List<SCharacterPrefab> CharacterPrefabs;

}

[Serializable]
public struct SCharacterPrefab
{
	public int ID;
	public GameObject Prefab;
}
