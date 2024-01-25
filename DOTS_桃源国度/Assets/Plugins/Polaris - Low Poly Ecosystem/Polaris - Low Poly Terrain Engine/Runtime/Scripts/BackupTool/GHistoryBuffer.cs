#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.BackupTool
{
    [System.Serializable]
    public class GHistoryBuffer
    {
        [SerializeField]
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        [SerializeField]
        private byte[] bytes;
        public byte[] Bytes
        {
            get
            {
                return bytes;
            }
            set
            {
                bytes = value;
            }
        }

        public GHistoryBuffer(string bufferName, byte[] data)
        {
            name = bufferName;
            bytes = data;
        }
    }
}
#endif
