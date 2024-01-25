using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameManager : Singleton<GameManager>
{

    [SerializeField]
    private GameObject[] uiGameState;
    [SerializeField]
    public bool OnGame;
    public bool isAllUnlocked;
    [SerializeField]
    private ESceneName nameLevel;

    public int[] roundNeeds;

    private void OnEnable()
    {
        /*TransitionManager.Instance.LoadScene += Load;
        TransitionManager.Instance.SaveScene += Save;*/
    }
    private void OnDisable()
    {
        /*TransitionManager.Instance.LoadScene -= Load;
        TransitionManager.Instance.SaveScene -= Save;*/
    }


    // Start is called before the first frame update
    void Start()
    {
        OnGame = true;
        DOTween.Init(true, true, LogBehaviour.Verbose).SetCapacity(500, 50);
    }
   
    public void Load()
    {
       

    }
    public void Save()
    {
        
    }

    public void TransitionToSpecificLevel(ESceneName sceneName)
    {
        Time.timeScale = 1;
        OnGame = true;
        TransitionManager.Instance.Transition(nameLevel, sceneName);
        nameLevel = sceneName;
    }
    public void TransitionToSpecificLevel(int numNewLevel)
    {
        Time.timeScale = 1;
        OnGame = true;
    }
    /// <summary>
    /// ��һ��
    /// </summary>
    public void TransitionToNextLevel()
    {
        Time.timeScale = 1;
        OnGame = true;
    }
    /// <summary>
    /// ���¿�ʼ���ؿ�
    /// </summary>
    public void TransitionToCurrentLevel()
    {
        Time.timeScale = 1;
        OnGame = true;
        TransitionManager.Instance.Transition(nameLevel, nameLevel);
    }
    /// <summary>
    /// �������˵�
    /// </summary>
    public void TransitionToMainMenu()
    {
        Time.timeScale = 1;
        OnGame = false;


        //UIShow(0);

    }

    /// <summary>
    /// ��ͣ��Ϸ
    /// </summary>
    public void PauseGame()
    {
        OnGame = false;
        //UIShow(1);
        Time.timeScale = 0;
    }
    /// <summary>
    /// ������Ϸ
    /// </summary>
    public void ContinueGame()
    {
        print("Continue");
        OnGame = true;
        //UIShow(10);
        Time.timeScale = 1;
    }

    public void OnGameOver()
    {
        OnGame = false;
        Time.timeScale = 0;
        UIShow(3);
    }

    public void OnNextLevel()
    {
        OnGame = false;
        Time.timeScale = 0;
/*        if (numLevel == numAllLevel)
        {
            OnGameWin();
            return;
        }*/
        UIShow(2);
    }
    public void OnGameWin()
    {
        OnGame = false;
        Time.timeScale = 0;
        UIShow(4);
    }


    //��ʾĳ��UI
    public void UIShow(int n)
    {

        for (int i = 0; i < uiGameState.Length; i++)
        {
            if (i != n)
                uiGameState[i].SetActive(false);
            else
                uiGameState[i].SetActive(true);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public int GetTargetRound()
    {
        return roundNeeds[SaveManager.Instance.roundLevel];
    }
}

public enum ESceneName
{
    None=-3,

    Welcome,
    Dialogue,
    StartGame,
    Level1,
    Level2,
}
