#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin
{
    //[CreateAssetMenu(menuName = "Griffin/Text Asset")]
    public class GTextAsset : ScriptableObject
    {
        [SerializeField]
        private string text;
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }

        public void Reset()
        {
        }
    }
}
#endif
