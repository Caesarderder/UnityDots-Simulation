using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    //基本设置
    public float musicVolume;
    public float musicEffectsVolue;

    //对话场景
    public int dialogueStage;

    //玩家信息
    public string playerName;

    //玩家成就
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

    //地形棋解锁顺序
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
            print("加载");
            Load();
        }
           
        if (Input.GetKeyDown(KeyCode.E))
        {
            print("保存");
            Save();
        }
            
    }

}
