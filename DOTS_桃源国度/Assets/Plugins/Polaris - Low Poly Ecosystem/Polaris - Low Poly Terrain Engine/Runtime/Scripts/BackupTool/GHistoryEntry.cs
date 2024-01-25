#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;
using DateTime = System.DateTime;

namespace Pinwheel.Griffin.BackupTool
{
    [System.Serializable]
    public class GHistoryEntry
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
        private DateTime creationTime;
        public DateTime CreationTime
        {
            get
            {
                return creationTime;
            }
            set
            {
                creationTime = value;
            }
        }

        [SerializeField]
        private List<GHistoryBuffer> buffers;
        public List<GHistoryBuffer> Buffers
        {
            get
            {
                if (buffers == null)
                {
                    buffers = new List<GHistoryBuffer>();
                }
                return buffers;
            }
            set
            {
                buffers = value;
            }
        }

        public GHistoryEntry(string n)
        {
            name = n;
            creationTime = DateTime.Now;
            buffers = new List<GHistoryBuffer>();
        }
    }
}
#endif
