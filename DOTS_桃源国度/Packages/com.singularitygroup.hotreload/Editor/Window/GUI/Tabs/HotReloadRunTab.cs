using System;
using System.Collections.Generic;
using System.IO;
using SingularityGroup.HotReload.DTO;
using SingularityGroup.HotReload.EditorDependencies;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;
using Task = System.Threading.Tasks.Task;
#if UNITY_2019_4_OR_NEWER
using Unity.CodeEditor;
#endif

namespace SingularityGroup.HotReload.Editor {
    internal struct HotReloadRunTabState {
        public readonly bool spinnerActive;
        public readonly string indicationIconPath;
        public readonly bool requestingDownloadAndRun;
        public readonly bool starting;
        public readonly bool stopping;
        public readonly bool running;
        public readonly Tuple<float, string> startupProgress;
        public readonly string indicationStatusText;
        public readonly IReadOnlyList<string> failures;
        public readonly LoginStatusResponse loginStatus;
        public readonly bool downloadRequired;
        public readonly bool downloadStarted;
        public readonly bool requestingLoginInfo;
        public readonly RedeemStage redeemStage;
        
        public HotReloadRunTabState(
            bool spinnerActive, 
            string indicationIconPath,
            bool requestingDownloadAndRun,
            bool starting,
            bool stopping,
            bool running,
            Tuple<float, string> startupProgress,
            string indicationStatusText,
            IReadOnlyList<string> failures,
            LoginStatusResponse loginStatus,
            bool downloadRequired,
            bool downloadStarted,
            bool requestingLoginInfo,
            RedeemStage redeemStage
        ) {
            this.spinnerActive = spinnerActive;
            this.indicationIconPath = indicationIconPath;
            this.requestingDownloadAndRun = requestingDownloadAndRun;
            this.starting = starting;
            this.stopping = stopping;
            this.running = running;
            this.startupProgress = startupProgress;
            this.indicationStatusText = indicationStatusText;
            this.failures = failures;
            this.loginStatus = loginStatus;
            this.downloadRequired = downloadRequired;
            this.downloadStarted = downloadStarted;
            this.requestingLoginInfo = requestingLoginInfo;
            this.redeemStage = redeemStage;
        }

        public static HotReloadRunTabState Current => new HotReloadRunTabState(
            spinnerActive: EditorIndicationState.SpinnerActive,
            indicationIconPath: EditorIndicationState.IndicationIconPath,
            requestingDownloadAndRun: EditorCodePatcher.RequestingDownloadAndRun,
            starting: EditorCodePatcher.Starting,
            stopping: EditorCodePatcher.Stopping,
            running: EditorCodePatcher.Running,
            startupProgress: EditorCodePatcher.StartupProgress,
            indicationStatusText: EditorIndicationState.IndicationStatusText,
            failures: EditorCodePatcher.Failures,
            loginStatus: EditorCodePatcher.Status,
            downloadRequired: EditorCodePatcher.DownloadRequired,
            downloadStarted: EditorCodePatcher.DownloadStarted,
            requestingLoginInfo: EditorCodePatcher.RequestingLoginInfo,
            redeemStage: RedeemLicenseHelper.I.RedeemStage
        );
    }

    internal struct LicenseErrorData {
        public readonly string description;
        public bool showBuyButton;
        public string buyButtonText;
        public readonly bool showLoginButton;
        public readonly string loginButtonText;
        public readonly bool showSupportButton;
        public readonly string supportButtonText;
        public readonly bool showManageLicenseButton;
        public readonly string manageLicenseButtonText;

        public LicenseErrorData(string description, bool showManageLicenseButton = false, string manageLicenseButtonText = "", string loginButtonText = "", bool showSupportButton = false, string supportButtonText = "", bool showBuyButton = false, string buyButtonText = "", bool showLoginButton = false) {
            this.description = description;
            this.showManageLicenseButton = showManageLicenseButton;
            this.manageLicenseButtonText = manageLicenseButtonText;
            this.loginButtonText = loginButtonText;
            this.showSupportButton = showSupportButton;
            this.supportButtonText = supportButtonText;
            this.showBuyButton = showBuyButton;
            this.buyButtonText = buyButtonText;
            this.showLoginButton = showLoginButton;
        }
    }
    
    internal class HotReloadRunTab : HotReloadTabBase {
        private string _pendingEmail;
        private string _pendingPassword;
        private string _pendingPromoCode;
        
        private bool _requestingFlushErrors;
        private bool _requestingActivatePromoCode;
        
        private long _lastErrorFlush;

        private Tuple<string, MessageType> _activateInfoMessage;
        
        // Has Indie or Pro license (even if not currenctly active)
        public bool HasPayedLicense => currentState.loginStatus != null && (currentState.loginStatus.isIndieLicense || currentState.loginStatus.isBusinessLicense);
        public bool TrialLicense => currentState.loginStatus != null && (currentState.loginStatus?.isTrial == true);
        
        private Vector2 _patchedMethodsScrollPos;
        private Vector2 _runTabScrollPos;

        private string promoCodeError;
        private MessageType promoCodeErrorType;
        private bool promoCodeActivatedThisSession;
        
        public HotReloadRunTab(HotReloadWindow window) : base(window, "Run", "forward", "Run and monitor the current Hot Reload session.") { }

        HotReloadRunTabState currentState;
        public override void OnGUI() {
            // HotReloadRunTabState ensures rendering is consistent between Layout and Repaint calls
            // Without it errors like this happen:
            // ArgumentException: Getting control 2's position in a group with only 2 controls when doing repaint
            // See thread for more context: https://answers.unity.com/questions/17718/argumentexception-getting-control-2s-position-in-a.html
            if (Event.current.type == EventType.Layout) {
                currentState = HotReloadRunTabState.Current;
            }
            EditorGUILayout.Space();
            using(new EditorGUILayout.VerticalScope()) {
                OnGUICore();
            }
        }
        
        private bool ShouldRenderConsumption => (currentState.running && !currentState.starting && !currentState.stopping && currentState.loginStatus?.isLicensed != true && currentState.loginStatus?.isFree != true && !EditorCodePatcher.LoginNotRequired) && !(currentState.loginStatus == null || currentState.loginStatus.isFree);
        private bool ShouldRenderUnsupportedChanges => currentState.running && !currentState.starting && currentState.failures.Count > 0;
        
        void OnGUICore() {
            using (var scope = new EditorGUILayout.ScrollViewScope(_runTabScrollPos, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUILayout.MaxHeight(Math.Max(Screen.height, 800)), GUILayout.MaxWidth(Math.Max(Screen.width, 800)))) {
                _runTabScrollPos.x = scope.scrollPosition.x;
                _runTabScrollPos.y = scope.scrollPosition.y;

                var isIndie = RedeemLicenseHelper.I.RegistrationOutcome == RegistrationOutcome.Indie
                    || EditorCodePatcher.licenseType == UnityLicenseType.UnityPersonalPlus;
                
                if (RedeemLicenseHelper.I.RegistrationOutcome == RegistrationOutcome.Business
                    && currentState.loginStatus?.isBusinessLicense != true
                    && (PackageConst.IsAssetStoreBuild || HotReloadPrefs.RateAppShown)
                ) {
                    // Warn asset store users they need to buy a business license
                    // Website users get reminded after using Hot Reload for 5+ days
                    RenderBusinessLicenseInfo();
                } else if (isIndie
                    && HotReloadPrefs.RateAppShown
                    && !PackageConst.IsAssetStoreBuild
                    && currentState.loginStatus?.isBusinessLicense != true
                    && currentState.loginStatus?.isIndieLicense != true
                ) {
                    // Reminder users they need to buy an indie license
                    RenderIndieLicenseInfo();
                }
                
                RenderIndicationPanel();
                if (ShouldRenderUnsupportedChanges) {
                    RenderUnsupportedChanges();
                }
            }

            // At the end to not fuck up rendering https://answers.unity.com/questions/400454/argumentexception-getting-control-0s-position-in-a-1.html
            var renderStart = !EditorCodePatcher.Running && !EditorCodePatcher.Starting && !currentState.requestingDownloadAndRun && currentState.redeemStage == RedeemStage.None;
            var e = Event.current;
            if (renderStart && e.type == EventType.KeyUp
                && (e.keyCode == KeyCode.Return
                    || e.keyCode == KeyCode.KeypadEnter)
            ) {
                EditorCodePatcher.DownloadAndRun().Forget();
            }
        }
        
        internal void RenderConsumption(LoginStatusResponse loginStatus) {
            if (loginStatus == null) {
                return;
            }
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField($"Hot Reload Limited", HotReloadWindowStyles.H3CenteredTitleStyle);
            EditorGUILayout.Space();
            if (loginStatus.consumptionsUnavailableReason == ConsumptionsUnavailableReason.NetworkUnreachable) {
                EditorGUILayout.HelpBox("Something went wrong. Please check your internet connection.", MessageType.Warning);
            } else if (loginStatus.consumptionsUnavailableReason == ConsumptionsUnavailableReason.UnrecoverableError) {
                EditorGUILayout.HelpBox("Something went wrong. Please contact support if the issue persists.", MessageType.Error);
            } else if (loginStatus.freeSessionFinished) {
                var now = DateTime.UtcNow;
                var sessionRefreshesAt = (now.AddDays(1).Date - now).Add(TimeSpan.FromMinutes(5));
                var sessionRefreshString = $"Next Session: {(sessionRefreshesAt.Hours > 0 ? $"{sessionRefreshesAt.Hours}h " : "")}{sessionRefreshesAt.Minutes}min";
                HotReloadGUIHelper.HelpBox(sessionRefreshString, MessageType.Warning, fontSize: 11);
            } else if (loginStatus.freeSessionRunning && loginStatus.freeSessionEndTime != null) {
                var sessionEndsAt = loginStatus.freeSessionEndTime.Value - DateTime.Now;
                var sessionString = $"Daily Session: {(sessionEndsAt.Hours > 0 ? $"{sessionEndsAt.Hours}h " : "")}{sessionEndsAt.Minutes}min Left";
                HotReloadGUIHelper.HelpBox(sessionString, MessageType.Info, fontSize: 11);
            } else if (loginStatus.freeSessionEndTime == null) {
                HotReloadGUIHelper.HelpBox("Daily Session: Make code changes to start", MessageType.Info, fontSize: 11);
            }
        }

        bool _repaint;
        bool _instantRepaint;
        DateTime _lastRepaint;
        private EditorIndicationState.IndicationStatus _lastStatus;
        public override void Update() {
            EditorCodePatcher.RequestServerInfo();
            if (!_requestingFlushErrors && EditorCodePatcher.Running) {
                RequestFlushErrors().Forget();
            }
            if (EditorIndicationState.SpinnerActive) {
                _repaint = true;
            }
            if (EditorCodePatcher.DownloadRequired) {
                _repaint = true;
            }
            if (EditorIndicationState.IndicationIconPath == Spinner.SpinnerIconPath) {
                _repaint = true;
            }
            if (_repaint && DateTime.UtcNow - _lastRepaint > TimeSpan.FromMilliseconds(33)) {
                _repaint = false;
                _instantRepaint = true;
            }
            // repaint on status change
            var status = EditorIndicationState.CurrentIndicationStatus;
            if (_lastStatus != status) {
                _lastStatus = status;
                _instantRepaint = true;
            }
            if (_instantRepaint) {
                Repaint();
                _instantRepaint = false;
                _repaint = false;
                _lastRepaint = DateTime.UtcNow;
            }
        }
        
        public void RepaintInstant() {
            _instantRepaint = true;
        }
        
        private void RenderIndicationButtons() {
            using (new EditorGUILayout.HorizontalScope()) {
                if (currentState.requestingDownloadAndRun || currentState.starting || currentState.stopping) {
                    RenderProgressBar();
                } else if (!currentState.running && (currentState.startupProgress?.Item1 ?? 0) == 0) {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent(" Start", EditorGUIUtility.IconContent("PlayButton@2x").image), HotReloadWindowStyles.StartButton)) {
                        EditorCodePatcher.DownloadAndRun().Forget();
                    }
                    GUILayout.FlexibleSpace();
                } else if (currentState.running && !currentState.starting) {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent(" Stop", EditorGUIUtility.IconContent("animationdopesheetkeyframe").image), HotReloadWindowStyles.StartButton)) {
                        if (!EditorCodePatcher.StoppedServerRecently()) {
                            EditorCodePatcher.StopCodePatcher().Forget();
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
            }
        }

        void RenderIndicationPanel() {
            using (new EditorGUILayout.HorizontalScope(HotReloadWindowStyles.SectionOuterBox)) {
                using (new EditorGUILayout.HorizontalScope(HotReloadWindowStyles.SectionInnerBox)) {
                    using (new EditorGUILayout.VerticalScope()) {
                        RenderIndication();

                        if (currentState.redeemStage != RedeemStage.None) {
                            RedeemLicenseHelper.I.RenderStage(currentState.redeemStage);
                        } else {
                            RenderIndicationButtons();
                    
                            if (ShouldRenderConsumption) {
                                RenderConsumption(currentState.loginStatus);
                                RenderLicenseInfo(currentState.loginStatus);
                                RenderLicenseButtons();
                            }
                        }
                    }
                } 
            } 
        }
        
        private Spinner _spinner = new Spinner(85);
        private void RenderIndication() {
            using (new EditorGUILayout.HorizontalScope(HotReloadWindowStyles.IndicationBox)) {
                // icon box
                using (new EditorGUILayout.HorizontalScope(HotReloadWindowStyles.IndicationIconBox)) {
                    if (currentState.indicationIconPath == Spinner.SpinnerIconPath) {
                        GUILayout.Label(image: _spinner.GetIcon(), style: HotReloadWindowStyles.SpinnerIcon);
                    } else if (currentState.indicationIconPath != null) {
                        GUILayout.Label(Resources.Load<Texture2D>(currentState.indicationIconPath), HotReloadWindowStyles.IndicationIcon);
                    }
                } 
                // text box
                using (new EditorGUILayout.HorizontalScope(HotReloadWindowStyles.IndicationTextBox)) {
                    GUILayout.Label(currentState.indicationStatusText, HotReloadWindowStyles.H1TitleCenteredStyle);
                }
            }
        }
        
        private GUIStyle _unsupportedChangesInnerBoxMinStyle;
        private GUIStyle UnsupportedChangesInnerBoxMin {
            get {
                const int minimumHeight = 63;
                const int minimumExpandedLogHeight = 60;
                const int minimumCollapsedHeight = 55;
                if (_unsupportedChangesInnerBoxMinStyle == null) {
                    _unsupportedChangesInnerBoxMinStyle = new GUIStyle(HotReloadWindowStyles.UnsupportedChangesInnerBox);
                }
                _unsupportedChangesInnerBoxMinStyle.margin.left = HotReloadWindowStyles.UnsupportedChangesInnerBox.margin.left;
                var height = minimumHeight + (HotReloadWindowStyles.LogStyle.fixedHeight * EditorCodePatcher.Failures.Count) + (minimumExpandedLogHeight * _expandedLogs.Count);
                if (HotReloadPrefs.ShowUnsupportedChanges) {
                    _unsupportedChangesInnerBoxMinStyle.fixedHeight = Math.Min(height, 400);
                } else {
                    _unsupportedChangesInnerBoxMinStyle.fixedHeight = minimumCollapsedHeight;
                }
                return _unsupportedChangesInnerBoxMinStyle;
            }
        }

        private string _warningIconPath = "warning";
        private void RenderUnsupportedChanges() {
            using (new EditorGUILayout.HorizontalScope(HotReloadWindowStyles.SectionOuterBox)) {
                using (new EditorGUILayout.HorizontalScope(UnsupportedChangesInnerBoxMin)) {
                    using (new EditorGUILayout.VerticalScope()) { 
                        
                        // header
                        using (new EditorGUILayout.HorizontalScope(HotReloadWindowStyles.UnsupportedChangesHeader)) {
                            HotReloadPrefs.ShowUnsupportedChanges = EditorGUILayout.Foldout(HotReloadPrefs.ShowUnsupportedChanges, "", true, HotReloadWindowStyles.FoldoutStyle);
                            GUILayout.Label(Resources.Load<Texture2D>(_warningIconPath), HotReloadWindowStyles.UnsupportedChangesIcon);
                            GUILayout.Space(-35);
                            if (GUILayout.Button("Unsupported changes detected!", HotReloadWindowStyles.UnsupportedChangesText)) {
                                HotReloadPrefs.ShowUnsupportedChanges = !HotReloadPrefs.ShowUnsupportedChanges;
                            }
                            GUILayout.Space(5);
                            if (GUILayout.Button(EditorApplication.isPlaying ? "Force Recompile" : "Recompile", HotReloadWindowStyles.UnsupportedChangesButton)) {
                                EditorApplication.isPlaying = false;
                                AssetDatabase.Refresh();
                            }
                            GUILayout.FlexibleSpace();
                        }
                        
                        // unsupported changes log list
                        if (HotReloadPrefs.ShowUnsupportedChanges) {
                            RenderUnsupportedChangesList();
                        }
                        
                    }
                }
            }
        }

        private string _closeIconPath = "close";
        private string[] supportedPaths = new[] { Path.GetFullPath("Assets"), Path.GetFullPath("Plugins") };
        private List<string> _expandedLogs = new List<string>();
        private void RenderUnsupportedChangesList() {
            using (var scope = new EditorGUILayout.ScrollViewScope(_patchedMethodsScrollPos, GUIStyle.none, GUI.skin.verticalScrollbar)) {
                _patchedMethodsScrollPos.y = scope.scrollPosition.y;
 
                GUIStyle logStyle;
                for (var i = currentState.failures.Count - 1; i >= 0; i--) {
                    var failure = currentState.failures[i];
                    if (string.IsNullOrEmpty(failure)) {
                        continue;
                    }
                    // Alternate log background color between light and dark
                    if (i % 2 == 1) {
                        logStyle = HotReloadWindowStyles.LogStyleLight;
                    } else {
                        logStyle = HotReloadWindowStyles.LogStyleDark;
                    }
                    // Get the relevant file name
                    string fileName = null;
                    try {
                        int csIndex = 0;
                        int attempt = 0;
                        do {
                            csIndex = failure.IndexOf(".cs", csIndex + 1, StringComparison.Ordinal);
                            if (csIndex == -1) {
                                break;
                            }
                            int fileNameStartIndex = csIndex - 1;
                            for (; fileNameStartIndex >= 0; fileNameStartIndex--) {
                                if (!char.IsLetter(failure[fileNameStartIndex])) {
                                    fileName = failure.Substring(fileNameStartIndex, csIndex - fileNameStartIndex + ".cs".Length);
                                    break;
                                }
                            }
                        } while (attempt++ < 100 && fileName == null);
                    } catch {
                        // ignore
                    }
                    fileName = fileName ?? "Tap to show stacktrace";
                    
                    // Get the error
                    string error;
                    int endOfError = failure.IndexOf(". in ", StringComparison.Ordinal);
                    string specialChars = "\"'/\\";
                    char[] characters = specialChars.ToCharArray();
                    int specialChar = failure.IndexOfAny(characters);
                    try {
                        if (failure.StartsWith("errors:", StringComparison.Ordinal) && endOfError > 0) {
                            error = failure.Substring("errors: ".Length, endOfError - "errors: ".Length).Trim();
                        } else if (failure.StartsWith("errors:", StringComparison.Ordinal) && specialChar > 0) {
                            error = failure.Substring("errors: ".Length, specialChar - "errors: ".Length).Trim();
                        } else {
                            error = "Unsupported change deteced, tap here to see more.";
                        }
                    } catch {
                        error = "Unsupported change deteced, tap here to see more.";
                    }
                    
                    // Get relative path
                    TextAsset file = null;
                    foreach (var path in supportedPaths) {
                        int lastprojectIndex = 0;
                        int attempt = 0;
                        while (attempt++ < 100 && !file) {
                            lastprojectIndex = failure.IndexOf(path, lastprojectIndex + 1, StringComparison.Ordinal);
                            if (lastprojectIndex == -1) {
                                break;
                            }
                            var fullCsIndex = failure.IndexOf(".cs", lastprojectIndex, StringComparison.Ordinal);
                            var candidateAbsolutePath = failure.Substring(lastprojectIndex, fullCsIndex - lastprojectIndex + ".cs".Length);
                            var candidateRelativePath = EditorCodePatcher.GetRelativePath(filespec: candidateAbsolutePath, folder: path);
                            file = AssetDatabase.LoadAssetAtPath<TextAsset>(candidateRelativePath);
                        }
                    }
                    
                    // Get the line number
                    int lineNumber = 0;
                    try {
                        int lastIndex = 0;
                        int attempt = 0;
                        do {
                            lastIndex = failure.IndexOf(fileName, lastIndex + 1, StringComparison.Ordinal);
                            if (lastIndex == -1) {
                                break;
                            }
                            var part = failure.Substring(lastIndex + fileName.Length);
                            if (!part.StartsWith(":", StringComparison.Ordinal) 
                                || part.Length == 1 
                                || !char.IsDigit(part[1])
                            ) {
                                continue;
                            }
                            int y = 1;
                            for (; y < part.Length; y++) {
                                if (!char.IsDigit(part[y])) {
                                    break;
                                }
                            }
                            if (int.TryParse(part.Substring(1, y), out lineNumber)) {
                                break;
                            }
                        } while (attempt++ < 100);
                    } catch { 
                        //ignore
                    }
                    
                    // make sure there is no overflow
                    const int filenameUpperLimit = 27;
                    const int errorLowerLimit = 115;
                    if (fileName.Length >= filenameUpperLimit) {
                        fileName = fileName.Substring(0, filenameUpperLimit) + "...";
                        lineNumber = 0;
                        if (error.Length >= errorLowerLimit) {
                            error = error.Substring(0, errorLowerLimit) + "...";
                        }
                    } else {
                        var errorLimit = errorLowerLimit + (filenameUpperLimit - fileName.Length);
                        if (error.Length >= errorLimit) {
                            error = error.Substring(0, errorLimit) + "...";
                        }
                    }
                    
                    // log entry
                    using (new EditorGUILayout.HorizontalScope(logStyle)) {
                        
                        // Error text
                        GUILayout.Space(10);
                        if (GUILayout.Button(error, HotReloadWindowStyles.LabelStyle)) {
                            if (!_expandedLogs.Contains(failure)) {
                                _expandedLogs.Add(failure);
                            } else {
                                _expandedLogs.Remove(failure);
                            }
                        }
                        GUILayout.FlexibleSpace();
                        
                        // Link
                        if (GUILayout.Button(lineNumber > 0 ? fileName + ":" + lineNumber : fileName, HotReloadWindowStyles.LinkStyle)) {
                            if (file) {
                                AssetDatabase.OpenAsset(file, Math.Max(lineNumber, 1));
                            } else {
                                if (!_expandedLogs.Contains(failure)) {
                                    _expandedLogs.Add(failure);
                                } else {
                                    _expandedLogs.Remove(failure);
                                }
                            } 
                        }
                        
                        // remove button
                        if (GUILayout.Button(Resources.Load<Texture2D>(_closeIconPath), HotReloadWindowStyles.RemoveUnsupportedChangeIcon)) {
                            var newFailures = new List<string>(EditorCodePatcher.Failures);
                            newFailures.RemoveAt(i);
                            EditorCodePatcher.Failures = newFailures;
                        }
                        GUILayout.Space(30);
                        
                    }
                    
                    // stacktrace if should show
                    if (_expandedLogs.Contains(failure)) {
                        using (new EditorGUILayout.VerticalScope(HotReloadWindowStyles.DropdownAreaStyle)) {
                            GUILayout.TextArea(failure, HotReloadWindowStyles.DropdownBox);
                        }
                    }
                }
            }
        }
        
        private async Task RequestFlushErrors() {
            _requestingFlushErrors = true;
            try {
                await RequestFlushErrorsCore();
            } finally {
                _requestingFlushErrors = false;
            }
        }
        
        private async Task RequestFlushErrorsCore() {
            var pollFrequency = 500;
            // Delay until we've hit the poll request frequency
            var waitMs = (int)Mathf.Clamp(pollFrequency - ((DateTime.Now.Ticks / (float)TimeSpan.TicksPerMillisecond) - _lastErrorFlush), 0, pollFrequency);
            await Task.Delay(waitMs);
            await FlushErrors();
            _lastErrorFlush = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }
        
        static async Task FlushErrors() {
            var response = await RequestHelper.RequestFlushErrors();
            if (response == null) {
                return;
            }
            foreach (var responseWarning in response.warnings) {
                Log.Warning(responseWarning);
            }
            foreach (var responseError in response.errors) {
                Log.Error(responseError);
            }
        }

        static GUIStyle _openSettingsStyle;
        static GUIStyle openSettingsStyle => _openSettingsStyle ?? (_openSettingsStyle = new GUIStyle(GUI.skin.button) {
            fontStyle = FontStyle.Normal,
            fixedHeight = 25,
        });
        
        static GUILayoutOption[] _bigButtonHeight;
        public static GUILayoutOption[] bigButtonHeight => _bigButtonHeight ?? (_bigButtonHeight = new [] {GUILayout.Height(25)});
        
        private static GUIContent indieLicenseContent;
        private static GUIContent businessLicenseContent;

        float lastRectHeigh = 38;
        internal void RenderLicenseStatusInfo(LoginStatusResponse loginStatus, bool allowHide = true, bool verbose = false) {
            string message = null;
            MessageType messageType = default(MessageType);
            Action customGUI = null;
            GUIContent content = null;
            if (loginStatus == null) {
                // no info
            } else if (loginStatus.lastLicenseError != null) {
                messageType = !loginStatus.freeSessionFinished ? MessageType.Warning : MessageType.Error;
                message = GetMessageFromError(loginStatus.lastLicenseError);
            } else if (loginStatus.isTrial && !PackageConst.IsAssetStoreBuild) {
                message = $"Using Trial license, valid until {loginStatus.licenseExpiresAt.ToShortDateString()}";
                messageType = MessageType.Info;
            } else if (loginStatus.isIndieLicense) {
                if (verbose) {
                    message = " Indie license active";
                    messageType = MessageType.Info;
                    if (loginStatus.licenseExpiresAt.Date != DateTime.MaxValue.Date) {
                        customGUI = () => {
                            EditorGUILayout.LabelField($"License will renew on {loginStatus.licenseExpiresAt.ToShortDateString()}.");
                            EditorGUILayout.Space();
                        };
                    }
                    if (indieLicenseContent == null) {
                        indieLicenseContent = new GUIContent(message, EditorGUIUtility.FindTexture("TestPassed"));
                    }
                    content = indieLicenseContent;
                }
            } else if (loginStatus.isBusinessLicense) {
                if (verbose) {
                    message = " Business license active";
                    messageType = MessageType.Info;
                    if (businessLicenseContent == null) {
                        businessLicenseContent = new GUIContent(message, EditorGUIUtility.FindTexture("TestPassed"));
                    }
                    content = businessLicenseContent;
                }
            }

            if (messageType != MessageType.Info && HotReloadPrefs.ErrorHidden && allowHide) {
                return;
            }
            if (message != null) {
                if (messageType != MessageType.Info) {
                    using(new EditorGUILayout.HorizontalScope()) {
                        EditorGUILayout.HelpBox(message, messageType);
                        // lastRectHeigh is not accurate during Layout event
                        if (Event.current.type == EventType.Repaint) {
                            lastRectHeigh = GUILayoutUtility.GetLastRect().height;
                        }
                        if (allowHide) {
                            if (GUILayout.Button("Hide", GUILayout.ExpandHeight(true), GUILayout.MaxHeight(lastRectHeigh))) {
                                HotReloadPrefs.ErrorHidden = true;
                            }
                        }
                    }
                } else if (content != null) {
                    EditorGUILayout.LabelField(content);
                    EditorGUILayout.Space();
                } else {
                    EditorGUILayout.LabelField(message);
                    EditorGUILayout.Space();
                }
                customGUI?.Invoke();
            }
        }

        float lastInfoRectHeigh;
        const string assetStoreProInfo = "Unity Pro/Enterprise users from company with your number of employees require a Business license. Please upgrade your license on our website.";
        public void RenderBusinessLicenseInfo() {
            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.HelpBox(assetStoreProInfo, MessageType.Info);
                // lastRectHeigh is not accurate during Layout event
                if (Event.current.type == EventType.Repaint) {
                    lastInfoRectHeigh = GUILayoutUtility.GetLastRect().height;
                }
                if (GUILayout.Button(" Upgrade ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxHeight(lastInfoRectHeigh))) {
                    Application.OpenURL(Constants.ProductPurchaseBusinessURL);
                }
            }
        }
        
        public void RenderIndieLicenseInfo() {
            string message;
            if (EditorCodePatcher.licenseType == UnityLicenseType.UnityPersonalPlus) {
                message = "Unity Plus users require an Indie license. Please upgrade your license on our website.";
            } else if (EditorCodePatcher.licenseType == UnityLicenseType.UnityPro) {
                message = "Unity Pro/Enterprise users from company with your number of employees require an Indie license. Please upgrade your license on our website.";
            } else {
                return;
            }
            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.HelpBox(message, MessageType.Info);
                // lastRectHeigh is not accurate during Layout event
                if (Event.current.type == EventType.Repaint) {
                    lastInfoRectHeigh = GUILayoutUtility.GetLastRect().height;
                }
                if (GUILayout.Button(" Upgrade ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.MaxHeight(lastInfoRectHeigh))) {
                    Application.OpenURL(Constants.ProductPurchaseURL);
                }
            }
        }

        const string GetLicense = "Get License";
        const string ContactSupport = "Contact Support";
        const string UpgradeLicense = "Upgrade License";
        const string ManageLicense = "Manage License";
        internal Dictionary<string, LicenseErrorData> _licenseErrorData;
        internal Dictionary<string, LicenseErrorData> LicenseErrorData => _licenseErrorData ?? (_licenseErrorData = new Dictionary<string, LicenseErrorData> {
            { "DeviceNotLicensedException", new LicenseErrorData(description: "Another device is using your license. Please reach out to customer support for assistance.", showSupportButton: true, supportButtonText: ContactSupport) },
            { "DeviceBlacklistedException", new LicenseErrorData(description: "You device has been blacklisted.") },
            { "DateHeaderInvalidException", new LicenseErrorData(description: $"Your license is not working because your computer's clock is incorrect. Please set the clock to the correct time to restore your license.") },
            { "DateTimeCheatingException", new LicenseErrorData(description: $"Your license is not working because your computer's clock is incorrect. Please set the clock to the correct time to restore your license.") },
            { "LicenseActivationException", new LicenseErrorData(description: "An error has occured while activating your license. Please contact customer support for assistance.", showSupportButton: true, supportButtonText: ContactSupport) },
            { "LicenseDeletedException", new LicenseErrorData(description: $"Your license has been deleted. Please contact customer support for assistance.", showBuyButton: true, buyButtonText: GetLicense, showSupportButton: true, supportButtonText: ContactSupport) },
            { "LicenseDisabledException", new LicenseErrorData(description: $"Your license has been disabled. Please contact customer support for assistance.", showBuyButton: true, buyButtonText: GetLicense, showSupportButton: true, supportButtonText: ContactSupport) },
            { "LicenseExpiredException", new LicenseErrorData(description: $"Your license has expired. Please renew your license subscription using the 'Upgrade License' button below and login with your email/password to activate your license.", showBuyButton: true, buyButtonText: UpgradeLicense, showManageLicenseButton: true, manageLicenseButtonText: ManageLicense) },
            { "LicenseInactiveException", new LicenseErrorData(description: $"Your license is currenty inactive. Please login with your email/password to activate your license.") },
            { "LocalLicenseException", new LicenseErrorData(description: $"Your license file was damaged or corrupted. Please login with your email/password to refresh your license file.") },
            // Note: obsolete
            { "MissingParametersException", new LicenseErrorData(description: "An account already exists for this device. Please login with your existing email/password.", showBuyButton: true, buyButtonText: GetLicense) },
            { "NetworkException", new LicenseErrorData(description: "There is an issue connecting to our servers. Please check your internet connection or contact customer support if the issue persists.", showSupportButton: true, supportButtonText: ContactSupport) },
            { "TrialLicenseExpiredException", new LicenseErrorData(description: $"Your trial has expired. Activate a license with unlimited usage or continue using the Free version. View available plans on our website.", showBuyButton: true, buyButtonText: UpgradeLicense) },
            { "InvalidCredentialException", new LicenseErrorData(description: "Incorrect email/password. You can find your initial password in the sign-up email.") },
            // Note: activating free trial with email is not supported anymore. This error shouldn't happen which is why we should rather user the fallback
            // { "LicenseNotFoundException", new LicenseErrorData(description: "The account you're trying to access doesn't seem to exist yet. Please enter your email address to create a new account and receive a trial license.", showLoginButton: true, loginButtonText: CreateAccount) },
            { "LicenseIncompatibleException", new LicenseErrorData(description: "Please upgrade your license to continue using hotreload with Unity Pro.", showManageLicenseButton: true, manageLicenseButtonText: ManageLicense) },
        });
        internal LicenseErrorData defaultLicenseErrorData = new LicenseErrorData(description: "We apologize, an error happened while verifying your license. Please reach out to customer support for assistance.", showSupportButton: true, supportButtonText: ContactSupport);

        internal string GetMessageFromError(string error) {
            if (PackageConst.IsAssetStoreBuild && error == "TrialLicenseExpiredException") {
                return assetStoreProInfo;
            }
            return GetLicenseErrorDataOrDefault(error).description;
        }
        
        internal LicenseErrorData GetLicenseErrorDataOrDefault(string error) {
            if (currentState.loginStatus?.isFree == true) {
                return default(LicenseErrorData);
            }
            if (currentState.loginStatus == null || string.IsNullOrEmpty(error) && (!currentState.loginStatus.isLicensed || currentState.loginStatus.isTrial)) {
                return new LicenseErrorData(null, showBuyButton: true, buyButtonText: GetLicense);
            }
            if (string.IsNullOrEmpty(error)) {
                return default(LicenseErrorData);
            }
            if (!LicenseErrorData.ContainsKey(error)) {
                return defaultLicenseErrorData;
            }
            return LicenseErrorData[error];
        }

        internal void RenderBuyLicenseButton(string buyLicenseButton) {
            OpenURLButton.Render(buyLicenseButton, Constants.ProductPurchaseURL);
        }

        void RenderLicenseActionButtons(LicenseErrorData errInfo) {
            if (errInfo.showBuyButton || errInfo.showManageLicenseButton) {
                using(new EditorGUILayout.HorizontalScope()) {
                    if (errInfo.showBuyButton) {
                        RenderBuyLicenseButton(errInfo.buyButtonText);
                    }
                    if (errInfo.showManageLicenseButton && !HotReloadPrefs.ErrorHidden) {
                        OpenURLButton.Render(errInfo.manageLicenseButtonText, Constants.ManageLicenseURL);
                    }
                }
            }
            if (errInfo.showLoginButton && GUILayout.Button(errInfo.loginButtonText, openSettingsStyle)) {
                // show license section
                _window.SelectTab(typeof(HotReloadSettingsTab));
                _window.SettingsTab.FocusLicenseFoldout();
            }
            if (errInfo.showSupportButton && !HotReloadPrefs.ErrorHidden) {
                OpenURLButton.Render(errInfo.supportButtonText, Constants.ContactURL);
            }
            if (currentState.loginStatus?.lastLicenseError != null) {
                _window.AboutTab.reportIssueButton.OnGUI();
            }
        }

        internal void RenderLicenseInfo(LoginStatusResponse loginStatus, bool verbose = false, bool allowHide = true, string overrideActionButton = null, bool showConsumptions = false) {
            HotReloadPrefs.ShowLogin = EditorGUILayout.Foldout(HotReloadPrefs.ShowLogin, "Hot Reload License", true, HotReloadWindowStyles.FoldoutStyle);
            if (HotReloadPrefs.ShowLogin) {
                EditorGUILayout.Space();
                if ((loginStatus?.isLicensed != true && showConsumptions) && !(loginStatus == null || loginStatus.isFree)) {
                    RenderConsumption(loginStatus);
                }
                RenderLicenseStatusInfo(loginStatus: loginStatus, allowHide: allowHide, verbose: verbose);

                RenderLicenseInnerPanel(overrideActionButton: overrideActionButton);
                
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
        }

        internal void RenderPromoCodes() {
            HotReloadPrefs.ShowPromoCodes = EditorGUILayout.Foldout(HotReloadPrefs.ShowPromoCodes, "Promo Codes", true, HotReloadWindowStyles.FoldoutStyle);
            if (!HotReloadPrefs.ShowPromoCodes) {
                return;
            }
            if (promoCodeActivatedThisSession) {
                EditorGUILayout.HelpBox($"Your promo code has been successfully activated. Free trial has been extended by 3 months.", MessageType.Info);
            } else {
                if (promoCodeError != null && promoCodeErrorType != MessageType.None) {
                    EditorGUILayout.HelpBox(promoCodeError, promoCodeErrorType);
                }
                EditorGUILayout.LabelField("Promo code");
                _pendingPromoCode = EditorGUILayout.TextField(_pendingPromoCode);
                EditorGUILayout.Space();

                using (new EditorGUI.DisabledScope(_requestingActivatePromoCode)) {
                    if (GUILayout.Button("Activate promo code", HotReloadRunTab.bigButtonHeight)) {
                        RequestActivatePromoCode().Forget();
                    }
                }
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
        
        private async Task RequestActivatePromoCode() {
            _requestingActivatePromoCode = true;
            try {
                var resp = await RequestHelper.RequestActivatePromoCode(_pendingPromoCode);
                if (resp != null && resp.error == null) {
                    promoCodeActivatedThisSession = true;
                } else {
                    var requestError = resp?.error ?? "Network error";
                    var errorType = ToErrorType(requestError);
                    promoCodeError = ToPrettyErrorMessage(errorType);
                    promoCodeErrorType = ToMessageType(errorType);
                }
            } finally {
                _requestingActivatePromoCode = false;
            }
        }

        PromoCodeErrorType ToErrorType(string error) {
            switch (error) {
                case "Input is missing":           return PromoCodeErrorType.MISSING_INPUT;
                case "only POST is supported":     return PromoCodeErrorType.INVALID_HTTP_METHOD;
                case "body is not a valid json":   return PromoCodeErrorType.BODY_INVALID;
                case "Promo code is not found":    return PromoCodeErrorType.PROMO_CODE_NOT_FOUND;
                case "Promo code already claimed": return PromoCodeErrorType.PROMO_CODE_CLAIMED;
                case "Promo code expired":         return PromoCodeErrorType.PROMO_CODE_EXPIRED;
                case "License not found":          return PromoCodeErrorType.LICENSE_NOT_FOUND;
                case "License is not a trial":     return PromoCodeErrorType.LICENSE_NOT_TRIAL;
                case "License already extended":   return PromoCodeErrorType.LICENSE_ALREADY_EXTENDED;
                case "conditionalCheckFailed":     return PromoCodeErrorType.CONDITIONAL_CHECK_FAILED;
            }
            if (error.Contains("Updating License Failed with error")) {
                return PromoCodeErrorType.UPDATING_LICENSE_FAILED;
            } else if (error.Contains("Unknown exception")) {
                return PromoCodeErrorType.UNKNOWN_EXCEPTION;
            } else if (error.Contains("Unsupported path")) {
                return PromoCodeErrorType.UNSUPPORTED_PATH;
            }
            return PromoCodeErrorType.NONE;
        }

        string ToPrettyErrorMessage(PromoCodeErrorType errorType) {
            var defaultMsg = "We apologize, an error happened while activating your promo code. Please reach out to customer support for assistance.";
            switch (errorType) {
                case PromoCodeErrorType.MISSING_INPUT:
                case PromoCodeErrorType.INVALID_HTTP_METHOD:
                case PromoCodeErrorType.BODY_INVALID:
                case PromoCodeErrorType.UNKNOWN_EXCEPTION:
                case PromoCodeErrorType.UNSUPPORTED_PATH:
                case PromoCodeErrorType.LICENSE_NOT_FOUND:
                case PromoCodeErrorType.UPDATING_LICENSE_FAILED:
                case PromoCodeErrorType.LICENSE_NOT_TRIAL:
                    return defaultMsg;
                case PromoCodeErrorType.PROMO_CODE_NOT_FOUND:     return "Your promo code is invalid. Please ensure that you have entered the correct promo code.";
                case PromoCodeErrorType.PROMO_CODE_CLAIMED:       return "Your promo code has already been used.";
                case PromoCodeErrorType.PROMO_CODE_EXPIRED:       return "Your promo code has expired.";
                case PromoCodeErrorType.LICENSE_ALREADY_EXTENDED: return "Your license has already been activated with a promo code. Only one promo code activation per license is allowed.";
                case PromoCodeErrorType.CONDITIONAL_CHECK_FAILED: return "We encountered an error while activating your promo code. Please try again. If the issue persists, please contact our customer support team for assistance.";
                case PromoCodeErrorType.NONE:                     return "There is an issue connecting to our servers. Please check your internet connection or contact customer support if the issue persists.";
                default:                                          return defaultMsg;
            }
        }

        MessageType ToMessageType(PromoCodeErrorType errorType) {
            switch (errorType) {
                case PromoCodeErrorType.MISSING_INPUT:            return MessageType.Error;
                case PromoCodeErrorType.INVALID_HTTP_METHOD:      return MessageType.Error;
                case PromoCodeErrorType.BODY_INVALID:             return MessageType.Error;
                case PromoCodeErrorType.PROMO_CODE_NOT_FOUND:     return MessageType.Warning;
                case PromoCodeErrorType.PROMO_CODE_CLAIMED:       return MessageType.Warning;
                case PromoCodeErrorType.PROMO_CODE_EXPIRED:       return MessageType.Warning;
                case PromoCodeErrorType.LICENSE_NOT_FOUND:        return MessageType.Error;
                case PromoCodeErrorType.LICENSE_NOT_TRIAL:        return MessageType.Error;
                case PromoCodeErrorType.LICENSE_ALREADY_EXTENDED: return MessageType.Warning;
                case PromoCodeErrorType.UPDATING_LICENSE_FAILED:  return MessageType.Error;
                case PromoCodeErrorType.CONDITIONAL_CHECK_FAILED: return MessageType.Error;
                case PromoCodeErrorType.UNKNOWN_EXCEPTION:        return MessageType.Error;
                case PromoCodeErrorType.UNSUPPORTED_PATH:         return MessageType.Error;
                case PromoCodeErrorType.NONE:                     return MessageType.Error;
                default:                                          return MessageType.Error;
            }
        }

        public void RenderLicenseButtons() {
            var errInfo = GetLicenseErrorDataOrDefault(currentState.loginStatus?.lastLicenseError);
            RenderLicenseActionButtons(errInfo);
        }

        internal void RenderLicenseInnerPanel(string overrideActionButton = null, bool renderLogout = true) {
            EditorGUILayout.LabelField("Email");
            GUI.SetNextControlName("email");
            _pendingEmail = EditorGUILayout.TextField(string.IsNullOrEmpty(_pendingEmail) ? HotReloadPrefs.LicenseEmail : _pendingEmail);
            _pendingEmail = _pendingEmail.Trim();

            EditorGUILayout.LabelField("Password");
            GUI.SetNextControlName("password");
            _pendingPassword = EditorGUILayout.PasswordField(string.IsNullOrEmpty(_pendingPassword) ? HotReloadPrefs.LicensePassword : _pendingPassword);
            
            RenderSwitchAuthMode();
            
            var e = Event.current;
            using(new EditorGUI.DisabledScope(currentState.requestingLoginInfo)) {
                var btnLabel = overrideActionButton;
                if (String.IsNullOrEmpty(overrideActionButton)) {
                    btnLabel = "Login";
                }
                using (new EditorGUILayout.HorizontalScope()) {
                    var focusedControl = GUI.GetNameOfFocusedControl();
                    if (GUILayout.Button(btnLabel, bigButtonHeight)
                        || (focusedControl == "email" 
                            || focusedControl == "password") 
                        && e.type == EventType.KeyUp 
                        && (e.keyCode == KeyCode.Return 
                            || e.keyCode == KeyCode.KeypadEnter)
                    ) {
                        var error = ValidateEmail(_pendingEmail);
                        if (!string.IsNullOrEmpty(error)) {
                            _activateInfoMessage = new Tuple<string, MessageType>(error, MessageType.Warning);
                        } else if (string.IsNullOrEmpty(_pendingPassword)) {
                            _activateInfoMessage = new Tuple<string, MessageType>("Please enter your password.", MessageType.Warning);
                        } else {
                            _window.SelectTab(typeof(HotReloadRunTab));

                            _activateInfoMessage = null;
                            if (RedeemLicenseHelper.I.RedeemStage == RedeemStage.Login) {
                                RedeemLicenseHelper.I.FinishRegistration(RegistrationOutcome.Indie);
                            }
                            if (!EditorCodePatcher.RequestingDownloadAndRun && !EditorCodePatcher.Running) {
                                LoginOnDownloadAndRun(new LoginData(email: _pendingEmail, password: _pendingPassword)).Forget();
                            } else {
                                EditorCodePatcher.RequestLogin(_pendingEmail, _pendingPassword).Forget();
                            }
                        }
                    }
                    if (renderLogout) {
                        RenderLogout();
                    }
                }
            }
            if (_activateInfoMessage != null && (e.type == EventType.Layout || e.type == EventType.Repaint)) {
                EditorGUILayout.HelpBox(_activateInfoMessage.Item1, _activateInfoMessage.Item2);
            }
        }

        public static string ValidateEmail(string email) {
            if (string.IsNullOrEmpty(email)) {
                return "Please enter your email address.";
            } else if (!EditorWindowHelper.IsValidEmailAddress(email)) {
                return "Please enter a valid email address.";
            } else if (email.Contains("+")) {
                return "Mail extensions (in a form of 'username+suffix@example.com') are not supported yet. Please provide your original email address (such as 'username@example.com' without '+suffix' part) as we're working on resolving this issue.";
            }
            return null;
        }

        public void RenderLogout() {
            if (currentState.loginStatus?.isLicensed != true) {
                return;
            }
            if (GUILayout.Button("Logout", bigButtonHeight)) {
                _window.SelectTab(typeof(HotReloadRunTab));
                if (!EditorCodePatcher.RequestingDownloadAndRun && !EditorCodePatcher.Running) {
                    LogoutOnDownloadAndRun().Forget();
                } else {
                    RequestLogout().Forget();
                }
            }
        }
        
        async Task LoginOnDownloadAndRun(LoginData loginData = null) {
            var ok = await EditorCodePatcher.DownloadAndRun(loginData);
            if (ok && loginData != null) {
                HotReloadPrefs.ErrorHidden = false;
                HotReloadPrefs.LicenseEmail = loginData.email;
                HotReloadPrefs.LicensePassword = loginData.password;
            }
        }

        async Task LogoutOnDownloadAndRun() {
            var ok = await EditorCodePatcher.DownloadAndRun();
            if (!ok) {
                return;
            }
            await RequestLogout();
        }

        private async Task RequestLogout() {
            int i = 0;
            while (!EditorCodePatcher.Running && i < 100) {
                await Task.Delay(100);
                i++;
            }
            var resp = await RequestHelper.RequestLogout();
            if (!EditorCodePatcher.RequestingLoginInfo && resp != null) {
                EditorCodePatcher.HandleStatus(resp);
            }
        }

        private static void RenderSwitchAuthMode() {
            var color = EditorGUIUtility.isProSkin ? new Color32(0x3F, 0x9F, 0xFF, 0xFF) : new Color32(0x0F, 0x52, 0xD7, 0xFF); 
            if (HotReloadGUIHelper.LinkLabel("Forgot password?", 12, FontStyle.Normal, TextAnchor.MiddleLeft, color)) {
                if (EditorUtility.DisplayDialog("Recover password", "Use company code 'naughtycult' and the email you signed up with in order to recover your account.", "Open in browser", "Cancel")) {
                    Application.OpenURL(Constants.ForgotPasswordURL);
                }
            }
        }
        
        Texture2D _greenTextureLight;
        Texture2D _greenTextureDark;
        Texture2D GreenTexture => EditorGUIUtility.isProSkin 
            ? _greenTextureDark ? _greenTextureDark : (_greenTextureDark = MakeTexture(0.5f))
            : _greenTextureLight ? _greenTextureLight : (_greenTextureLight = MakeTexture(0.85f));
        
        private void RenderProgressBar() {
            if (currentState.downloadRequired && !currentState.downloadStarted) {
                return;
            }
            
            GUILayout.Label("", HotReloadWindowStyles.ProgressBarAnchorStyle);
            var progressBarAnchorRect = GUILayoutUtility.GetLastRect();
            
            using(var scope = new EditorGUILayout.VerticalScope(HotReloadWindowStyles.MiddleCenterStyle)) {
                float progress;
                var bg = HotReloadWindowStyles.ProgressBarBarStyle.normal.background;
                try {
                    HotReloadWindowStyles.ProgressBarBarStyle.normal.background = GreenTexture;
                    var barRect = scope.rect;
                    
                    barRect.x = progressBarAnchorRect.x + HotReloadWindowStyles.IndicationBox.margin.left + 3;
                    barRect.height = HotReloadWindowStyles.StartButton.fixedHeight - 1;
                    var indicationsLength = HotReloadWindowStyles.SectionInnerBox.fixedWidth - HotReloadWindowStyles.IndicationBox.margin.right - HotReloadWindowStyles.IndicationBox.margin.left - HotReloadWindowStyles.SectionInnerBox.padding.left - HotReloadWindowStyles.SectionInnerBox.padding.right; 
                    if (currentState.downloadRequired) {
                        using (new EditorGUILayout.HorizontalScope()) {
                            progress = EditorCodePatcher.DownloadProgress;
                            const int infoButtonWidth = 60;
                            const int padding = 5;
                            barRect.width = indicationsLength - infoButtonWidth - padding;
                            var spaceDistance = HotReloadWindowStyles.SectionInnerBox.fixedWidth - HotReloadWindowStyles.IndicationBox.margin.right - HotReloadWindowStyles.SectionInnerBox.padding.right - HotReloadWindowStyles.SectionInnerBox.padding.left - infoButtonWidth;
                            GUILayout.Space(spaceDistance - 10);
                            EditorGUI.ProgressBar(barRect, progress, "");
                            if (GUILayout.Button(new GUIContent(" Info", EditorGUIUtility.IconContent("console.infoicon").image), HotReloadWindowStyles.DownloadInfoButtonStyle)) {
                                Application.OpenURL(Constants.AdditionalContentURL);
                            }
                        }
                    } else {
                        barRect.width = indicationsLength;
                        progress = EditorCodePatcher.Stopping ? 1 : Mathf.Clamp(EditorCodePatcher.StartupProgress?.Item1 ?? 0f, 0f, 1f);
                        EditorGUI.ProgressBar(barRect, progress, "");
                    }
                } finally {
                    HotReloadWindowStyles.ProgressBarBarStyle.normal.background = bg;
                }
            }
        }

        private Texture2D MakeTexture(float maxHue) {
            var width = 11;
            var height = 11;
            Color[] pix = new Color[width * height];
            for (int y = 0; y < height; y++) {
                var middle = Math.Ceiling(height / (double)2);
                var maxGreen = maxHue;
                var yCoord = y + 1;
                var green = maxGreen - Math.Abs(yCoord - middle) * 0.02;
                for (int x = 0; x < width; x++) {
                    pix[y * width + x] = new Color(0.1f, (float)green, 0.1f, 1.0f);
                }
            }
            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
        
        
        /*
        [MenuItem("codepatcher/restart")]
        public static void TestRestart() {
            CodePatcherCLI.Restart(Application.dataPath, false);
        }
        */
        
    }

    internal static class HotReloadGUIHelper {
        public static bool LinkLabel(string labelText, int fontSize, FontStyle fontStyle, TextAnchor alignment, Color? color = null) {
            var stl = EditorStyles.label;

            // copy
            var origSize = stl.fontSize;
            var origStyle = stl.fontStyle;
            var origAnchor = stl.alignment;
            var origColor = stl.normal.textColor;

            // temporarily modify the built-in style
            stl.fontSize = fontSize;
            stl.fontStyle = fontStyle;
            stl.alignment = alignment;
            stl.normal.textColor = color ?? origColor;
            stl.active.textColor = color ?? origColor;
            stl.focused.textColor = color ?? origColor;
            stl.hover.textColor = color ?? origColor;

            try {
                return GUILayout.Button(labelText, stl);
            }  finally{
                // set the editor style (stl) back to normal
                stl.fontSize = origSize;
                stl.fontStyle = origStyle;
                stl.alignment = origAnchor;
                stl.normal.textColor = origColor;
                stl.active.textColor = origColor;
                stl.focused.textColor = origColor;
                stl.hover.textColor = origColor;
            }
        }

        public static void HelpBox(string message, MessageType type, int fontSize) {
            var _fontSize = EditorStyles.helpBox.fontSize;
            try {
                EditorStyles.helpBox.fontSize = fontSize;
                EditorGUILayout.HelpBox(message, type);
            } finally {
                EditorStyles.helpBox.fontSize = _fontSize;
            }
        }
    }

    internal enum PromoCodeErrorType {
        NONE,
        MISSING_INPUT,
        INVALID_HTTP_METHOD,
        BODY_INVALID,
        PROMO_CODE_NOT_FOUND,
        PROMO_CODE_CLAIMED,
        PROMO_CODE_EXPIRED,
        LICENSE_NOT_FOUND,
        LICENSE_NOT_TRIAL,
        LICENSE_ALREADY_EXTENDED,
        UPDATING_LICENSE_FAILED,
        CONDITIONAL_CHECK_FAILED,
        UNKNOWN_EXCEPTION,
        UNSUPPORTED_PATH,
    }

    internal class LoginData {
        public readonly string email;
        public readonly string password;

        public LoginData(string email, string password) {
            this.email = email;
            this.password = password;
        }
    }
}

