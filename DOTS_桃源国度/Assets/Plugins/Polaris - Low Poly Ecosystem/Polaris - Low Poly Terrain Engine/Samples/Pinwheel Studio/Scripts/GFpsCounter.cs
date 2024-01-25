#if GRIFFIN
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Pinwheel.Griffin
{
    public class GFpsCounter : MonoBehaviour
    {
        public static float msec;
        public static float fps;
        public static float avgFps
        {
            get
            {
                return (minFps + maxFps) / 2;
            }
        }
        public static float maxFps = Mathf.NegativeInfinity;
        public static float minFps = Mathf.Infinity;
        public bool showFPS;
        public Text text;
        float deltaTime = 0.0f;

        public void Awake()
        {
            StartCoroutine(ResetCounter(3));
        }

        void Update()
        {
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
            msec = deltaTime * 1000.0f;
            fps = 1.0f / deltaTime;

            minFps = fps < minFps ? fps : minFps;
            maxFps = fps > maxFps ? fps : maxFps;

            if (showFPS)
                text.text = ((int)fps).ToString();
            else
                text.gameObject.SetActive(false);
        }

        public IEnumerator ResetCounter(float period)
        {
            while (true)
            {
                maxFps = Mathf.NegativeInfinity;
                minFps = Mathf.Infinity;
                yield return new WaitForSeconds(period);
            }

        }

        //void OnGUI()
        //{
        //    if (!showFPS)
        //        return;

        //    int w = Screen.width, h = Screen.height;
        //    GUIStyle style = new GUIStyle();
        //    Rect rect = new Rect(0, 0, w, h * 10 / 100);
        //    style.alignment = TextAnchor.UpperRight;
        //    style.fontSize = h * 3 / 100;
        //    style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);

        //    GUI.Label(rect, "\nMin " + minFps, style);
        //    GUI.Label(rect, "\n\nMax " + maxFps, style);
        //    GUI.Label(rect, "\n\n\nAvg " + avgFps, style);

        //    if (fps < avgFps)
        //        style.normal.textColor = Color.red;
        //    else
        //        style.normal.textColor = Color.green;
        //    string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        //    GUI.Label(rect, text, style);

        //}
    }
}
#endif
