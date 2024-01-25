#if GRIFFIN
using System.Collections;
using System.IO;
using UnityEngine;

namespace Pinwheel.Griffin
{
    public class GTakeScreenshot : MonoBehaviour
    {
        [SerializeField]
        private KeyCode hotKey;
        public KeyCode HotKey
        {
            get
            {
                return hotKey;
            }
            set
            {
                hotKey = value;
            }
        }

        [SerializeField]
        private string fileNamePrefix;
        public string FileNamePrefix
        {
            get
            {
                return fileNamePrefix;
            }
            set
            {
                fileNamePrefix = value;
            }
        }

        private void Reset()
        {
            HotKey = KeyCode.F9;
            FileNamePrefix = "Screenshot";
        }

        private void Update()
        {
            if (Input.GetKeyDown(HotKey))
                StartCoroutine(CrTakeScreenshot());
        }

        private IEnumerator CrTakeScreenshot()
        {
            // wait for graphics to render
            yield return new WaitForEndOfFrame();

            // create a texture to pass to encoding
            Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

            // put buffer into texture
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            texture.Apply();

            // split the process up--ReadPixels() and the GetPixels() call inside of the encoder are both pretty heavy
            yield return 0;

            byte[] bytes = texture.EncodeToPNG();
            System.DateTime d = System.DateTime.Now;
            string timeString = string.Format("{0}-{1}-{2}-{3}-{4}-{5}",
                d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);
            string fileName = string.Format("{0}{1}{2}{3}",
                FileNamePrefix,
                FileNamePrefix == null ? "" : "-",
                timeString,
                ".png");
            string filePath = Application.dataPath + "/" + fileName;
            // save our test image (could also upload to WWW)
            File.WriteAllBytes(filePath, bytes);

            // Added by Karl. - Tell unity to delete the texture, by default it seems to keep hold of it and memory crashes will occur after too many screenshots.
            Object.Destroy(texture);
            Debug.Log("Screenshot saved at: " + filePath);
        }
    }
}
#endif
