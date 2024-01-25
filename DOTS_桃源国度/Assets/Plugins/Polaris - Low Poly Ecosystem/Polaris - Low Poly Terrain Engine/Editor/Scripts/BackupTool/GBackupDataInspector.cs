#if GRIFFIN
using UnityEditor;

namespace Pinwheel.Griffin.BackupTool
{
    [CustomEditor(typeof(GBackupData))]
    public class GBackupDataInspector : Editor
    {
        private GBackupData instance;
        private void OnEnable()
        {
            instance = target as GBackupData;
        }

        public override void OnInspectorGUI()
        {
        }
    }
}
#endif
