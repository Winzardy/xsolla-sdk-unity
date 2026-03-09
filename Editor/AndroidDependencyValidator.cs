#if UNITY_EDITOR && UNITY_ANDROID

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Xsolla.SDK
{
    internal sealed class AndroidDependencyValidatorWindow : EditorWindow
    {
        private const float Spacing = 8f;
        private const float SmallSpacing = 4f;
        private const float ButtonWidth = 100f;
        private const float IconWidth = 20f;

        private const string EDM4UGitHubUrl = "https://github.com/googlesamples/unity-jar-resolver";
        private const string EDM4UReleasesUrl = "https://github.com/googlesamples/unity-jar-resolver/releases";

        private static readonly Color SeparatorColor = new(0.5f, 0.5f, 0.5f, 0.5f);
        private static readonly Color SuccessColor = new(0.4f, 0.8f, 0.4f);
        private static readonly Color ErrorColor = new(1f, 0.4f, 0.4f);
        private static readonly Color ErrorBgColor = new(1f, 0.85f, 0.85f);
        private static readonly Color ValidBgColor = new(0.85f, 1f, 0.85f);
        private static readonly Color WarningBgColor = new(1f, 0.95f, 0.8f);

        private static readonly GUILayoutOption IconWidthOption = GUILayout.Width(IconWidth);

        private AndroidDependencyValidator.ValidationResult _result;
        private Vector2 _scrollPosition;
        private bool _errorsFoldout = true;
        private bool _validFoldout = true;
        private bool _isEDM4UInstalled;

        public static void Show(AndroidDependencyValidator.ValidationResult result)
        {
            var window = GetWindow<AndroidDependencyValidatorWindow>(true, "Android Dependency Validation", true);
            window._result = result;
            window._isEDM4UInstalled = IsEDM4UInstalled();
            window.minSize = new Vector2(500, 300);
            window.Show();
        }

        private static bool IsEDM4UInstalled()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                var name = assembly.GetName().Name;
                if (name.StartsWith("Google.JarResolver") ||
                    name.StartsWith("Google.VersionHandler") ||
                    name.Contains("ExternalDependencyManager"))
                    return true;

                try {
                    foreach (var type in assembly.GetTypes()) {
                        if (type.FullName != null &&
                            (type.FullName.Contains("PlayServicesResolver") ||
                             type.FullName.Contains("AndroidResolverInternal")))
                            return true;
                    }
                } catch {
                    // Ignore reflection errors
                }
            }

            var patterns = new[] { "PlayServicesResolver", "ExternalDependencyManager", "AndroidResolver" };
            foreach (var pattern in patterns) {
                foreach (var guid in AssetDatabase.FindAssets(pattern)) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.Contains("ExternalDependencyManager") ||
                        path.Contains("PlayServicesResolver") ||
                        path.Contains("Google.JarResolver"))
                        return true;
                }
            }

            return false;
        }

        private void OnGUI()
        {
            if (_result == null) {
                EditorGUILayout.HelpBox("No validation result available.", MessageType.Warning);
                return;
            }

            DrawHeader();
            DrawSummary();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (_result.Errors.Count > 0)
                DrawErrorsSection();

            if (_result.ValidatedDependencies.Count > 0)
                DrawValidSection();

            EditorGUILayout.EndScrollView();

            DrawEDM4USection();
            DrawFooter();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(SmallSpacing);

            using (new EditorGUILayout.HorizontalScope()) {
                GUILayout.FlexibleSpace();
                var (icon, color, status) = _result.IsValid ? ("✓", "green", "PASSED") : ("✗", "red", "FAILED");
                GUILayout.Label($"<color={color}><size=16>{icon}</size></color> <b>Validation {status}</b>",
                    new GUIStyle(EditorStyles.label) { richText = true, fontSize = 14 });
                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.Space(SmallSpacing);
            DrawSeparator();
            EditorGUILayout.Space(SmallSpacing);
        }

        private void DrawSummary()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
                GUILayout.FlexibleSpace();
                DrawSummaryItem("Total Required", _result.TotalRequired.ToString(), Color.white);
                GUILayout.Space(20);
                DrawSummaryItem("Passed", _result.ValidatedDependencies.Count.ToString(), SuccessColor);
                GUILayout.Space(20);
                DrawSummaryItem("Errors", _result.Errors.Count.ToString(), _result.Errors.Count > 0 ? ErrorColor : Color.white);
                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.Space(Spacing);
        }

        private static void DrawSummaryItem(string label, string value, Color color)
        {
            using (new EditorGUILayout.VerticalScope()) {
                GUILayout.Label(label, new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter });
                var prev = GUI.contentColor;
                GUI.contentColor = color;
                GUILayout.Label(value, new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 16 });
                GUI.contentColor = prev;
            }
        }

        private void DrawErrorsSection()
        {
            DrawSection(ErrorBgColor, $"Errors ({_result.Errors.Count})", ref _errorsFoldout, () => {
                foreach (var error in _result.Errors)
                    DrawErrorItem(error);
            });
            EditorGUILayout.Space(SmallSpacing);
        }

        private static void DrawErrorItem(string error)
        {
            var (type, message) = ParseError(error);

            using (new EditorGUILayout.HorizontalScope()) {
                DrawIcon("✗", "red");

                using (new EditorGUILayout.VerticalScope()) {
                    GUILayout.Label(type, new GUIStyle(EditorStyles.miniLabel) {
                        normal = { textColor = GetErrorTypeColor(type) },
                        fontStyle = FontStyle.Bold
                    });

                    if (type.Equals("Version mismatch", StringComparison.OrdinalIgnoreCase)) {
                        DrawVersionMismatchContent(message);
                    } else {
                        DrawGenericErrorContent(message);
                    }
                }
            }

            EditorGUILayout.Space(SmallSpacing);
        }

        private static void DrawVersionMismatchContent(string message)
        {
            // Parse: "group:artifact expected X but found Y in mainTemplate.gradle"
            var match = Regex.Match(message, @"^([^\s]+)\s+expected\s+([^\s]+)\s+but\s+found\s+([^\s]+)");
            if (match.Success) {
                var dependency = match.Groups[1].Value;
                var expected = match.Groups[2].Value;
                var found = match.Groups[3].Value;

                DrawSelectableField(dependency);

                EditorGUILayout.Space(2);

                DrawVersionRow("Expected:", expected, SuccessColor);
                DrawVersionRow("Found:", found, ErrorColor);
            } else {
                DrawGenericErrorContent(message);
            }
        }

        private static void DrawVersionRow(string label, string version, Color color)
        {
            using (new EditorGUILayout.HorizontalScope()) {
                GUILayout.Label(label, GUILayout.Width(60));
                DrawSelectableField(version, color, FontStyle.Bold);
            }
        }

        private static void DrawGenericErrorContent(string message)
        {
            var dependency = ExtractDependency(message);

            if (dependency != null) {
                DrawSelectableField(dependency);
                var desc = message.Replace(dependency, "").Trim().TrimStart(':').Trim();
                if (desc.Length > 0)
                    GUILayout.Label(desc, new GUIStyle(EditorStyles.miniLabel) { wordWrap = true });
            } else {
                GUILayout.Label(message, new GUIStyle(EditorStyles.label) { wordWrap = true });
            }
        }

        private static (string type, string message) ParseError(string error)
        {
            var match = Regex.Match(error, @"^\[([^\]]+)\]\s*(.*)$");
            return match.Success ? (match.Groups[1].Value, match.Groups[2].Value) : ("Error", error);
        }

        private static string ExtractDependency(string message)
        {
            var match = Regex.Match(message, @"([a-zA-Z0-9_.\-]+:[a-zA-Z0-9_.\-]+:[a-zA-Z0-9_.\-]+)");
            return match.Success ? match.Groups[1].Value : null;
        }

        private static Color GetErrorTypeColor(string type) =>
            type.ToLowerInvariant() switch {
                "missing" => new Color(0.9f, 0.3f, 0.3f),
                "duplicate" => new Color(0.9f, 0.6f, 0.2f),
                "version mismatch" => new Color(0.8f, 0.5f, 0.8f),
                "wrong keyword" => new Color(0.3f, 0.6f, 0.9f),
                _ => Color.red
            };

        private void DrawValidSection()
        {
            DrawSection(ValidBgColor, $"Validated ({_result.ValidatedDependencies.Count})", ref _validFoldout, () => {
                foreach (var dep in _result.ValidatedDependencies)
                    DrawValidItem(dep);
            });
        }

        private static void DrawValidItem(string dependency)
        {
            using (new EditorGUILayout.HorizontalScope()) {
                DrawIcon("✓", "green");
                DrawSelectableField(dependency);
            }
        }

        private void DrawEDM4USection()
        {
            EditorGUILayout.Space(SmallSpacing);

            if (_isEDM4UInstalled) {
                DrawInfoBox(
                    "Use the Android Resolver to automatically download and configure dependencies:",
                    "Assets → External Dependency Manager → Android Resolver → Resolve");
            } else {
                DrawWarningBox();
            }
        }

        private static void DrawInfoBox(string message, string highlightedPath)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
                var iconContent = EditorGUIUtility.IconContent("console.infoicon");
                GUILayout.Label(iconContent, GUILayout.Width(36), GUILayout.Height(36));

                using (new EditorGUILayout.VerticalScope()) {
                    GUILayout.Label("External Dependency Manager Detected", EditorStyles.boldLabel);

                    EditorGUILayout.Space(2);

                    GUILayout.Label(message, new GUIStyle(EditorStyles.label) { wordWrap = true });

                    EditorGUILayout.Space(4);

                    GUILayout.Label(highlightedPath, EditorStyles.boldLabel);
                }
            }
        }

        private static void DrawWarningBox()
        {
            var prev = GUI.backgroundColor;
            GUI.backgroundColor = WarningBgColor;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                GUI.backgroundColor = prev;

                using (new EditorGUILayout.HorizontalScope()) {
                    var iconContent = EditorGUIUtility.IconContent("console.warnicon");
                    GUILayout.Label(iconContent, GUILayout.Width(36), GUILayout.Height(36));

                    using (new EditorGUILayout.VerticalScope()) {
                        GUILayout.Label("External Dependency Manager Not Detected", EditorStyles.boldLabel);

                        EditorGUILayout.Space(2);

                        GUILayout.Label(
                            "EDM4U (External Dependency Manager for Unity) can automatically resolve Android dependencies for you.",
                            new GUIStyle(EditorStyles.label) { wordWrap = true });

                        EditorGUILayout.Space(Spacing);

                        using (new EditorGUILayout.HorizontalScope()) {
                            if (GUILayout.Button("View on GitHub", GUILayout.Width(ButtonWidth + 20)))
                                Application.OpenURL(EDM4UGitHubUrl);
                            GUILayout.Space(SmallSpacing);
                            if (GUILayout.Button("Download Latest", GUILayout.Width(ButtonWidth + 10)))
                                Application.OpenURL(EDM4UReleasesUrl);
                        }
                    }
                }
            }
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space(Spacing);
            DrawSeparator();
            EditorGUILayout.Space(Spacing);

            using (new EditorGUILayout.HorizontalScope()) {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Re-validate", GUILayout.Width(ButtonWidth))) {
                    _result = AndroidDependencyValidator.Validate(verboseLogs: true);
                    _isEDM4UInstalled = IsEDM4UInstalled();
                    Repaint();
                }

                GUILayout.Space(Spacing);

                if (GUILayout.Button("Close", GUILayout.Width(ButtonWidth - 20)))
                    Close();

                GUILayout.Space(Spacing);
            }

            EditorGUILayout.Space(Spacing);
        }

        private static void DrawSection(Color bgColor, string title, ref bool foldout, Action drawContent)
        {
            var prev = GUI.backgroundColor;
            GUI.backgroundColor = bgColor;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                GUI.backgroundColor = prev;
                foldout = EditorGUILayout.Foldout(foldout, title, true, EditorStyles.foldoutHeader);

                if (foldout) {
                    EditorGUILayout.Space(SmallSpacing);
                    drawContent();
                }
            }
        }

        private static void DrawSeparator() =>
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1f), SeparatorColor);

        private static void DrawIcon(string icon, string color) =>
            GUILayout.Label($"<color={color}>{icon}</color>",
                new GUIStyle(EditorStyles.label) { richText = true }, IconWidthOption);

        private static void DrawSelectableField(string text, Color? textColor = null, FontStyle fontStyle = FontStyle.Normal)
        {
            var style = new GUIStyle(EditorStyles.textField) {
                wordWrap = false,
                fontStyle = fontStyle,
                margin = new RectOffset(0, 4, 2, 2)
            };

            if (textColor.HasValue) {
                style.normal.textColor = textColor.Value;
                style.focused.textColor = textColor.Value;
            }

            var width = style.CalcSize(new GUIContent(text)).x + 8;
            EditorGUILayout.SelectableLabel(text, style, GUILayout.Width(width), GUILayout.Height(EditorGUIUtility.singleLineHeight));
        }
    }

    internal static class AndroidDependencyValidator
    {
        private const string DependenciesFileName = "android-dependencies.txt";
        private const string ImplementationKeyword = "implementation";
        private const string DefaultTemplatesDir = "Assets/Plugins/Android";

        private static readonly string[] PropertyFilenames = { "gradle.properties", "local.properties" };
        private static Dictionary<string, CatalogEntry> _versionCatalog;

        public sealed class ValidationResult
        {
            public bool IsValid => Errors.Count == 0;
            public List<string> Errors { get; } = new();
            public List<string> ValidatedDependencies { get; } = new();
            public int TotalRequired { get; set; }
        }

        private sealed class DependencyOccurrence
        {
            public string ModuleName, Config, Group, Artifact, Version;
        }

        private sealed class CatalogEntry
        {
            public string Group, Name, Version;
        }

        [MenuItem("Window/Xsolla/SDK/Dev Tools/Validate Android Dependencies", false, 1500)]
        private static void ValidateMenu()
        {
            var mainPath = Path.Combine(DefaultTemplatesDir, "mainTemplate.gradle").Replace('\\', '/');
            var launcherPath = Path.Combine(DefaultTemplatesDir, "launcherTemplate.gradle").Replace('\\', '/');

            if (!File.Exists(mainPath) && !File.Exists(launcherPath)) {
                EditorUtility.DisplayDialog(nameof(AndroidDependencyValidator), "No Gradle templates found.", "OK");
                return;
            }

            AndroidDependencyValidatorWindow.Show(Validate(verboseLogs: true));
        }

        internal static ValidationResult Validate(bool verboseLogs) => VerifyTemplatesProject(DefaultTemplatesDir, verboseLogs);

        private static ValidationResult VerifyTemplatesProject(string templatesRoot, bool verboseLogs)
        {
            var result = new ValidationResult();

            if (string.IsNullOrEmpty(templatesRoot) || !Directory.Exists(templatesRoot)) {
                LogError(result, $"Invalid templates root: '{templatesRoot}'.");
                return result;
            }

            Dictionary<string, string> required;
            try {
                var depsPath = FindDependenciesAssetPath();
                required = LoadRequiredDependencies(depsPath);
                if (verboseLogs)
                    Debug.Log($"[{nameof(AndroidDependencyValidator)}] Using {DependenciesFileName}: {depsPath} ({required.Count} entries)");
            } catch (Exception ex) {
                LogError(result, ex.Message);
                return result;
            }

            result.TotalRequired = required.Count;
            if (required.Count == 0) {
                if (verboseLogs) Debug.Log($"[{nameof(AndroidDependencyValidator)}] No dependencies to validate.");
                return result;
            }

            var mainPath = Path.Combine(templatesRoot, "mainTemplate.gradle").Replace('\\', '/');
            var launcherPath = Path.Combine(templatesRoot, "launcherTemplate.gradle").Replace('\\', '/');

            if (!File.Exists(mainPath) && !File.Exists(launcherPath)) {
                LogError(result, $"No Gradle templates found at '{templatesRoot}'.");
                return result;
            }

            _versionCatalog = LoadVersionCatalog(templatesRoot, GetProjectRoot());

            var occurrences = new Dictionary<string, List<DependencyOccurrence>>(StringComparer.OrdinalIgnoreCase) {
                ["mainTemplate.gradle"] = File.Exists(mainPath) ? ParseGradleModule(mainPath, "mainTemplate.gradle") : new(),
                ["launcherTemplate.gradle"] = File.Exists(launcherPath) ? ParseGradleModule(launcherPath, "launcherTemplate.gradle") : new()
            };

            result.Errors.AddRange(ValidateDependencies(required, occurrences, result.ValidatedDependencies));
            LogResults(result, required, verboseLogs);

            return result;
        }

        private static void LogError(ValidationResult result, string message)
        {
            result.Errors.Add(message);
            Debug.LogError($"[{nameof(AndroidDependencyValidator)}] {message}");
        }

        private static void LogResults(ValidationResult result, Dictionary<string, string> required, bool verboseLogs)
        {
            if (result.IsValid) {
                if (!verboseLogs) return;
                Debug.Log($"[{nameof(AndroidDependencyValidator)}] OK — all {required.Count} dependencies present:");
                foreach (var (k, v) in required)
                    Debug.Log($"[{nameof(AndroidDependencyValidator)}]   ✓ {k}:{v}");
            } else {
                Debug.LogError($"[{nameof(AndroidDependencyValidator)}] Validation failed with {result.Errors.Count} error(s):");
                foreach (var error in result.Errors)
                    Debug.LogError($"[{nameof(AndroidDependencyValidator)}]   ✗ {error}");

                if (result.ValidatedDependencies.Count > 0 && verboseLogs) {
                    Debug.Log($"[{nameof(AndroidDependencyValidator)}] Successfully validated:");
                    foreach (var dep in result.ValidatedDependencies)
                        Debug.Log($"[{nameof(AndroidDependencyValidator)}]   ✓ {dep}");
                }
            }
        }

        private static string FindDependenciesAssetPath()
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:MonoScript {nameof(AndroidDependencyValidator)}")) {
                var scriptPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!scriptPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                    !Path.GetFileName(scriptPath).Equals($"{nameof(AndroidDependencyValidator)}.cs", StringComparison.OrdinalIgnoreCase))
                    continue;

                var editorDir = Path.GetDirectoryName(scriptPath)?.Replace('\\', '/');
                if (string.IsNullOrEmpty(editorDir)) continue;

                var candidate = $"{editorDir}/{DependenciesFileName}";
                if (AssetDatabase.LoadAssetAtPath<TextAsset>(candidate)) return candidate;

                var parent = Path.GetDirectoryName(editorDir)?.Replace('\\', '/');
                if (string.IsNullOrEmpty(parent)) continue;

                candidate = $"{parent}/Editor/{DependenciesFileName}";
                if (AssetDatabase.LoadAssetAtPath<TextAsset>(candidate)) return candidate;
            }

            var hit = AssetDatabase.FindAssets("android-dependencies t:TextAsset")
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(p => p.EndsWith("/" + DependenciesFileName, StringComparison.OrdinalIgnoreCase));

            return hit ?? throw new Exception($"Cannot find {DependenciesFileName}. Ensure it resides in your plugin's Editor folder.");
        }

        private static Dictionary<string, string> LoadRequiredDependencies(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path)
                ?? throw new Exception($"Cannot load {DependenciesFileName} at '{path}'.");

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var lines = asset.text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

            for (var i = 0; i < lines.Length; i++) {
                var line = lines[i].Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;

                var parts = line.Split(':');
                if (parts.Length != 3)
                    throw new Exception($"{DependenciesFileName}:{i + 1} must be 'group:artifact:version' (got '{line}').");

                var group = parts[0].Trim();
                var artifact = parts[1].Trim();
                var version = parts[2].Trim();

                if (group.Length == 0 || artifact.Length == 0 || version.Length == 0)
                    throw new Exception($"{DependenciesFileName}:{i + 1} contains empty fields.");

                var key = $"{group}:{artifact}";
                if (!result.TryAdd(key, version))
                    throw new Exception($"{DependenciesFileName}:{i + 1} duplicate '{key}'.");
            }

            return result;
        }

        private static List<DependencyOccurrence> ParseGradleModule(string path, string moduleName)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return ParseGradleModuleRecursive(path, moduleName, visited, null);
        }

        private static List<DependencyOccurrence> ParseGradleModuleRecursive(
            string path, string moduleName, HashSet<string> visited, Dictionary<string, string> inheritedVars)
        {
            path = NormalizePath(path);
            if (!File.Exists(path) || !visited.Add(path))
                return new List<DependencyOccurrence>();

            var text = File.ReadAllText(path);
            var vars = MergeVars(inheritedVars, ExtractGroovyVars(text));
            var baseDir = Path.GetDirectoryName(path) ?? ".";
            var templatesRoot = NormalizePath(baseDir);

            if (visited.Count == 1)
                MergePropertiesIntoVars(new[] { templatesRoot, GetProjectRoot() }, vars);

            var results = ParseDependencies(text, moduleName, vars);

            foreach (var applyRef in FindApplyFromRefs(text)) {
                var resolved = ResolveApplyPath(baseDir, templatesRoot, applyRef, vars);
                if (resolved != null && File.Exists(resolved))
                    results.AddRange(ParseGradleModuleRecursive(resolved, moduleName, visited, vars));
            }

            return results;
        }

        private static Dictionary<string, string> MergeVars(Dictionary<string, string> inherited, Dictionary<string, string> current)
        {
            var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (inherited != null)
                foreach (var kv in inherited) merged[kv.Key] = kv.Value;
            foreach (var kv in current) merged[kv.Key] = kv.Value;
            return merged;
        }

        private static List<DependencyOccurrence> ParseDependencies(string text, string moduleName, Dictionary<string, string> vars)
        {
            var results = new List<DependencyOccurrence>();
            var blockRx = new Regex(@"\bdependencies\s*\{");

            foreach (Match match in blockRx.Matches(text)) {
                var braceOpen = match.Index + match.Length - 1;
                if (braceOpen >= text.Length || text[braceOpen] != '{') continue;

                var end = FindMatchingBrace(text, braceOpen);
                if (end <= braceOpen) continue;

                var body = text.Substring(braceOpen + 1, end - braceOpen - 1);
                body = Regex.Replace(body, @"/\*.*?\*/", "", RegexOptions.Singleline);
                body = body.Replace('\u201C', '"').Replace('\u201D', '"')
                           .Replace('\u2018', '\'').Replace('\u2019', '\'')
                           .Replace("\u200B", "");

                foreach (var stmt in CollectStatements(body)) {
                    var line = StripLineComment(stmt).Trim();
                    if (line.Length == 0) continue;

                    var occ = TryParseLibsCatalog(line, moduleName)
                           ?? TryParseGavString(line, moduleName, vars)
                           ?? TryParseNamedArgs(line, moduleName, vars);

                    if (occ != null) results.Add(occ);
                }
            }

            return results;
        }

        private static DependencyOccurrence TryParseLibsCatalog(string line, string moduleName)
        {
            var m = Regex.Match(line, @"^\s*([A-Za-z_]\w*)\s*\(?\s*libs\.([A-Za-z0-9_.\-]+)\s*\)?\s*$");
            if (!m.Success || _versionCatalog == null) return null;

            if (!_versionCatalog.TryGetValue(m.Groups[2].Value.Trim(), out var ce) ||
                string.IsNullOrEmpty(ce.Group) || string.IsNullOrEmpty(ce.Name) || string.IsNullOrEmpty(ce.Version))
                return null;

            return new DependencyOccurrence {
                ModuleName = moduleName, Config = m.Groups[1].Value.Trim(),
                Group = ce.Group, Artifact = ce.Name, Version = ce.Version
            };
        }

        private static DependencyOccurrence TryParseGavString(string line, string moduleName, Dictionary<string, string> vars)
        {
            var m = Regex.Match(line, @"^\s*([A-Za-z_]\w*)\s*\(?\s*['""]([^'""]+)['""]\s*\)?\s*$");
            if (!m.Success) return null;

            var gav = m.Groups[2].Value.Trim();
            if (!gav.Contains(":") || gav.Contains("(") || gav.Contains(")")) return null;

            var parts = gav.Split(':');
            if (parts.Length != 3) return null;

            var g = ResolveToken(parts[0], vars);
            var a = ResolveToken(parts[1], vars);
            var v = ResolveToken(parts[2], vars);
            if (g.Length == 0 || a.Length == 0 || v.Length == 0) return null;

            return new DependencyOccurrence {
                ModuleName = moduleName, Config = m.Groups[1].Value.Trim(),
                Group = g, Artifact = a, Version = v
            };
        }

        private static DependencyOccurrence TryParseNamedArgs(string line, string moduleName, Dictionary<string, string> vars)
        {
            var m = Regex.Match(line, @"^\s*([A-Za-z_]\w*)\s*\(?\s*(.*?)\s*\)?\s*$");
            if (!m.Success) return null;

            var args = m.Groups[2].Value;
            var g = CaptureKv(args, "group");
            var a = CaptureKv(args, "name");
            var v = CaptureKv(args, "version");
            if (string.IsNullOrEmpty(g) || string.IsNullOrEmpty(a) || string.IsNullOrEmpty(v)) return null;

            return new DependencyOccurrence {
                ModuleName = moduleName, Config = m.Groups[1].Value.Trim(),
                Group = ResolveToken(g, vars), Artifact = ResolveToken(a, vars), Version = ResolveToken(v, vars)
            };
        }

        private static string CaptureKv(string args, string key)
        {
            var m = Regex.Match(args, @"\b" + Regex.Escape(key) + @"\s*:\s*(['""])(.*?)\1", RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[2].Value : null;
        }

        private static List<string> FindApplyFromRefs(string text)
        {
            var refs = new List<string>();

            foreach (Match m in Regex.Matches(text, @"apply\s+from\s*:\s*(['""])(.*?)\1"))
                if (m.Groups[2].Value.Trim().Length > 0) refs.Add(m.Groups[2].Value.Trim());

            foreach (Match m in Regex.Matches(text, @"apply\s+from\s*:\s*rootProject\.file\(\s*(['""])(.*?)\1\s*\)"))
                if (m.Groups[2].Value.Trim().Length > 0) refs.Add("ROOTFILE::" + m.Groups[2].Value.Trim());

            foreach (Match m in Regex.Matches(text, @"apply\s+from\s*:\s*file\(\s*(['""])(.*?)\1\s*\)"))
                if (m.Groups[2].Value.Trim().Length > 0) refs.Add("FILE::" + m.Groups[2].Value.Trim());

            return refs;
        }

        private static string ResolveApplyPath(string baseDir, string templatesRoot, string applyRef, Dictionary<string, string> vars)
        {
            var isRoot = applyRef.StartsWith("ROOTFILE::", StringComparison.OrdinalIgnoreCase);
            var isFile = applyRef.StartsWith("FILE::", StringComparison.OrdinalIgnoreCase);
            var refVal = isRoot ? applyRef[10..] : isFile ? applyRef[6..] : applyRef;

            refVal = ReplacePathVars(refVal, baseDir, templatesRoot, vars);

            return NormalizePath(Path.IsPathRooted(refVal)
                ? refVal
                : Path.GetFullPath(isRoot ? Path.Combine(templatesRoot, refVal) : Path.Combine(baseDir, refVal)));
        }

        private static string ReplacePathVars(string s, string baseDir, string root, Dictionary<string, string> vars)
        {
            s = s.Replace("${rootDir}", root).Replace("$rootDir", root)
                 .Replace("${projectDir}", baseDir).Replace("$projectDir", baseDir);

            s = Regex.Replace(s, @"\$\{(\w+)\}", m => vars.TryGetValue(m.Groups[1].Value, out var v) ? v : m.Value);
            s = Regex.Replace(s, @"\$(\w+)", m => vars.TryGetValue(m.Groups[1].Value, out var v) ? v : m.Value);

            return NormalizePath(s);
        }

        private static string NormalizePath(string p) => (p ?? "").Replace('\\', '/');
        private static string GetProjectRoot() => NormalizePath(Path.GetDirectoryName(Application.dataPath) ?? ".");

        private static IEnumerable<string> CollectStatements(string body)
        {
            var lines = body.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            var buf = "";
            int depth = 0;
            bool inS = false, inD = false;

            foreach (var raw in lines) {
                foreach (var c in raw) {
                    if (c == '\'' && !inD) inS = !inS;
                    else if (c == '"' && !inS) inD = !inD;
                    else if (!inS && !inD) {
                        if (c == '(') depth++;
                        else if (c == ')') depth--;
                    }
                }

                buf += (buf.Length > 0 ? " " : "") + raw.Trim();

                if (depth <= 0 && !inS && !inD) {
                    if (buf.Trim().Length > 0) yield return buf.Trim();
                    buf = "";
                }
            }

            if (buf.Trim().Length > 0) yield return buf.Trim();
        }

        private static string ResolveToken(string token, Dictionary<string, string> vars)
        {
            var m = Regex.Match(token, @"^\$\{?(\w+)\}?$");
            return m.Success && vars.TryGetValue(m.Groups[1].Value, out var v) ? v.Trim() : token.Trim();
        }

        private static int FindMatchingBrace(string s, int open)
        {
            int depth = 0;
            for (var i = open; i < s.Length; i++) {
                if (s[i] == '{') depth++;
                else if (s[i] == '}' && --depth == 0) return i;
            }
            return -1;
        }

        private static string StripLineComment(string line)
        {
            bool inS = false, inD = false;
            for (var i = 0; i < line.Length - 1; i++) {
                if (line[i] == '\'' && !inD) inS = !inS;
                else if (line[i] == '"' && !inS) inD = !inD;
                else if (line[i] == '/' && line[i + 1] == '/' && !inS && !inD) return line[..i];
            }
            return line;
        }

        private static Dictionary<string, string> ExtractGroovyVars(string text)
        {
            var vars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (Match m in Regex.Matches(text, @"\b(?:def\s+)?(\w+)\s*=\s*['""]([^'""]+)['""]"))
                vars[m.Groups[1].Value] = m.Groups[2].Value;

            foreach (Match m in Regex.Matches(text, @"\bext\.(\w+)\s*=\s*['""]([^'""]+)['""]"))
                vars[m.Groups[1].Value] = m.Groups[2].Value;

            foreach (Match block in Regex.Matches(text, @"ext\s*\{([^}]*)\}", RegexOptions.Singleline))
                foreach (Match m in Regex.Matches(block.Groups[1].Value, @"\b(?:def\s+)?(\w+)\s*=\s*['""]([^'""]+)['""]"))
                    vars[m.Groups[1].Value] = m.Groups[2].Value;

            return vars;
        }

        private static void MergePropertiesIntoVars(IEnumerable<string> roots, Dictionary<string, string> vars)
        {
            foreach (var root in roots) {
                foreach (var filename in PropertyFilenames) {
                    var p = Path.Combine(string.IsNullOrEmpty(root) ? "." : root, filename);
                    if (!File.Exists(p)) continue;

                    foreach (var raw in File.ReadAllLines(p)) {
                        var s = raw.Trim();
                        if (s.Length == 0 || s.StartsWith("#")) continue;
                        var idx = s.IndexOf('=');
                        if (idx <= 0) continue;
                        var k = s[..idx].Trim();
                        if (k.Length > 0) vars[k] = s[(idx + 1)..].Trim();
                    }
                }
            }
        }

        private static Dictionary<string, CatalogEntry> LoadVersionCatalog(params string[] roots)
        {
            var catalog = new Dictionary<string, CatalogEntry>(StringComparer.OrdinalIgnoreCase);
            var versions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var path = roots.Select(r => Path.Combine(string.IsNullOrEmpty(r) ? "." : r, "gradle", "libs.versions.toml"))
                            .FirstOrDefault(File.Exists);
            if (path == null) return catalog;

            var section = "";
            foreach (var raw in File.ReadAllLines(path)) {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;

                if (line.StartsWith("[")) {
                    section = line.Equals("[versions]", StringComparison.OrdinalIgnoreCase) ? "versions" :
                              line.Equals("[libraries]", StringComparison.OrdinalIgnoreCase) ? "libraries" : "";
                    continue;
                }

                if (section == "versions") {
                    var m = Regex.Match(line, @"^([A-Za-z0-9_.\-]+)\s*=\s*['""]([^'""]+)['""]\s*$");
                    if (m.Success) versions[m.Groups[1].Value] = m.Groups[2].Value;
                } else if (section == "libraries") {
                    var entry = ParseCatalogEntry(line, versions);
                    if (entry.HasValue) catalog[entry.Value.alias] = entry.Value.entry;
                }
            }

            return catalog;
        }

        private static (string alias, CatalogEntry entry)? ParseCatalogEntry(string line, Dictionary<string, string> versions)
        {
            var mBraces = Regex.Match(line, @"^([A-Za-z0-9_.\-]+)\s*=\s*\{([^}]*)\}\s*$");
            if (mBraces.Success) {
                var alias = mBraces.Groups[1].Value;
                var body = mBraces.Groups[2].Value;
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (Match kv in Regex.Matches(body, @"([A-Za-z0-9_.\-]+)\s*=\s*(['""])(.*?)\2"))
                    dict[kv.Groups[1].Value] = kv.Groups[3].Value;

                var entry = new CatalogEntry();

                if (dict.TryGetValue("module", out var module)) {
                    var parts = module.Split(':');
                    if (parts.Length >= 2) { entry.Group = parts[0].Trim(); entry.Name = parts[1].Trim(); }
                } else {
                    dict.TryGetValue("group", out entry.Group);
                    dict.TryGetValue("name", out entry.Name);
                }

                if (dict.TryGetValue("version", out var v)) entry.Version = v;
                else if (dict.TryGetValue("version.ref", out var vr) && versions.TryGetValue(vr, out var vv)) entry.Version = vv;

                if (!string.IsNullOrEmpty(entry.Group) && !string.IsNullOrEmpty(entry.Name))
                    return (alias, entry);
            }

            var mSimple = Regex.Match(line, @"^([A-Za-z0-9_.\-]+)\s*=\s*['""]([^'""]+)['""]\s*$");
            if (mSimple.Success) {
                var parts = mSimple.Groups[2].Value.Split(':');
                if (parts.Length >= 2) {
                    var entry = new CatalogEntry { Group = parts[0].Trim(), Name = parts[1].Trim() };
                    if (parts.Length >= 3) entry.Version = parts[2].Trim();
                    return (mSimple.Groups[1].Value, entry);
                }
            }

            return null;
        }

        private static List<string> ValidateDependencies(
            Dictionary<string, string> required,
            Dictionary<string, List<DependencyOccurrence>> occurrences,
            List<string> validated)
        {
            var errors = new List<string>();

            foreach (var (ga, expectedVersion) in required) {
                var hasErrors = false;
                var mainDeps = GetOccurrences("mainTemplate.gradle", ga, occurrences);

                if (mainDeps.Count == 0) {
                    errors.Add($"[Missing] {ga}:{expectedVersion} not found in mainTemplate.gradle");
                    hasErrors = true;
                } else if (mainDeps.Count > 1) {
                    errors.Add($"[Duplicate] {ga} appears {mainDeps.Count} times in mainTemplate.gradle ({string.Join(", ", mainDeps.Select(o => $"{o.Config}@{o.Version}"))})");
                    hasErrors = true;
                } else {
                    var occ = mainDeps[0];
                    if (!occ.Config.Equals(ImplementationKeyword, StringComparison.OrdinalIgnoreCase)) {
                        errors.Add($"[Wrong keyword] {ga} uses '{occ.Config}' in mainTemplate.gradle — must be '{ImplementationKeyword}'");
                        hasErrors = true;
                    }
                    if (!occ.Version.Equals(expectedVersion, StringComparison.OrdinalIgnoreCase)) {
                        errors.Add($"[Version mismatch] {ga} expected {expectedVersion} but found {occ.Version}");
                        hasErrors = true;
                    }
                }

                var launcherDeps = GetOccurrences("launcherTemplate.gradle", ga, occurrences);
                if (launcherDeps.Count > 0) {
                    errors.Add($"[Duplicate] {ga} must NOT be in launcherTemplate.gradle (found {launcherDeps.Count}: {string.Join(", ", launcherDeps.Select(o => $"{o.Config}@{o.Version}"))})");
                    hasErrors = true;
                }

                if (!hasErrors) validated.Add($"{ga}:{expectedVersion}");
            }

            return errors;
        }

        private static List<DependencyOccurrence> GetOccurrences(string module, string ga, Dictionary<string, List<DependencyOccurrence>> occurrences)
        {
            var parts = ga.Split(':');
            return occurrences.TryGetValue(module, out var list)
                ? list.Where(o => o.Group.Equals(parts[0], StringComparison.OrdinalIgnoreCase) &&
                                  o.Artifact.Equals(parts[1], StringComparison.OrdinalIgnoreCase)).ToList()
                : new List<DependencyOccurrence>();
        }
    }
}

#endif