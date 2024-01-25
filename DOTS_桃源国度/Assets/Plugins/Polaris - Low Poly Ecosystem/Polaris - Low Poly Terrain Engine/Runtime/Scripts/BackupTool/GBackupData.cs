#if GRIFFIN
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Griffin.BackupTool
{
    //[CreateAssetMenu(menuName = "Griffin/Backup Data")]
    public class GBackupData : ScriptableObject
    {
        private static GBackupData instance;
        public static GBackupData Instance
        {
            get
            {
                if (instance == null)
                {
#if UNITY_EDITOR
                    instance = Resources.Load<GBackupData>("GriffinBackupData");
#endif
                    if (instance == null)
                    {
                        instance = ScriptableObject.CreateInstance<GBackupData>();
                    }
                    //instance.hideFlags = HideFlags.DontSaveInBuild;
                }
                return instance;
            }
        }

        [SerializeField]
        private List<GHistoryEntry> historyEntries;
        public List<GHistoryEntry> HistoryEntries
        {
            get
            {
                if (historyEntries == null)
                {
                    historyEntries = new List<GHistoryEntry>();
                }
                return historyEntries;
            }
            set
            {
                historyEntries = value;
            }
        }
    }
}
#endif
