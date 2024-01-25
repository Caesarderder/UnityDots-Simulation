#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.BackupTool
{
    [System.Serializable]
    public struct GBackupToolSettings
    {
        //[SerializeField]
        //private bool dontClearHistoryOnEditorExit;
        //public bool DontClearHistoryOnEditorExit
        //{
        //    get
        //    {
        //        return dontClearHistoryOnEditorExit;
        //    }
        //    set
        //    {
        //        dontClearHistoryOnEditorExit = value;
        //    }
        //}

        [SerializeField]
        private int bufferSizeMB;
        public int BufferSizeMB
        {
            get
            {
                return bufferSizeMB;
            }
            set
            {
                bufferSizeMB = value;
            }
        }
    }
}
#endif
