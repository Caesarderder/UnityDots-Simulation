using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingBarUI : MonoBehaviour
{
	[SerializeField]
	private GameObject buttonGo;
	[SerializeField]
	private ObjectsDatabaseSO databaseSO;


    // Start is called before the first frame update
    void Start()
    {

		foreach (var buildingData in databaseSO.objectsData)
		{
			if(buildingData.ID != 0)
			{
				var btnGo= Instantiate(buttonGo,transform);
				var btnInfo = btnGo.AddComponent<BuildingButtonUI>();
				btnInfo.ID = buildingData.ID;

				var btn= btnGo.GetComponent<Button>();
				btnGo.GetComponentInChildren<TMP_Text>().text = buildingData.Name;
				btn.onClick.AddListener(btnInfo.OnButtonClick) ;

			}
		}
		
	}
}
