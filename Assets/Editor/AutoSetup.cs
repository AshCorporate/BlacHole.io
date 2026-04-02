using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BlacHole.Editor
{
    /// <summary>
    /// Editor utility: auto-wires ScriptableObject configs to GameBootstrap and
    /// registers all scenes in Build Settings.
    /// </summary>
    public static class AutoSetup
    {
        private const string BootScenePath      = "Assets/Scenes/Boot.unity";
        private const string MainMenuScenePath  = "Assets/Scenes/MainMenu.unity";
        private const string GameScenePath      = "Assets/Scenes/Game.unity";
        private const string ScriptableObjDir   = "Assets/Data/ScriptableObjects";

        [MenuItem("Tools/BlacHole.io/Auto Setup Project")]
        public static void RunAutoSetup()
        {
            // ── 1. Create default ScriptableObject configs (if missing) ──────
            ConfigCreator.CreateDefaultConfigs();

            // ── 2. Build all scenes and register them in Build Settings ──────
            SceneBuilder.BuildAllScenes();

            // ── 3. Save everything ───────────────────────────────────────────
            AssetDatabase.SaveAssets();

            Debug.Log("✅ BlacHole.io Auto Setup Complete! Press Play to start.");
        }

        [MenuItem("Tools/BlacHole.io/Open Boot Scene")]
        public static void OpenBootScene()
        {
            if (!File.Exists(BootScenePath))
            {
                Debug.LogWarning($"[AutoSetup] Boot scene not found at '{BootScenePath}'.");
                return;
            }

            EditorSceneManager.OpenScene(BootScenePath);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static void AssignConfigsToBootstrap()
        {
            if (!File.Exists(BootScenePath))
            {
                Debug.LogWarning($"[AutoSetup] Boot scene not found at '{BootScenePath}'. Skipping config assignment.");
                return;
            }

            // Open Boot scene without affecting the currently-open scene.
            var scene = EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Additive);

            try
            {
                // Find GameBootstrap in the scene.
                Component bootstrap = null;
                foreach (var go in scene.GetRootGameObjects())
                {
                    bootstrap = go.GetComponentInChildren(System.Type.GetType("BlacHole.Core.GameBootstrap, Assembly-CSharp"), true)
                             ?? go.GetComponentInChildren(System.Type.GetType("BlacHole.Core.GameBootstrap"), true);
                    if (bootstrap != null) break;
                }
                if (bootstrap == null)
                {
                    // Fallback: search by component name string across root objects
                    foreach (var go in scene.GetRootGameObjects())
                    {
                        bootstrap = go.GetComponentInChildren<MonoBehaviour>(true) is MonoBehaviour mb
                                 && mb.GetType().Name == "GameBootstrap" ? mb : null;
                        if (bootstrap != null) break;
                        foreach (var mb2 in go.GetComponentsInChildren<MonoBehaviour>(true))
                        {
                            if (mb2.GetType().Name == "GameBootstrap") { bootstrap = mb2; break; }
                        }
                        if (bootstrap != null) break;
                    }
                }

                if (bootstrap == null)
                {
                    Debug.LogWarning("[AutoSetup] GameBootstrap component not found in Boot scene. Skipping config assignment.");
                    return;
                }

                // Find all ScriptableObject assets in the config directory.
                if (!Directory.Exists(ScriptableObjDir))
                {
                    Debug.LogWarning($"[AutoSetup] ScriptableObjects directory not found at '{ScriptableObjDir}'. Skipping config assignment.");
                    return;
                }

                var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { ScriptableObjDir });
                var so    = new SerializedObject(bootstrap);

                foreach (var guid in guids)
                {
                    var path  = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                    if (asset == null) continue;

                    // Try to match the asset type name to a serialized field name
                    // (e.g. PlayerConfig → playerConfig / PlayerConfig).
                    var typeName  = asset.GetType().Name;
                    if (string.IsNullOrEmpty(typeName)) continue;
                    var fieldName = char.ToLower(typeName[0]) + typeName.Substring(1);

                    var prop = so.FindProperty(fieldName) ?? so.FindProperty(typeName);
                    if (prop != null && prop.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        prop.objectReferenceValue = asset;
                        Debug.Log($"[AutoSetup] Assigned {typeName} → {bootstrap.name}.{fieldName}");
                    }
                }

                so.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.SaveScene(scene);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void RegisterScenes()
        {
            var scenePaths = new[] { BootScenePath, MainMenuScenePath, GameScenePath };
            var existing   = EditorBuildSettings.scenes.ToList();

            foreach (var path in scenePaths)
            {
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[AutoSetup] Scene not found: '{path}'. Skipping.");
                    continue;
                }

                if (existing.Any(s => s.path == path))
                    continue;

                existing.Add(new EditorBuildSettingsScene(path, true));
                Debug.Log($"[AutoSetup] Added to Build Settings: {path}");
            }

            EditorBuildSettings.scenes = existing.ToArray();
        }
    }
}
