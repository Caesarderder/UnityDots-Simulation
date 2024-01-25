using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    //��������
    public float musicVolume;
    public float musicEffectsVolue;

    //�Ի�����
    public int dialogueStage;

    //�����Ϣ
    public string playerName;

    //��ҳɾ�
    public int[] storyAchievement;

    public bool[] roundBuffUnlocked;
    public bool[] terrainUnlocked;

    /// <summary>
    /// 0-8
    /// </summary>
    public int roundLevel;
    public int maxRound;

    public int maxTerrainNum;
    public int maxRoundBuff;
    public int story1Num;
    public int story2Num;
    public int story3Num;
    public int maxStoryNum;

    //���������˳��
    //public ETerrainType[] terrainTypeUnlockSort;


    private void OnEnable()
    {
        TransitionManager.Instance.LoadScene += Load;
        TransitionManager.Instance.SaveScene += Save;
   /*     if(ES3.KeyExists("SaveManager"))
            ES3.LoadInto("SaveManager", gameObject);*/
    }
    private void OnDisable()
    {
        TransitionManager.Instance.LoadScene -= Load;
        TransitionManager.Instance.SaveScene -= Save;
       // ES3.Save("SaveManager", gameObject);
        
    }
    private void Load()
    {
    }
    private void Save()
    {

     
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            print("����");
            Load();
        }
           
        if (Input.GetKeyDown(KeyCode.E))
        {
            print("����");
            Save();
        }
            
    }

}
