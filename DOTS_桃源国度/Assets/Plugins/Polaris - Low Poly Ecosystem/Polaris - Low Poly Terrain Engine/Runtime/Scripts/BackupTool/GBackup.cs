#if GRIFFIN
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Pinwheel.Griffin.Compression;

namespace Pinwheel.Griffin.BackupTool
{
    public static class GBackup
    {
        public delegate void BackupChangedHandler();
        public static event BackupChangedHandler Changed;

        static GBackup()
        {
            EditorApplication.quitting += OnEditorQuitting;
        }

        public static void Create(string backupName, int terrainGroupId)
        {
            List<GTerrainResourceFlag> flags = new List<GTerrainResourceFlag>();
            foreach (GTerrainResourceFlag t in System.Enum.GetValues(typeof(GTerrainResourceFlag)))
            {
                flags.Add(t);
            }

            Create(backupName, terrainGroupId, flags);
            CleanUp();
        }

        public static void Create(string backupName, int terrainGroupId, List<GTerrainResourceFlag> flags)
        {
            List<GStylizedTerrain> terrains = new List<GStylizedTerrain>(GStylizedTerrain.ActiveTerrains);
            for (int i = 0; i < terrains.Count; ++i)
            {
                GStylizedTerrain t = terrains[i];
                if (t.TerrainData == null)
                    continue;
                if (terrainGroupId >= 0 && terrainGroupId != t.GroupId)
                    continue;
                try
                {
                    BackupTerrain(t, backupName, flags);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(string.Format("Error on creating backup for {0}: {1}", t.name, e.ToString()));
                }
            }

            GBackupFile.SetBackupCreationTime(backupName, System.DateTime.Now);
            GUndoCompatibleBuffer.Instance.CurrentBackupName = backupName;
            CleanUp();
            if (Changed != null)
                Changed.Invoke();
        }

        public static void Create(string backupName, GStylizedTerrain terrain, List<GTerrainResourceFlag> flags)
        {
            try
            {
                BackupTerrain(terrain, backupName, flags);
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("Error on creating backup for {0}: {1}", terrain.name, e.ToString()));
            }
            GBackupFile.SetBackupCreationTime(backupName, System.DateTime.Now);
            GUndoCompatibleBuffer.Instance.CurrentBackupName = backupName;
            CleanUp();
            if (Changed != null)
                Changed.Invoke();
        }

        public static bool TryCreateInitialBackup(string namePrefix, int terrainGroupId, List<GTerrainResourceFlag> flags, bool showProgress = true)
        {
            bool success = false;

#if UNITY_EDITOR
            if (showProgress)
                GCommonGUI.ProgressBar("Backing up", "Creating terrain backup...", 1f);
#endif

            try
            {
                List<string> createdBackup = new List<string>(GBackupFile.GetAllBackupNames());
                string prefix = GBackupFile.GetHistoryPrefix(namePrefix);
                string initialPrefix = GBackupFile.GetInitialHistoryPrefix(namePrefix);

                bool willCreateInitialBackup = true;
                for (int i = createdBackup.Count - 1; i >= 0; --i)
                {
                    if (createdBackup[i].StartsWith(prefix))
                        continue;
                    else if (createdBackup[i].StartsWith(initialPrefix))
                    {
                        willCreateInitialBackup = false;
                        break;
                    }
                    else
                    {
                        willCreateInitialBackup = true;
                        break;
                    }
                }

                if (willCreateInitialBackup)
                {
                    string backupName = string.Format("{0}_{1}", initialPrefix, GBackupFile.GetBackupNameByTimeNow());
                    GUndoCompatibleBuffer.Instance.RecordUndo();
                    GBackup.Create(backupName, terrainGroupId, flags);
                    success = true;
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("Fail to creating terrain backup: " + e.ToString());
            }
            finally
            {
#if UNITY_EDITOR
                GCommonGUI.ClearProgressBar();
#endif
            }
            return success;
        }

        public static string TryCreateInitialBackup(string namePrefix, GStylizedTerrain terrain, List<GTerrainResourceFlag> flags, bool showProgress = true)
        {
            string backupName = null;

#if UNITY_EDITOR
            if (showProgress)
                GCommonGUI.ProgressBar("Backing up", "Creating terrain backup...", 1f);
#endif

            try
            {
                List<string> createdBackup = new List<string>(GBackupFile.GetAllBackupNames());
                string prefix = GBackupFile.GetHistoryPrefix(namePrefix);
                string initialPrefix = GBackupFile.GetInitialHistoryPrefix(namePrefix);

                bool willCreateInitialBackup = true;
                for (int i = createdBackup.Count - 1; i >= 0; --i)
                {
                    if (createdBackup[i].StartsWith(prefix))
                        continue;
                    else if (createdBackup[i].StartsWith(initialPrefix))
                    {
                        willCreateInitialBackup = false;
                        break;
                    }
                    else
                    {
                        willCreateInitialBackup = true;
                        break;
                    }
                }

                if (!willCreateInitialBackup)
                {
                    if (!GBackupFile.HistoryContainsDataForTerrain(createdBackup[createdBackup.Count - 1], terrain.TerrainData.Id))
                    {
                        willCreateInitialBackup = true;
                    }
                }

                if (willCreateInitialBackup)
                {
                    backupName = string.Format("{0}_{1}", initialPrefix, GBackupFile.GetBackupNameByTimeNow());
                    GUndoCompatibleBuffer.Instance.RecordUndo();
                    GBackup.Create(backupName, terrain, flags);
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("Fail to creating terrain backup: " + e.ToString());
            }
            finally
            {
#if UNITY_EDITOR
                GCommonGUI.ClearProgressBar();
#endif
            }
            return backupName;
        }

        public static bool TryCreateBackup(string namePrefix, int terrainGroupId, List<GTerrainResourceFlag> flags, bool showProgress = true)
        {
            bool success = false;

#if UNITY_EDITOR
            if (showProgress)
                GCommonGUI.ProgressBar("Backing up", "Creating terrain backup...", 1f);
#endif

            try
            {
                string historyPrefix = GBackupFile.GetHistoryPrefix(namePrefix);
                string backupName = string.Format("{0}_{1}", historyPrefix, GBackupFile.GetBackupNameByTimeNow());
                GUndoCompatibleBuffer.Instance.RecordUndo();
                GBackup.Create(backupName, terrainGroupId, flags);
                success = true;
            }
            catch (System.Exception e)
            {
                Debug.Log("Fail to creating terrain backup: " + e.ToString());
            }
            finally
            {
#if UNITY_EDITOR
                GCommonGUI.ClearProgressBar();
#endif
            }

            return success;
        }

        public static string TryCreateBackup(string namePrefix, GStylizedTerrain terrain, List<GTerrainResourceFlag> flags, bool showProgress = true)
        {
            string backupName = null;

#if UNITY_EDITOR
            if (showProgress)
                GCommonGUI.ProgressBar("Backing up", "Creating terrain backup...", 1f);
#endif

            try
            {
                string historyPrefix = GBackupFile.GetHistoryPrefix(namePrefix);
                backupName = string.Format("{0}_{1}", historyPrefix, GBackupFile.GetBackupNameByTimeNow());
                GUndoCompatibleBuffer.Instance.RecordUndo();
                GBackup.Create(backupName, terrain, flags);
            }
            catch (System.Exception e)
            {
                Debug.Log("Fail to creating terrain backup: " + e.ToString());
            }
            finally
            {
#if UNITY_EDITOR
                GCommonGUI.ClearProgressBar();
#endif
            }

            return backupName;
        }

        public static void BackupTerrain(GStylizedTerrain t, string backupName, List<GTerrainResourceFlag> flags)
        {
            if (t.TerrainData == null)
                return;
            if (flags == null || flags.Count == 0)
                return;

            if (flags.Contains(GTerrainResourceFlag.HeightMap))
            {
                BackupHeightMap(t, backupName);
            }

            if (flags.Contains(GTerrainResourceFlag.AlbedoMap))
            {
                BackupAlbedoMap(t, backupName);
            }

            if (flags.Contains(GTerrainResourceFlag.MetallicMap))
            {
                BackupMetallicMap(t, backupName);
            }

            if (flags.Contains(GTerrainResourceFlag.SplatControlMaps))
            {
                BackupSplatControlMaps(t, backupName);
            }

            if (flags.Contains(GTerrainResourceFlag.MaskMap))
            {
                BackupMaskMap(t, backupName);
            }

            if (flags.Contains(GTerrainResourceFlag.TreeInstances))
            {
                BackupTreeInstances(t, backupName);
            }

            if (flags.Contains(GTerrainResourceFlag.GrassInstances))
            {
                BackupGrassInstances(t, backupName);
            }

            CleanUp();
        }

        private static void BackupHeightMap(GStylizedTerrain t, string backupName)
        {
            Color32[] heightmapData = t.TerrainData.Geometry.HeightMap.GetPixels32();
            byte[] dataR = new byte[heightmapData.Length];
            byte[] dataG = new byte[heightmapData.Length];
            byte[] dataB = new byte[heightmapData.Length];
            byte[] dataA = new byte[heightmapData.Length];
            for (int i = 0; i < heightmapData.Length; ++i)
            {
                dataR[i] = heightmapData[i].r;
                dataG[i] = heightmapData[i].g;
                dataB[i] = heightmapData[i].b;
                dataA[i] = heightmapData[i].a;
            }

            dataR = GCompressor.Compress(dataR);
            dataG = GCompressor.Compress(dataG);
            dataB = GCompressor.Compress(dataB);
            dataA = GCompressor.Compress(dataA);

            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.HEIGHT_R_SUFFIX),
                dataR);
            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.HEIGHT_G_SUFFIX),
                dataG);
            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.HEIGHT_B_SUFFIX),
                dataB);
            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.HEIGHT_A_SUFFIX),
                dataA);
        }

        private static void BackupMaskMap(GStylizedTerrain t, string backupName)
        {
            Color32[] maskmapData = t.TerrainData.Mask.MaskMap.GetPixels32();
            byte[] dataR = new byte[maskmapData.Length];
            byte[] dataG = new byte[maskmapData.Length];
            byte[] dataB = new byte[maskmapData.Length];
            byte[] dataA = new byte[maskmapData.Length];
            for (int i = 0; i < maskmapData.Length; ++i)
            {
                dataR[i] = maskmapData[i].r;
                dataG[i] = maskmapData[i].g;
                dataB[i] = maskmapData[i].b;
                dataA[i] = maskmapData[i].a;
            }

            dataR = GCompressor.Compress(dataR);
            dataG = GCompressor.Compress(dataG);
            dataB = GCompressor.Compress(dataB);
            dataA = GCompressor.Compress(dataA);

            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.MASK_R_SUFFIX),
                dataR);
            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.MASK_G_SUFFIX),
                dataG);
            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.MASK_B_SUFFIX),
                dataB);
            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.MASK_A_SUFFIX),
                dataA);
        }

        private static void BackupAlbedoMap(GStylizedTerrain t, string backupName)
        {
            Color32[] albedoColor = t.TerrainData.Shading.AlbedoMap.GetPixels32();
            byte[] albedoR = new byte[albedoColor.Length];
            byte[] albedoG = new byte[albedoColor.Length];
            byte[] albedoB = new byte[albedoColor.Length];
            byte[] albedoA = new byte[albedoColor.Length];
            for (int i = 0; i < albedoColor.Length; ++i)
            {
                albedoR[i] = albedoColor[i].r;
                albedoG[i] = albedoColor[i].g;
                albedoB[i] = albedoColor[i].b;
                albedoA[i] = albedoColor[i].a;
            }
            albedoR = GCompressor.Compress(albedoR);
            albedoG = GCompressor.Compress(albedoG);
            albedoB = GCompressor.Compress(albedoB);
            albedoA = GCompressor.Compress(albedoA);
            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.ALBEDO_R_SUFFIX),
                albedoR);
            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.ALBEDO_G_SUFFIX),
                albedoG);
            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.ALBEDO_B_SUFFIX),
                albedoB);
            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.ALBEDO_A_SUFFIX),
                albedoA);
        }

        private static void BackupMetallicMap(GStylizedTerrain t, string backupName)
        {
            Color32[] metallicColor = t.TerrainData.Shading.MetallicMap.GetPixels32();
            byte[] metallic = new byte[metallicColor.Length];
            byte[] smoothness = new byte[metallicColor.Length];
            for (int i = 0; i < metallicColor.Length; ++i)
            {
                metallic[i] = metallicColor[i].r;
                smoothness[i] = metallicColor[i].a;
            }
            metallic = GCompressor.Compress(metallic);
            smoothness = GCompressor.Compress(smoothness);
            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.METALLIC_SUFFIX),
                metallic);
            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.SMOOTHNESS_SUFFIX),
                smoothness);
        }

        private static void BackupSplatControlMaps(GStylizedTerrain t, string backupName)
        {
            int splatControlCount = t.TerrainData.Shading.SplatControlMapCount;
            for (int controlMapIndex = 0; controlMapIndex < splatControlCount; ++controlMapIndex)
            {
                Texture2D controlMap = t.TerrainData.Shading.GetSplatControl(controlMapIndex);
                Color32[] controlMapColor = controlMap.GetPixels32();
                byte[] controlr = new byte[controlMapColor.Length];
                byte[] controlg = new byte[controlMapColor.Length];
                byte[] controlb = new byte[controlMapColor.Length];
                byte[] controla = new byte[controlMapColor.Length];
                for (int i = 0; i < controlMapColor.Length; ++i)
                {
                    controlr[i] = controlMapColor[i].r;
                    controlg[i] = controlMapColor[i].g;
                    controlb[i] = controlMapColor[i].b;
                    controla[i] = controlMapColor[i].a;
                }
                controlr = GCompressor.Compress(controlr);
                controlg = GCompressor.Compress(controlg);
                controlb = GCompressor.Compress(controlb);
                controla = GCompressor.Compress(controla);
                GBackupFile.Create(
                   backupName,
                   string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.CONTROL_R_SUFFIX, controlMapIndex),
                   controlr);
                GBackupFile.Create(
                   backupName,
                   string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.CONTROL_G_SUFFIX, controlMapIndex),
                   controlg);
                GBackupFile.Create(
                   backupName,
                   string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.CONTROL_B_SUFFIX, controlMapIndex),
                   controlb);
                GBackupFile.Create(
                   backupName,
                   string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.CONTROL_A_SUFFIX, controlMapIndex),
                   controla);
            }
        }

        private static void BackupTreeInstances(GStylizedTerrain t, string backupName)
        {
            if (t.TerrainData.Foliage.Trees == null)
                return;
            List<GTreeInstance> trees = t.TerrainData.Foliage.TreeInstances;
            int[] protoIndices = new int[trees.Count];
            float[] positions = new float[trees.Count * 3];
            float[] rotations = new float[trees.Count * 4];
            float[] scales = new float[trees.Count * 3];
            for (int i = 0; i < trees.Count; ++i)
            {
                GTreeInstance tree = trees[i];
                protoIndices[i] = tree.PrototypeIndex;

                positions[i * 3 + 0] = tree.Position.x;
                positions[i * 3 + 1] = tree.Position.y;
                positions[i * 3 + 2] = tree.Position.z;

                rotations[i * 4 + 0] = tree.Rotation.x;
                rotations[i * 4 + 1] = tree.Rotation.y;
                rotations[i * 4 + 2] = tree.Rotation.z;
                rotations[i * 4 + 3] = tree.Rotation.w;

                scales[i * 3 + 0] = tree.Scale.x;
                scales[i * 3 + 1] = tree.Scale.y;
                scales[i * 3 + 2] = tree.Scale.z;
            }

            byte[] protoIndicesData = new byte[Buffer.ByteLength(protoIndices)];
            Buffer.BlockCopy(protoIndices, 0, protoIndicesData, 0, protoIndicesData.Length);
            protoIndicesData = GCompressor.Compress(protoIndicesData);

            byte[] positionsData = new byte[Buffer.ByteLength(positions)];
            Buffer.BlockCopy(positions, 0, positionsData, 0, positionsData.Length);
            positionsData = GCompressor.Compress(positionsData);

            byte[] rotationsData = new byte[Buffer.ByteLength(rotations)];
            Buffer.BlockCopy(rotations, 0, rotationsData, 0, rotationsData.Length);
            rotationsData = GCompressor.Compress(rotationsData);

            byte[] scalesData = new byte[Buffer.ByteLength(scales)];
            Buffer.BlockCopy(scales, 0, scalesData, 0, scalesData.Length);
            scalesData = GCompressor.Compress(scalesData);

            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.TREE_SUFFIX, GBackupFile.PROTOTYPEINDEX_SUFFIX),
                protoIndicesData);
            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.TREE_SUFFIX, GBackupFile.POSITION_SUFFIX),
                positionsData);
            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.TREE_SUFFIX, GBackupFile.ROTATION_SUFFIX),
                rotationsData);
            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.TREE_SUFFIX, GBackupFile.SCALE_SUFFIX),
                scalesData);
        }

        private static void BackupGrassInstances(GStylizedTerrain t, string backupName)
        {
            if (t.TerrainData.Foliage.Grasses == null)
                return;
            List<GGrassInstance> grasses = new List<GGrassInstance>();
            GGrassPatch[] patches = t.TerrainData.Foliage.GrassPatches;
            for (int i = 0; i < patches.Length; ++i)
            {
                grasses.AddRange(patches[i].Instances);
            }

            int[] protoIndices = new int[grasses.Count];
            float[] positions = new float[grasses.Count * 3];
            float[] rotations = new float[grasses.Count * 4];
            float[] scales = new float[grasses.Count * 3];
            for (int i = 0; i < grasses.Count; ++i)
            {
                GGrassInstance grass = grasses[i];
                protoIndices[i] = grass.PrototypeIndex;

                positions[i * 3 + 0] = grass.Position.x;
                positions[i * 3 + 1] = grass.Position.y;
                positions[i * 3 + 2] = grass.Position.z;

                rotations[i * 4 + 0] = grass.Rotation.x;
                rotations[i * 4 + 1] = grass.Rotation.y;
                rotations[i * 4 + 2] = grass.Rotation.z;
                rotations[i * 4 + 3] = grass.Rotation.w;

                scales[i * 3 + 0] = grass.Scale.x;
                scales[i * 3 + 1] = grass.Scale.y;
                scales[i * 3 + 2] = grass.Scale.z;
            }

            byte[] protoIndicesData = new byte[Buffer.ByteLength(protoIndices)];
            Buffer.BlockCopy(protoIndices, 0, protoIndicesData, 0, protoIndicesData.Length);
            protoIndicesData = GCompressor.Compress(protoIndicesData);

            byte[] positionsData = new byte[Buffer.ByteLength(positions)];
            Buffer.BlockCopy(positions, 0, positionsData, 0, positionsData.Length);
            positionsData = GCompressor.Compress(positionsData);

            byte[] rotationsData = new byte[Buffer.ByteLength(rotations)];
            Buffer.BlockCopy(rotations, 0, rotationsData, 0, rotationsData.Length);
            rotationsData = GCompressor.Compress(rotationsData);

            byte[] scalesData = new byte[Buffer.ByteLength(scales)];
            Buffer.BlockCopy(scales, 0, scalesData, 0, scalesData.Length);
            scalesData = GCompressor.Compress(scalesData);

            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.GRASS_SUFFIX, GBackupFile.PROTOTYPEINDEX_SUFFIX),
                protoIndicesData);
            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.GRASS_SUFFIX, GBackupFile.POSITION_SUFFIX),
                positionsData);
            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.GRASS_SUFFIX, GBackupFile.ROTATION_SUFFIX),
                rotationsData);
            GBackupFile.Create(
                backupName,
                string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.GRASS_SUFFIX, GBackupFile.SCALE_SUFFIX),
                scalesData);
        }

        public static void Delete(string backupName)
        {
            GBackupFile.Delete(backupName);
            if (backupName.Equals(GUndoCompatibleBuffer.Instance.CurrentBackupName))
            {
                GUndoCompatibleBuffer.Instance.CurrentBackupName = string.Empty;
            }

            if (Changed != null)
                Changed.Invoke();
        }

        public static void Restore(string backupName)
        {
            List<GStylizedTerrain> terrains = new List<GStylizedTerrain>(GStylizedTerrain.ActiveTerrains);
            List<GStylizedTerrain> dirtyTerrains = new List<GStylizedTerrain>();
            for (int i = 0; i < terrains.Count; ++i)
            {
                GStylizedTerrain t = terrains[i];
                if (t.TerrainData == null)
                    continue;
                try
                {
#if UNITY_EDITOR
                    GCommonGUI.ProgressBar("Restoring", "Restoring " + t.name + " data...", 1f);
#endif
                    if (RestoreTerrain(t, backupName))
                    {
                        dirtyTerrains.Add(t);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError(string.Format("Error on restoring {0}: {1}", e.ToString(), t.name));
                }
                finally
                {
#if UNITY_EDITOR
                    GCommonGUI.ClearProgressBar();
#endif
                }
            }
            //GStylizedTerrain.MatchEdges(-1);
            for (int i = 0; i < dirtyTerrains.Count; ++i)
            {
                dirtyTerrains[i].MatchEdges();
            }

            GUndoCompatibleBuffer.Instance.CurrentBackupName = backupName;
            CleanUp();
        }

        private static bool RestoreTerrain(GStylizedTerrain t, string backupName)
        {
            RestoreAlbedoMap(t, backupName);
            bool isHeightMapRestored = false;
            isHeightMapRestored = RestoreHeightMap(t, backupName);
            RestoreMetallicMap(t, backupName);
            RestoreSplatControlMaps(t, backupName);
            RestoreMaskMap(t, backupName);
            RestoreTreeInstances(t, backupName);
            RestoreGrassInstances(t, backupName);
            return isHeightMapRestored;
        }

        private static bool RestoreHeightMap(GStylizedTerrain t, string backupName)
        {
            string rFileName = string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.HEIGHT_R_SUFFIX);
            string gFileName = string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.HEIGHT_G_SUFFIX);
            string bFileName = string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.HEIGHT_B_SUFFIX);
            string aFileName = string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.HEIGHT_A_SUFFIX);

            byte[] dataR = GBackupFile.ReadAllBytes(backupName, rFileName);
            byte[] dataG = GBackupFile.ReadAllBytes(backupName, gFileName);
            byte[] dataB = GBackupFile.ReadAllBytes(backupName, bFileName);
            byte[] dataA = GBackupFile.ReadAllBytes(backupName, aFileName);

            if (dataR == null || dataG == null || dataB == null || dataA == null)
                return false;
            dataR = GCompressor.Decompress(dataR);
            dataG = GCompressor.Decompress(dataG);
            dataB = GCompressor.Decompress(dataB);
            dataA = GCompressor.Decompress(dataA);

            Color32[] heightMapColors = new Color32[dataR.Length];
            for (int i = 0; i < heightMapColors.Length; ++i)
            {
                heightMapColors[i].r = dataR[i];
                heightMapColors[i].g = dataG[i];
                heightMapColors[i].b = dataB[i];
                heightMapColors[i].a = dataA[i];
            }

            Color32[] oldHeightMapColors = t.TerrainData.Geometry.HeightMap.GetPixels32();
            int heightMapResolution = Mathf.RoundToInt(Mathf.Sqrt(heightMapColors.LongLength));
            t.TerrainData.Geometry.HeightMapResolution = heightMapResolution;
            t.TerrainData.Geometry.HeightMap.SetPixels32(heightMapColors);
            t.TerrainData.Geometry.HeightMap.Apply();

            List<Rect> dirtyRects = new List<Rect>(GCommon.CompareTerrainTexture(t.TerrainData.Geometry.ChunkGridSize, oldHeightMapColors, heightMapColors));
            for (int i = 0; i < dirtyRects.Count; ++i)
            {
                t.TerrainData.Geometry.SetRegionDirty(dirtyRects[i]);
            }
            t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Geometry);
            return true;
        }

        private static void RestoreMaskMap(GStylizedTerrain t, string backupName)
        {
            string rFileName = string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.MASK_R_SUFFIX);
            string gFileName = string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.MASK_G_SUFFIX);
            string bFileName = string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.MASK_B_SUFFIX);
            string aFileName = string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.MASK_A_SUFFIX);

            byte[] dataR = GBackupFile.ReadAllBytes(backupName, rFileName);
            byte[] dataG = GBackupFile.ReadAllBytes(backupName, gFileName);
            byte[] dataB = GBackupFile.ReadAllBytes(backupName, bFileName);
            byte[] dataA = GBackupFile.ReadAllBytes(backupName, aFileName);

            if (dataR == null || dataG == null || dataB == null || dataA == null)
                return;
            dataR = GCompressor.Decompress(dataR);
            dataG = GCompressor.Decompress(dataG);
            dataB = GCompressor.Decompress(dataB);
            dataA = GCompressor.Decompress(dataA);

            Color32[] mapColors = new Color32[dataR.Length];
            for (int i = 0; i < mapColors.Length; ++i)
            {
                mapColors[i].r = dataR[i];
                mapColors[i].g = dataG[i];
                mapColors[i].b = dataB[i];
                mapColors[i].a = dataA[i];
            }

            int resolution = Mathf.RoundToInt(Mathf.Sqrt(mapColors.LongLength));
            t.TerrainData.Mask.MaskMapResolution = resolution;
            t.TerrainData.Mask.MaskMap.SetPixels32(mapColors);
            t.TerrainData.Mask.MaskMap.Apply();
        }

        private static void RestoreAlbedoMap(GStylizedTerrain t, string backupName)
        {
            string albedoRFileName = string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.ALBEDO_R_SUFFIX);
            string albedoGFileName = string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.ALBEDO_G_SUFFIX);
            string albedoBFileName = string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.ALBEDO_B_SUFFIX);
            string albedoAFileName = string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.ALBEDO_A_SUFFIX);
            byte[] albedoRData = GBackupFile.ReadAllBytes(backupName, albedoRFileName);
            byte[] albedoGData = GBackupFile.ReadAllBytes(backupName, albedoGFileName);
            byte[] albedoBData = GBackupFile.ReadAllBytes(backupName, albedoBFileName);
            byte[] albedoAData = GBackupFile.ReadAllBytes(backupName, albedoAFileName);
            if (albedoRData != null &&
                albedoGData != null &&
                albedoBData != null &&
                albedoAData != null)
            {
                albedoRData = GCompressor.Decompress(albedoRData);
                albedoGData = GCompressor.Decompress(albedoGData);
                albedoBData = GCompressor.Decompress(albedoBData);
                albedoAData = GCompressor.Decompress(albedoAData);
                Color32[] albedoColors = new Color32[albedoRData.Length];
                for (int i = 0; i < albedoRData.Length; ++i)
                {
                    albedoColors[i] = new Color32(albedoRData[i], albedoGData[i], albedoBData[i], albedoAData[i]);
                }

                bool willRegenerateGeometry = t.TerrainData.Geometry.AlbedoToVertexColorMode != GAlbedoToVertexColorMode.None;
                if (willRegenerateGeometry)
                {
                    Color32[] oldAlbedoColor = t.TerrainData.Shading.AlbedoMap.GetPixels32();
                    List<Rect> dirtyRects = new List<Rect>(GCommon.CompareTerrainTexture(t.TerrainData.Geometry.ChunkGridSize, oldAlbedoColor, albedoColors));
                    t.TerrainData.Geometry.SetRegionDirty(dirtyRects);
                    willRegenerateGeometry = true;

                }

                int resolution = Mathf.RoundToInt(Mathf.Sqrt(albedoRData.LongLength));
                t.TerrainData.Shading.AlbedoMapResolution = resolution;
                t.TerrainData.Shading.AlbedoMap.SetPixels32(albedoColors);
                t.TerrainData.Shading.AlbedoMap.Apply();
                t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Shading);

                if (willRegenerateGeometry)
                {
                    t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Geometry);
                }
            }
        }

        private static void RestoreMetallicMap(GStylizedTerrain t, string backupName)
        {
            string metallicFileName = string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.METALLIC_SUFFIX);
            string smoothnessFileName = string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.SMOOTHNESS_SUFFIX);
            byte[] metallicData = GBackupFile.ReadAllBytes(backupName, metallicFileName);
            byte[] smoothnessData = GBackupFile.ReadAllBytes(backupName, smoothnessFileName);
            if (metallicData != null &&
                smoothnessData != null)
            {
                metallicData = GCompressor.Decompress(metallicData);
                smoothnessData = GCompressor.Decompress(smoothnessData);
                Color32[] metallicSmoothnessColors = new Color32[metallicData.Length];
                for (int i = 0; i < metallicData.Length; ++i)
                {
                    metallicSmoothnessColors[i] = new Color32(metallicData[i], metallicData[i], metallicData[i], smoothnessData[i]);
                }
                int resolution = Mathf.RoundToInt(Mathf.Sqrt(metallicData.Length));
                t.TerrainData.Shading.MetallicMapResolution = resolution;
                t.TerrainData.Shading.MetallicMap.SetPixels32(metallicSmoothnessColors);
                t.TerrainData.Shading.MetallicMap.Apply();
                t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Shading);
            }
        }

        private static void RestoreSplatControlMaps(GStylizedTerrain t, string backupName)
        {
            int controlMapCount = t.TerrainData.Shading.SplatControlMapCount;
            for (int controlIndex = 0; controlIndex < controlMapCount; ++controlIndex)
            {
                string controlrFileName = string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.CONTROL_R_SUFFIX, controlIndex);
                string controlgFileName = string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.CONTROL_G_SUFFIX, controlIndex);
                string controlbFileName = string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.CONTROL_B_SUFFIX, controlIndex);
                string controlaFileName = string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.CONTROL_A_SUFFIX, controlIndex);
                byte[] controlr = GBackupFile.ReadAllBytes(backupName, controlrFileName);
                byte[] controlg = GBackupFile.ReadAllBytes(backupName, controlgFileName);
                byte[] controlb = GBackupFile.ReadAllBytes(backupName, controlbFileName);
                byte[] controla = GBackupFile.ReadAllBytes(backupName, controlaFileName);
                if (controlr != null &&
                    controlg != null &&
                    controlb != null &&
                    controla != null)
                {
                    controlr = GCompressor.Decompress(controlr);
                    controlg = GCompressor.Decompress(controlg);
                    controlb = GCompressor.Decompress(controlb);
                    controla = GCompressor.Decompress(controla);
                    Color32[] controlMapColors = new Color32[controlr.Length];
                    for (int i = 0; i < controlMapColors.Length; ++i)
                    {
                        controlMapColors[i] = new Color32(controlr[i], controlg[i], controlb[i], controla[i]);
                    }
                    int resolution = Mathf.RoundToInt(Mathf.Sqrt(controlMapColors.Length));
                    t.TerrainData.Shading.SplatControlResolution = resolution;
                    Texture2D controlMap = t.TerrainData.Shading.GetSplatControl(controlIndex);
                    controlMap.SetPixels32(controlMapColors);
                    controlMap.Apply();
                }
                t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Shading);
            }
            LogIncorrectSplatConfigWarning(t, backupName);
        }

        private static void RestoreTreeInstances(GStylizedTerrain t, string backupName)
        {
            string prototyeIndicesFileName = string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.TREE_SUFFIX, GBackupFile.PROTOTYPEINDEX_SUFFIX);
            string positionsFileName = string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.TREE_SUFFIX, GBackupFile.POSITION_SUFFIX);
            string rotationsFileName = string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.TREE_SUFFIX, GBackupFile.ROTATION_SUFFIX);
            string scalesFileName = string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.TREE_SUFFIX, GBackupFile.SCALE_SUFFIX);
            byte[] protoIndicesData = GBackupFile.ReadAllBytes(backupName, prototyeIndicesFileName);
            byte[] positionsData = GBackupFile.ReadAllBytes(backupName, positionsFileName);
            byte[] rotationData = GBackupFile.ReadAllBytes(backupName, rotationsFileName);
            byte[] scalesData = GBackupFile.ReadAllBytes(backupName, scalesFileName);
            if (protoIndicesData != null &&
                positionsData != null &&
                rotationData != null &&
                scalesData != null)
            {
                protoIndicesData = GCompressor.Decompress(protoIndicesData);
                positionsData = GCompressor.Decompress(positionsData);
                rotationData = GCompressor.Decompress(rotationData);
                scalesData = GCompressor.Decompress(scalesData);

                int[] indices = new int[protoIndicesData.Length / sizeof(int)];
                float[] positions = new float[positionsData.Length / sizeof(float)];
                float[] rotations = new float[rotationData.Length / sizeof(float)];
                float[] scales = new float[scalesData.Length / sizeof(float)];

                Buffer.BlockCopy(protoIndicesData, 0, indices, 0, protoIndicesData.Length);
                Buffer.BlockCopy(positionsData, 0, positions, 0, positionsData.Length);
                Buffer.BlockCopy(rotationData, 0, rotations, 0, rotationData.Length);
                Buffer.BlockCopy(scalesData, 0, scales, 0, scalesData.Length);

                List<GTreeInstance> trees = new List<GTreeInstance>();
                for (int i = 0; i < indices.Length; ++i)
                {
                    GTreeInstance tree = GTreeInstance.Create(indices[i]);
                    tree.Position = new Vector3(
                        positions[i * 3 + 0],
                        positions[i * 3 + 1],
                        positions[i * 3 + 2]);
                    tree.Rotation = new Quaternion(
                        rotations[i * 4 + 0],
                        rotations[i * 4 + 1],
                        rotations[i * 4 + 2],
                        rotations[i * 4 + 3]);
                    tree.Scale = new Vector3(
                        scales[i * 3 + 0],
                        scales[i * 3 + 1],
                        scales[i * 3 + 2]);
                    trees.Add(tree);
                }

                t.TerrainData.Foliage.TreeInstances.Clear();
                t.TerrainData.Foliage.TreeInstances.AddRange(trees);
                t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Foliage);
            }
        }

        private static void RestoreGrassInstances(GStylizedTerrain t, string backupName)
        {
            string prototyeIndicesFileName = string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.GRASS_SUFFIX, GBackupFile.PROTOTYPEINDEX_SUFFIX);
            string positionsFileName = string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.GRASS_SUFFIX, GBackupFile.POSITION_SUFFIX);
            string rotationsFileName = string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.GRASS_SUFFIX, GBackupFile.ROTATION_SUFFIX);
            string scalesFileName = string.Format("{0}_{1}_{2}", t.TerrainData.Id, GBackupFile.GRASS_SUFFIX, GBackupFile.SCALE_SUFFIX);
            byte[] protoIndicesData = GBackupFile.ReadAllBytes(backupName, prototyeIndicesFileName);
            byte[] positionsData = GBackupFile.ReadAllBytes(backupName, positionsFileName);
            byte[] rotationData = GBackupFile.ReadAllBytes(backupName, rotationsFileName);
            byte[] scalesData = GBackupFile.ReadAllBytes(backupName, scalesFileName);
            if (protoIndicesData != null &&
                positionsData != null &&
                rotationData != null &&
                scalesData != null)
            {
                protoIndicesData = GCompressor.Decompress(protoIndicesData);
                positionsData = GCompressor.Decompress(positionsData);
                rotationData = GCompressor.Decompress(rotationData);
                scalesData = GCompressor.Decompress(scalesData);

                int[] indices = new int[protoIndicesData.Length / sizeof(int)];
                float[] positions = new float[positionsData.Length / sizeof(float)];
                float[] rotations = new float[rotationData.Length / sizeof(float)];
                float[] scales = new float[scalesData.Length / sizeof(float)];

                Buffer.BlockCopy(protoIndicesData, 0, indices, 0, protoIndicesData.Length);
                Buffer.BlockCopy(positionsData, 0, positions, 0, positionsData.Length);
                Buffer.BlockCopy(rotationData, 0, rotations, 0, rotationData.Length);
                Buffer.BlockCopy(scalesData, 0, scales, 0, scalesData.Length);

                List<GGrassInstance> grasses = new List<GGrassInstance>();
                for (int i = 0; i < indices.Length; ++i)
                {
                    GGrassInstance grass = GGrassInstance.Create(indices[i]);
                    grass.Position = new Vector3(
                        positions[i * 3 + 0],
                        positions[i * 3 + 1],
                        positions[i * 3 + 2]);
                    grass.Rotation = new Quaternion(
                        rotations[i * 4 + 0],
                        rotations[i * 4 + 1],
                        rotations[i * 4 + 2],
                        rotations[i * 4 + 3]);
                    grass.Scale = new Vector3(
                        scales[i * 3 + 0],
                        scales[i * 3 + 1],
                        scales[i * 3 + 2]);
                    grasses.Add(grass);
                }

                t.TerrainData.Foliage.ClearGrassInstances();
                t.TerrainData.Foliage.AddGrassInstances(grasses);

                t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Foliage);
            }
        }

        private static void LogIncorrectSplatConfigWarning(GStylizedTerrain t, string backupName)
        {
            if (backupName.StartsWith("~"))
            {
                //don't log when it is a History backup
                return;
            }
            List<string> filePaths = new List<string>(GBackupFile.GetAllFilePaths(backupName));
            int backupCount = filePaths.FindAll(p => p.Contains(string.Format("{0}_{1}", t.TerrainData.Id, GBackupFile.CONTROL_R_SUFFIX))).Count;
            int requiredCount = t.TerrainData.Shading.SplatControlMapCount;
            if (backupCount != requiredCount)
            {
                string s = string.Format(
                    "{0}: {1} Splat Control Map{2} found in the backup, while the current Splat config requires {3}. The result may looks different!",
                    t.name,
                    backupCount,
                    backupCount >= 2 ? "s" : "",
                    requiredCount);
                Debug.LogWarning(s);
            }
        }

        public static void ClearHistory()
        {
            GBackupFile.ClearHistory();
            Undo.ClearUndo(GUndoCompatibleBuffer.Instance);
            if (Changed != null)
                Changed.Invoke();
        }

        private static void OnEditorQuitting()
        {
            //if (GGriffinSettings.Instance.BackupToolSettings.DontClearHistoryOnEditorExit)
            //    return;

            //GBackupFile.ClearHistory();
        }

        private static void CleanUp()
        {
            GCompressor.CleanUp();
        }
    }
}
#endif
#endif
