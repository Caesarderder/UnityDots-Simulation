#if GRIFFIN
#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin.BackupTool
{
    public static class GBackupInternal
    {
        public static string TryCreateAndMergeInitialBackup(string historyPrefix, List<GStylizedTerrain> terrains, List<GTerrainResourceFlag> flags, bool showProgess = true)
        {
            if (terrains.Count == 0)
                return null;
            string backupName = GBackup.TryCreateInitialBackup(historyPrefix, terrains[0], flags, showProgess);
            if (!string.IsNullOrEmpty(backupName))
            {
                for (int i = 1; i < terrains.Count; ++i)
                {
                    GBackup.BackupTerrain(terrains[i], backupName, flags);
                }
            }
            return backupName;
        }

        public static string TryCreateAndMergeBackup(string historyPrefix, List<GStylizedTerrain> terrains, List<GTerrainResourceFlag> flags, bool showProgress = true)
        {
            if (terrains.Count == 0)
                return null;
            string backupName = GBackup.TryCreateBackup(historyPrefix, terrains[0], flags, showProgress);
            if (!string.IsNullOrEmpty(backupName))
            {
                for (int i = 1; i < terrains.Count; ++i)
                {
                    GBackup.BackupTerrain(terrains[i], backupName, flags);
                }
            }
            return backupName;
        }
    }
}
#endif
#endif
