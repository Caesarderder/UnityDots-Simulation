#if GRIFFIN
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Griffin.HelpTool
{
    //[CreateAssetMenu(fileName ="HelpDatabase", menuName ="Griffin/Help Database")]
    public class GHelpDatabase : ScriptableObject
    {
        private static GHelpDatabase instance;
        public static GHelpDatabase Instance
        {
            get
            {
                if (instance == null)
                {
#if UNITY_EDITOR
                    instance = Resources.Load<GHelpDatabase>("HelpDatabase");
#endif
                    if (instance == null)
                    {
                        instance = ScriptableObject.CreateInstance<GHelpDatabase>();
                    }
                }
                return instance;
            }
        }

        [SerializeField]
        private List<GHelpEntry> entries;
        public List<GHelpEntry> Entries
        {
            get
            {
                if (entries == null)
                {
                    entries = new List<GHelpEntry>();
                }
                return entries;
            }
        }
    }
}
#endif
