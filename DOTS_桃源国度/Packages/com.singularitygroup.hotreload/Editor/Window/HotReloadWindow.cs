
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using SingularityGroup.HotReload.DTO;
using SingularityGroup.HotReload.Editor.Cli;
using SingularityGroup.HotReload.Editor.Semver;
using UnityEditor;
using UnityEngine;

[assembly: InternalsVisibleTo("SingularityGroup.HotReload.EditorSamples")]

namespace SingularityGroup.HotReload.Editor {
    class HotReloadWindow : EditorWindow {
        public static HotReloadWindow Current { get; private set; }

        static readonly Dictionary<string, PatchInfo> pendingPatches = new Dictionary<string, PatchInfo>();

        List<HotReloadTabBase> tabs;
        List<HotReloadTabBase> Tabs => tabs ?? (tabs = new List<HotReloadTabBase> {
            RunTab,
            SettingsTab,
            AboutTab,
        });
        int selectedTab;

        Vector2 scrollPos;


        HotReloadRunTab runTab;
        internal HotReloadRunTab RunTab => runTab ?? (runTab = new HotReloadRunTab(this));
        HotReloadSettingsTab settingsTab;
        internal HotReloadSettingsTab SettingsTab => settingsTab ?? (settingsTab = new HotReloadSettingsTab(this));
        HotReloadAboutTab aboutTab;
        internal HotReloadAboutTab AboutTab => aboutTab ?? (aboutTab = new HotReloadAboutTab(this));

        ShowOnStartupEnum _showOnStartupOption;

        /// <summary>
        /// This token is cancelled when the EditorWindow is disabled.
        /// </summary>
        /// <remarks>
        /// Use it for all tasks.
        /// When token is cancelled, scripts are about to be recompiled and this will cause tasks to fail for weird reasons.
        /// </remarks>
        public CancellationToken cancelToken;
        CancellationTokenSource cancelTokenSource;

        static readonly PackageUpdateChecker packageUpdateChecker = new PackageUpdateChecker();

        [MenuItem("Window/Hot Reload &#H")]
        internal static void Open() {
            // opening the window on CI systems was keeping Unity open indefinitely
            if (EditorWindowHelper.IsHumanControllingUs()) {
                if (Current) {
                    Current.Show();
                    Current.Focus();
                } else {
                    Current = GetWindow<HotReloadWindow>();
                }
            }
        }

        void OnEnable() {
            Current = this;
            if (cancelTokenSource != null) {
                cancelTokenSource.Cancel();
            }
            cancelTokenSource = new CancellationTokenSource();
            cancelToken = cancelTokenSource.Token;


            this.minSize = new Vector2(425, 150f);
            var tex = Resources.Load<Texture>(HotReloadWindowStyles.IsDarkMode ? "Icon_DarkMode" : "Icon_LightMode");
            this.titleContent = new GUIContent(" Hot Reload", tex);
            this._showOnStartupOption = HotReloadPrefs.ShowOnStartup;

            foreach (var patch in CodePatcher.I.PendingPatches) {
                pendingPatches.Add(patch.id, new PatchInfo(patch));
            }
            packageUpdateChecker.StartCheckingForNewVersion();
        }

        void Update() {
            foreach (var tab in Tabs) {
                tab.Update();
            }
        }

        void OnDisable() {
            if (cancelTokenSource != null) {
                cancelTokenSource.Cancel();
                cancelTokenSource = null;
            }

            if (Current == this) {
                Current = null;
            }
        }

        internal void SelectTab(Type tabType) {
            selectedTab = Tabs.FindIndex(x => x.GetType() == tabType);
        }

        void OnGUI() {
            using(var scope = new EditorGUILayout.ScrollViewScope(scrollPos, false, false)) {
                scrollPos = scope.scrollPosition;
                // RenderDebug();
                RenderTabs();
            }
            GUILayout.FlexibleSpace(); // GUI below will be rendered on the bottom
            RenderBottomBar();
        }

        void RenderDebug() {
            if (GUILayout.Button("RESET WINDOW")) {
                OnDisable();

                RequestHelper.RequestLogin("test", "test", 1).Forget();

                HotReloadPrefs.RemoteServer = false;
                HotReloadPrefs.RemoteServerHost = null;
                HotReloadPrefs.LicenseEmail = null;
                HotReloadPrefs.ExposeServerToLocalNetwork = true;
                HotReloadPrefs.LicensePassword = null;
                HotReloadPrefs.LoggedBurstHint = false;
                HotReloadPrefs.DontShowPromptForDownload = false;
                HotReloadPrefs.RateAppShown = false;
                HotReloadPrefs.ActiveDays = string.Empty;
                HotReloadPrefs.LaunchOnEditorStart = false;
                HotReloadPrefs.ShowUnsupportedChanges = true;
                HotReloadPrefs.RedeemLicenseEmail = null;
                HotReloadPrefs.RedeemLicenseInvoice = null;
                OnEnable();
                File.Delete(EditorCodePatcher.serverDownloader.GetExecutablePath(HotReloadCli.controller));
                InstallUtility.DebugClearInstallState();
                InstallUtility.CheckForNewInstall();
                EditorPrefs.DeleteKey(Attribution.LastLoginKey);
                File.Delete(RedeemLicenseHelper.registerOutcomePath);
                AssetDatabase.Refresh();
            }
        }

        void RenderLogo() {
            var isDarkMode = HotReloadWindowStyles.IsDarkMode;
            var tex = Resources.Load<Texture>(isDarkMode ? "Logo_HotReload_DarkMode" : "Logo_HotReload_LightMode");
            //Can happen during player builds where Editor Resources are unavailable
            if(tex == null) {
                return;
            }
            var targetWidth = 243;
            var targetHeight = 44;
            GUILayout.Space(4f);
            // background padding top and bottom
            float padding = 5f;
            // reserve layout space for the texture
            var backgroundRect = GUILayoutUtility.GetRect(targetWidth + padding, targetHeight + padding, HotReloadWindowStyles.LogoStyle);
            // draw the texture into that reserved space. First the bg then the logo.
            if (isDarkMode) {
                GUI.DrawTexture(backgroundRect, EditorTextures.DarkGray17, ScaleMode.StretchToFill);
            } else {
                GUI.DrawTexture(backgroundRect, EditorTextures.LightGray238, ScaleMode.StretchToFill);
            }
            
            var foregroundRect = backgroundRect;
            foregroundRect.yMin += padding;
            foregroundRect.yMax -= padding;
            // during player build (EditorWindow still visible), Resources.Load returns null
            if (tex) {
                GUI.DrawTexture(foregroundRect, tex, ScaleMode.ScaleToFit);
            }
        }

        void RenderTabs() {
            using(new EditorGUILayout.VerticalScope(HotReloadWindowStyles.BoxStyle)) {
                selectedTab = GUILayout.Toolbar(
                    selectedTab,
                    Tabs.Select(t => new GUIContent(t.Title.StartsWith(" ", StringComparison.Ordinal) ? t.Title : " " + t.Title, t.Icon, t.Tooltip)).ToArray(),
                    GUILayout.Height(22f) // required, otherwise largest icon height determines toolbar height
                );
                RenderLogo();

                Tabs[selectedTab].OnGUI();
            }
        }

        void RenderBottomBar() {
            SemVersion newVersion;
            var updateAvailable = packageUpdateChecker.TryGetNewVersion(out newVersion); 

            RenderRateApp();
            
            // var updateAvailable = true;
            // newVersion = SemVersion.Parse("9.9.9");
            using(new EditorGUILayout.HorizontalScope("ProjectBrowserBottomBarBg", GUILayout.ExpandWidth(true), GUILayout.Height(25f))) {
                RenderAutoRefreshTroubleshooting();
            }
            using(new EditorGUILayout.HorizontalScope("ProjectBrowserBottomBarBg", GUILayout.ExpandWidth(true), GUILayout.Height(updateAvailable ? 28f : 25f))) {
                RenderBottomBarCore(updateAvailable, newVersion);
            }
        }

        static GUIStyle _renderAppBoxStyle;
        static GUIStyle renderAppBoxStyle => _renderAppBoxStyle ?? (_renderAppBoxStyle = new GUIStyle(GUI.skin.box) {
            padding = new RectOffset(10, 10, 10, 10)
        });
        
        static GUILayoutOption[] _nonExpandable;
        public static GUILayoutOption[] NonExpandableLayout => _nonExpandable ?? (_nonExpandable = new [] {GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)});
        
        void RenderRateApp() {
            if (HotReloadPrefs.RateAppShown) {
                return;
            }
            var activeDays = EditorCodePatcher.GetActiveDaysForRateApp();
            if (activeDays.Count < Constants.DaysToRateApp) {
                return;
            }
            EditorGUILayout.BeginVertical(renderAppBoxStyle);
            EditorGUILayout.BeginHorizontal();
            HotReloadGUIHelper.HelpBox("Are you enjoying using Hot Reload?", MessageType.Info, 11);
            if (GUILayout.Button("Hide", NonExpandableLayout)) {
                RequestHelper.RequestEditorEventWithRetry(new Stat(StatSource.Client, StatLevel.Debug, StatFeature.RateApp), new EditorExtraData { { "dismissed", true } }).Forget();
                HotReloadPrefs.RateAppShown = true;
            }
            EditorGUILayout.EndHorizontal();
            using (new EditorGUILayout.HorizontalScope()) {
                if (GUILayout.Button("Yes")) {
                    var openedUrl = PackageConst.IsAssetStoreBuild && EditorUtility.DisplayDialog("Rate Hot Reload", "Thank you for using Hot Reload!\n\nPlease consider leaving a review on the Asset Store to support us.", "Open in browser", "Cancel");
                    if (openedUrl) {
                        Application.OpenURL(Constants.UnityStoreRateAppURL);
                    }
                    HotReloadPrefs.RateAppShown = true;
                    var data = new EditorExtraData();
                    if (PackageConst.IsAssetStoreBuild) {
                        data.Add("opened_url", openedUrl);
                    }
                    data.Add("enjoy_app", true);
                    RequestHelper.RequestEditorEventWithRetry(new Stat(StatSource.Client, StatLevel.Debug, StatFeature.RateApp), data).Forget();
                }
                if (GUILayout.Button("No")) {
                    HotReloadPrefs.RateAppShown = true;
                    var data = new EditorExtraData();
                    data.Add("enjoy_app", false);
                    RequestHelper.RequestEditorEventWithRetry(new Stat(StatSource.Client, StatLevel.Debug, StatFeature.RateApp), data).Forget();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
        
        static readonly OpenURLButton autoRefreshTroubleshootingBtn = new OpenURLButton("Troubleshooting", Constants.TroubleshootingURL);
        void RenderAutoRefreshTroubleshooting() {
            EditorGUILayout.LabelField("Issues with Unity Auto Refresh?", GUILayout.Width(180));
            autoRefreshTroubleshootingBtn.OnGUI();
            GUILayout.FlexibleSpace();
        }

        void RenderBottomBarCore(bool updateAvailable, SemVersion newVersion) {
            if (updateAvailable) {
                var btn = EditorStyles.miniButton;
                var prevStyle = btn.fontStyle;
                var prevSize = btn.fontSize;
                var prevHeight = btn.fixedHeight;
                try {
                    btn.fontStyle = FontStyle.Bold;
                    btn.fontSize = 11;
                    btn.fixedHeight = 20.45f; // make same height as documentation button next to it
                    if (GUILayout.Button($"Update To v{newVersion}", btn, GUILayout.MaxWidth(140), GUILayout.ExpandHeight(true))) {
                        packageUpdateChecker.UpdatePackageAsync(newVersion).Forget(CancellationToken.None);
                        
                        //open changelog
                        HotReloadPrefs.ShowChangeLog = true;
                        Current.SelectTab(typeof(HotReloadAboutTab));
                    }
                } finally {
                    btn.fontStyle = prevStyle;
                    btn.fontSize = prevSize;
                    btn.fixedHeight = prevHeight;
                }
            }
            
            aboutTab.documentationButton.OnGUI();
            GUILayout.FlexibleSpace();
            var prevLabelWidth = EditorGUIUtility.labelWidth;
            try {
                EditorGUIUtility.labelWidth = 105f;
                using (new GUILayout.VerticalScope()) {
                    GUILayout.FlexibleSpace();
                    using (new GUILayout.HorizontalScope()) {
                        GUILayout.Label("Show On Startup");
                        Rect buttonRect = GUILayoutUtility.GetLastRect();
                        if (EditorGUILayout.DropdownButton(new GUIContent(Regex.Replace(_showOnStartupOption.ToString(), "([a-z])([A-Z])", "$1 $2")), FocusType.Passive, GUILayout.Width(110f))) {
                            GenericMenu menu = new GenericMenu();
                            foreach (ShowOnStartupEnum option in Enum.GetValues(typeof(ShowOnStartupEnum))) {
                                menu.AddItem(new GUIContent(Regex.Replace(option.ToString(), "([a-z])([A-Z])", "$1 $2")), false, () => {
                                    if (_showOnStartupOption != option) {
                                        _showOnStartupOption = option;
                                        HotReloadPrefs.ShowOnStartup = _showOnStartupOption;
                                    }
                                });
                            }
                            menu.DropDown(new Rect(buttonRect.x, buttonRect.y, 100, 0));
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
            } finally {
                EditorGUIUtility.labelWidth = prevLabelWidth;
            }
        }

        struct PatchInfo {
            public readonly string patchId;
            public readonly bool apply;
            public readonly string[] methodNames;

            public PatchInfo(MethodPatchResponse response) : this(response.id, apply: true, methodNames: GetMethodNames(response)) { }

            PatchInfo(string patchId, bool apply, string[] methodNames) {
                this.patchId = patchId;
                this.apply = apply;
                this.methodNames = methodNames;
            }


            static string[] GetMethodNames(MethodPatchResponse response) {
                var methodNames = new string[MethodCount(response)];
                var methodIndex = 0;
                for (int i = 0; i < response.patches.Length; i++) {
                    for (int j = 0; j < response.patches[i].modifiedMethods.Length; j++) {
                        var method = response.patches[i].modifiedMethods[j];
                        var displayName = method.displayName;

                        var spaceIndex = displayName.IndexOf(" ", StringComparison.Ordinal);
                        if (spaceIndex > 0) {
                            displayName = displayName.Substring(spaceIndex);
                        }

                        methodNames[methodIndex++] = displayName;
                    }
                }
                return methodNames;
            }

            static int MethodCount(MethodPatchResponse response) {
                var count = 0;
                for (int i = 0; i < response.patches.Length; i++) {
                    count += response.patches[i].modifiedMethods.Length;
                }
                return count;
            }
        }
    }
}