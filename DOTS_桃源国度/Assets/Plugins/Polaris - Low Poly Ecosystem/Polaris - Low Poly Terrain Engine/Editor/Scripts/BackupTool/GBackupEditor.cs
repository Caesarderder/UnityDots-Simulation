#if GRIFFIN
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.BackupTool
{
    public class GBackupEditor : EditorWindow
    {
        public static GBackupEditor Instance { get; set; }

        private string backupName;
        private string BackupName
        {
            get
            {
                return backupName;
            }
            set
            {
                backupName = value;
            }
        }

        private int groupId = -1;
        private int GroupID
        {
            get
            {
                return groupId;
            }
            set
            {
                groupId = value;
            }
        }

        private bool useAutoName;
        private bool UseAutoName
        {
            get
            {
                return useAutoName;
            }
            set
            {
                useAutoName = value;
            }
        }

        private Vector2 ScrollPos { get; set; }

        private List<string> backups;
        private List<string> Backups
        {
            get
            {
                if (backups == null)
                    backups = new List<string>();
                return backups;
            }
        }

        public static void ShowWindow()
        {
            GBackupEditor window = GetWindow<GBackupEditor>();
            window.titleContent = new GUIContent("Backup");
            window.minSize = new Vector2(250, 300);
            window.Show();
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            GBackup.Changed += OnBackupChanged;
            Instance = this;
            wantsMouseMove = true;
            groupId = EditorPrefs.GetInt(GEditorCommon.GetProjectRelatedEditorPrefsKey("backupeditor", "groupid"), -1);
            useAutoName = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey("backupeditor", "autoname"), true);
            RefreshBackup();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            GBackup.Changed -= OnBackupChanged;
            Instance = null;
            EditorPrefs.SetInt(GEditorCommon.GetProjectRelatedEditorPrefsKey("backupeditor", "groupid"), groupId);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey("backupeditor", "autoname"), useAutoName);
        }

        private void OnUndoRedo()
        {
            Repaint();
        }

        private void OnBackupChanged()
        {
            RefreshBackup();
        }

        private void OnGUI()
        {
            ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);
            DrawCreateBackupGUI();
            DrawAvailableBackups();
            DrawEditingHistory();
            EditorGUILayout.EndScrollView();

            HandleRepaint();
        }

        private void DrawCreateBackupGUI()
        {
            string label = "Create";
            string id = "createbackup";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 100;

                GroupID = GEditorCommon.ActiveTerrainGroupPopupWithAllOption("Group Id", GroupID);
                GUI.enabled = !UseAutoName;
                BackupName = EditorGUILayout.TextField("Name", BackupName);
                GUI.enabled = true;
                UseAutoName = EditorGUILayout.Toggle("Auto Name", UseAutoName);
                if (UseAutoName)
                {
                    BackupName = GBackupFile.GetBackupNameByTimeNow();
                }

                GUI.enabled = !string.IsNullOrEmpty(BackupName);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.GetControlRect(GUILayout.Width(GEditorCommon.indentSpace));
                Rect r = EditorGUILayout.GetControlRect();
                if (GUI.Button(r, "Create"))
                {
                    EditorUtility.DisplayProgressBar("Backing Up", "Creating backup files...", 1);
                    GUndoCompatibleBuffer.Instance.RecordUndo();
                    GBackup.Create(BackupName, GroupID);
                    EditorUtility.ClearProgressBar();
                }
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;

                EditorGUIUtility.labelWidth = labelWidth;
            });
        }

        private void DrawAvailableBackups()
        {
            string label = "Available Backups";
            string id = "availablebackups";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                int entryCount = 0;
                for (int i = 0; i < Backups.Count; ++i)
                {
                    if (!Backups[i].StartsWith("~"))
                    {
                        entryCount += 1;
                        DrawBackupEntry(Backups[i]);
                    }
                }

                if (entryCount == 0)
                {
                    EditorGUILayout.LabelField("No Backup found!", GEditorCommon.WordWrapItalicLabel);
                }
            });
        }

        private void RefreshBackup()
        {
            List<string> backups = new List<string>(GBackupFile.GetAllBackupNames());
            Backups.Clear();
            Backups.AddRange(backups);
            Repaint();
        }

        private void DrawBackupEntry(string backupName)
        {
            Rect r = EditorGUILayout.GetControlRect();
            if (r.Contains(Event.current.mousePosition))
            {
                Color boxColor = EditorGUIUtility.isProSkin ? GEditorCommon.lightGrey : GEditorCommon.darkGrey;
                GEditorCommon.DrawOutlineBox(r, boxColor);
                if (Event.current != null && Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.button == 0)
                    {
                        ConfirmAndRestoreBackup(backupName);
                    }
                    else if (Event.current.button == 1)
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(
                            new GUIContent("Restore"),
                            false,
                            () => { ConfirmAndRestoreBackup(backupName); });
                        menu.AddItem(
                            new GUIContent("Delete"),
                            false,
                            () => { ConfirmAndDeleteBackup(backupName); });
                        menu.ShowAsContext();
                    }
                }
            }
            EditorGUI.LabelField(r, backupName);

            if (backupName.Equals(GUndoCompatibleBuffer.Instance.CurrentBackupName))
            {
                Rect dotRect = new Rect(r.x, r.y, r.height, r.height);
                GUI.Label(dotRect, GEditorCommon.dot);
            }
        }

        private void ConfirmAndDeleteBackup(string backupName)
        {
            if (EditorUtility.DisplayDialog(
                "Confirm",
                "Delete the selected Backup?\n" +
                "This action cannot be undone!",
                "OK", "Cancel"))
            {
                GBackup.Delete(backupName);
                RefreshBackup();
                Repaint();
            }
        }

        private void ConfirmAndRestoreBackup(string backupName)
        {
            if (EditorUtility.DisplayDialog(
                "Confirm",
                "Restore this Backup?\n" +
                "It's better to save your work before proceeding!",
                "OK", "Cancel"))
            {
                RestoreBackup(backupName);
            }
        }

        private void HandleRepaint()
        {
            if (Event.current != null && Event.current.isMouse)
                Repaint();
        }

        private void DrawEditingHistory()
        {
            string label = "History";
            string id = "history";

            GenericMenu context = new GenericMenu();
            context.AddItem(
                new GUIContent("Clear"),
                false,
                () => { ConfirmAndClearHistory(); });

            GEditorCommon.Foldout(label, false, id, () =>
            {
                int entryCount = 0;
                for (int i = 0; i < Backups.Count; ++i)
                {
                    if (Backups[i].StartsWith("~"))
                    {
                        entryCount += 1;
                        DrawHistoryEntry(Backups[i]);
                    }
                }

                if (entryCount == 0)
                {
                    EditorGUILayout.LabelField("No History found", GEditorCommon.WordWrapItalicLabel);
                }
            },
            context);
        }

        private void ConfirmAndClearHistory()
        {
            if (EditorUtility.DisplayDialog(
                "Confirm",
                "Clear all History?\n" +
                "This action cannot be undone!",
                "OK", "Cancel"))
            {
                EditorUtility.DisplayProgressBar("Deleting", "Deleting History files...", 1);
                try
                {
                    GBackup.ClearHistory();
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
                EditorUtility.ClearProgressBar();
            }
        }

        private void DrawHistoryEntry(string backupName)
        {
            Rect r = EditorGUILayout.GetControlRect();
            if (r.Contains(Event.current.mousePosition))
            {
                Color boxColor = EditorGUIUtility.isProSkin ? GEditorCommon.lightGrey : GEditorCommon.midGrey;
                GEditorCommon.DrawOutlineBox(r, boxColor);
                if (Event.current != null && Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.button == 0)
                    {
                        RestoreBackup(backupName);
                    }
                    else if (Event.current.button == 1)
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(
                            new GUIContent("Restore"),
                            false,
                            () => { RestoreBackup(backupName); });
                        menu.AddItem(
                            new GUIContent("Delete"),
                            false,
                            () => { ConfirmAndDeleteBackup(backupName); });
                        menu.ShowAsContext();
                    }
                }
            }

            string displayName = backupName.Substring(1, backupName.IndexOf('_') - 1);
            EditorGUI.LabelField(r, displayName);

            if (backupName.Equals(GUndoCompatibleBuffer.Instance.CurrentBackupName))
            {
                Rect dotRect = new Rect(r.x, r.y, r.height, r.height);
                GUI.Label(dotRect, GEditorCommon.dot);
            }
        }

        private void RestoreBackup(string backupName)
        {
            EditorUtility.DisplayProgressBar("Restoring", "Restoring terrain data...", 1);
            GUndoCompatibleBuffer.Instance.RecordUndo();
            GBackup.Restore(backupName);
            EditorUtility.ClearProgressBar();
        }
    }
}
#endif
