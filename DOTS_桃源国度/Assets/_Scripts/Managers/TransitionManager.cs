using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class TransitionManager : Singleton<TransitionManager>
{
    public bool isFade;
    public Image fade;
    public float fadeDuration;

    public event Action LoadScene;
    public event Action SaveScene;

    private void Start()
    {
        Quaternion q = Quaternion.identity;
        q.SetFromToRotation(Vector3.one, Vector3.forward);

         //Transition(ESceneName.None,ESceneName.Welcome);
        //StartCoroutine(StartGame());

    }
    private void Update()
    {

    }
    public void Transition(ESceneName from, ESceneName to)
    {

        StartCoroutine(TransitionToSecne(from.ToString(), to.ToString()));


    }
    public void Transition(string from, string to)
    {

        StartCoroutine(TransitionToSecne(from, to));


    }
    public void Transition(int from, int to)
    {

        StartCoroutine(TransitionToSecne(from, to));

    }
    //ʹ��˵����
    /*���˵�����Ϸ����numAny��numLevel��
     * �ؿ�a��b��a��b��
     * ���¿�ʼ�ؿ�a��a��a��
     */
    private IEnumerator TransitionToSecne(int from, int to)
    {
        if (SceneManager.sceneCount != 1)
            yield return SceneManager.UnloadSceneAsync(from);
        if (to != 0)
            yield return SceneManager.LoadSceneAsync(to, LoadSceneMode.Additive);


        //�����³���Ϊ�����
        Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        SceneManager.SetActiveScene(newScene);



    }
    //��Ϸ�еĳ����л�
    private IEnumerator TransitionToSecne(string from, string to)
    {
        print(from + "TO" + to);
        Fade(1f);
        yield return new WaitForSeconds(fadeDuration);
        if (SceneManager.sceneCount != 1&&from!="None")
        {
            OnSaveScene();

            yield return SceneManager.UnloadSceneAsync(from);
        }


        yield return SceneManager.LoadSceneAsync(to, LoadSceneMode.Additive);
        OnLoadScene();



        yield return new WaitForSeconds(fadeDuration);
        //�����³���Ϊ�����
        /*        Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                SceneManager.SetActiveScene(newScene);*/
        Fade(0f);



    }   


    public void Fade(float alph)
    {
        isFade = true;
        fade.transform.gameObject.SetActive(true);
        //fade.DOColor(new Color(1f,1f,1f,alph),1f).OnComplete(FadeFinish);
    }

    public void FadeFinish()
    {
        isFade = false;
        fade.transform.gameObject.SetActive(fade);
    }

    public void OnLoadScene()
    {
        LoadScene?.Invoke();
    }

    public void OnSaveScene()
    {
        SaveScene?.Invoke();
    }


}
