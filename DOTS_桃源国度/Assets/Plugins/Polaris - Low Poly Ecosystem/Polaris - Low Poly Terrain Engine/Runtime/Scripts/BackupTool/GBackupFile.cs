#if GRIFFIN
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pinwheel.Griffin.BackupTool
{
    public static class GBackupFile
    {
        public const string DIRECTORY = "GriffinBackup";
        public const string INITIAL_HISTORY_PREFIX = "Begin";
        public const string EXTENSION = ".gbackup";
        public const string ALBEDO_R_SUFFIX = "albedo_r";
        public const string ALBEDO_G_SUFFIX = "albedo_g";
        public const string ALBEDO_B_SUFFIX = "albedo_b";
        public const string ALBEDO_A_SUFFIX = "albedo_a";
        public const string METALLIC_SUFFIX = "metallic";
        public const string SMOOTHNESS_SUFFIX = "smoothness";
        public const string CONTROL_R_SUFFIX = "control_r";
        public const string CONTROL_G_SUFFIX = "control_g";
        public const string CONTROL_B_SUFFIX = "control_b";
        public const string CONTROL_A_SUFFIX = "control_a";
        public const string TREE_SUFFIX = "tree";
        public const string GRASS_SUFFIX = "grass";
        public const string PROTOTYPEINDEX_SUFFIX = "protoindex";
        public const string POSITION_SUFFIX = "position";
        public const string ROTATION_SUFFIX = "rotation";
        public const string SCALE_SUFFIX = "scale";

        public const string HEIGHT_R_SUFFIX = "heightmap_r";
        public const string HEIGHT_G_SUFFIX = "heightmap_g";
        public const string HEIGHT_B_SUFFIX = "heightmap_b";
        public const string HEIGHT_A_SUFFIX = "heightmap_a";

        public const string MASK_R_SUFFIX = "maskmap_r";
        public const string MASK_G_SUFFIX = "maskmap_g";
        public const string MASK_B_SUFFIX = "maskmap_b";
        public const string MASK_A_SUFFIX = "maskmap_a";

        private static string GetRootDirectory()
        {
            string assetsFolder = Application.dataPath;
            string projectFolder = Directory.GetParent(assetsFolder).FullName;
            return Path.Combine(projectFolder, DIRECTORY);
        }

        public static string GetFileDirectory(string backupName)
        {
            return Path.Combine(GetRootDirectory(), backupName);
        }

        public static string GetFilePath(string backupName, string fileNameNoExt)
        {
            return Path.Combine(GetFileDirectory(backupName), fileNameNoExt + EXTENSION);
        }

        public static bool Exist(string backupName, string fileNameNoExt)
        {
            if (backupName.StartsWith("~"))
            {
                GHistoryEntry entry = GetHistoryEntry(backupName);
                if (entry == null)
                    return false;
                GHistoryBuffer buffer = entry.Buffers.Find(b => b.Name.Equals(fileNameNoExt));
                return buffer != null;
            }
            else
            {
                string filePath = GetFilePath(backupName, fileNameNoExt);
                return File.Exists(filePath);
            }
        }

        public static bool HistoryContainsDataForTerrain(string backupName, string terrainId)
        {
            GHistoryEntry entry = GetHistoryEntry(backupName);
            if (entry == null)
                return false;
            GHistoryBuffer buffer = entry.Buffers.Find(b => b.Name.Contains(terrainId));
            return buffer != null;
        }

        public static string Create(string backupName, string fileNameNoExt, byte[] data)
        {
            if (backupName.StartsWith("~"))
            {
                GHistoryEntry entry = EnsureHistoryEntryExists(backupName);
                GHistoryBuffer buffer = new GHistoryBuffer(fileNameNoExt, data);
                entry.Buffers.Add(buffer);
                return string.Empty;
            }
            else
            {
                GUtilities.EnsureDirectoryExists(Path.Combine(GetRootDirectory(), backupName));
                string filePath = GetFilePath(backupName, fileNameNoExt);
                File.WriteAllBytes(filePath, data);
                return filePath;
            }
        }

        private static GHistoryEntry EnsureHistoryEntryExists(string backupName)
        {
            GHistoryEntry entry = GetHistoryEntry(backupName);
            if (entry == null)
            {
                entry = new GHistoryEntry(backupName);
                GBackupData.Instance.HistoryEntries.Add(entry);
            }
            return entry;
        }

        private static GHistoryEntry GetHistoryEntry(string backupName)
        {
            return GBackupData.Instance.HistoryEntries.Find(e => e.Name.Equals(backupName));
        }

        public static string[] GetAllFilePaths(string backupName)
        {
            List<string> files = new List<string>(Directory.GetFiles(GetFileDirectory(backupName)));
            files.RemoveAll(f => !f.EndsWith(EXTENSION));
            return files.ToArray();
        }

        public static void SetBackupCreationTime(string backupName, System.DateTime time)
        {
            if (backupName.StartsWith("~"))
            {
                GHistoryEntry entry = GetHistoryEntry(backupName);
                if (entry != null)
                {
                    entry.CreationTime = time;
                }
            }
            else
            {
                string folder = GetFileDirectory(backupName);
                if (Directory.Exists(folder))
                {
                    Directory.SetCreationTime(folder, time);
                }
            }
        }

        public static System.DateTime GetBackupCreationTime(string backupName)
        {
            if (backupName.StartsWith("~"))
            {
                GHistoryEntry entry = GetHistoryEntry(backupName);
                if (entry != null)
                {
                    return entry.CreationTime;
                }
                else
                {
                    return System.DateTime.MaxValue;
                }
            }
            else
            {
                string folder = GetFileDirectory(backupName);
                if (Directory.Exists(folder))
                {
                    return Directory.GetCreationTime(folder);
                }
                else
                {
                    return System.DateTime.MaxValue;
                }
            }
        }

        public static string[] GetAllBackupNames()
        {
            GUtilities.EnsureDirectoryExists(GetRootDirectory());
            List<string> names = new List<string>(Directory.GetDirectories(GetRootDirectory()));
            for (int i = 0; i < names.Count; ++i)
            {
                names[i] = Path.GetFileNameWithoutExtension(names[i]);
            }
            names.Sort((b0, b1) =>
            {
                return GBackupFile.GetBackupCreationTime(b0).CompareTo(GBackupFile.GetBackupCreationTime(b1));
            });

            List<GHistoryEntry> historyEntries = GBackupData.Instance.HistoryEntries;
            for (int i = 0; i < historyEntries.Count; ++i)
            {
                names.Add(historyEntries[i].Name);
            }

            return names.ToArray();
        }

        public static void Delete(string backupName)
        {
            if (backupName.StartsWith("~"))
            {
                GBackupData.Instance.HistoryEntries.RemoveAll(e => e.Name.Equals(backupName));
            }
            else
            {
                string folder = GetFileDirectory(backupName);
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
            }
        }

        public static byte[] ReadAllBytes(string backupName, string fileNameNoExtension)
        {
            if (Exist(backupName, fileNameNoExtension))
            {
                if (backupName.StartsWith("~"))
                {
                    GHistoryEntry entry = GBackupData.Instance.HistoryEntries.Find(e => e.Name.Equals(backupName));
                    GHistoryBuffer buffer = entry.Buffers.Find(b => b.Name.Equals(fileNameNoExtension));
                    return buffer.Bytes;
                }
                else
                {
                    return File.ReadAllBytes(GetFilePath(backupName, fileNameNoExtension));
                }
            }
            else
            {
                return null;
            }
        }

        public static string GetBackupNameByTimeNow()
        {
            System.DateTime d = System.DateTime.Now;
            string s = string.Format("{0}{1}{2}{3}{4}{5}", d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);
            return s;
        }

        public static void ClearHistory()
        {
            string[] backupNames = GetAllBackupNames();
            for (int i = 0; i < backupNames.Length; ++i)
            {
                if (backupNames[i].StartsWith("~"))
                {
                    Delete(backupNames[i]);
                }
            }
        }

        public static string GetInitialHistoryPrefix(string prefixWithoutWaveSymbol)
        {
            return string.Format("~{0} {1}", INITIAL_HISTORY_PREFIX, prefixWithoutWaveSymbol);
        }

        public static string GetHistoryPrefix(string prefixWithoutWaveSymbol)
        {
            return string.Format("~{0}", prefixWithoutWaveSymbol);
        }
    }
}
#endif
#endif
