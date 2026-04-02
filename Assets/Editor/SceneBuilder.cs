using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace BlacHole.Editor
{
    /// <summary>
    /// Programmatically builds all game scenes and registers them in Build Settings.
    /// Menu: Tools → BlacHole.io → 🚀 BUILD ALL SCENES &amp; PLAY
    ///        Tools → BlacHole.io → 📁 Create Scenes Only
    /// </summary>
    public static class SceneBuilder
    {
        private const string ScenesFolder      = "Assets/Scenes";
        private const string BootScenePath     = "Assets/Scenes/Boot.unity";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string GameScenePath     = "Assets/Scenes/Game.unity";
        private const string DataDir           = "Assets/Data";

        // ── Public menu items ────────────────────────────────────────────────

        [MenuItem("Tools/BlacHole.io/🚀 BUILD ALL SCENES & PLAY")]
        public static void BuildAllScenesAndPlay()
        {
            BuildAllScenes();
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Tools/BlacHole.io/📁 Create Scenes Only")]
        public static void BuildAllScenes()
        {
            EnsureScenesFolder();
            CreateBootScene();
            CreateMainMenuScene();
            CreateGameScene();
            RegisterScenes();
            TryAutoWireConfigs();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("✅ All scenes built successfully!");
        }

        // ── Scene creation ───────────────────────────────────────────────────

        private static void EnsureScenesFolder()
        {
            if (!AssetDatabase.IsValidFolder(ScenesFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
                Debug.Log($"[SceneBuilder] Created folder: {ScenesFolder}");
            }
        }

        private static void CreateBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // GameBootstrap
            var bootstrapGo = new GameObject("GameBootstrap");
            AddComponentByTypeName(bootstrapGo, "BlacHole.Core.GameBootstrap");

            // AudioListener
            var audioGo = new GameObject("AudioListener");
            audioGo.AddComponent<AudioListener>();

            // EventSystem
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, BootScenePath);
            Debug.Log($"[SceneBuilder] Boot scene saved → {BootScenePath}");
        }

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.backgroundColor = HexColor("0A0A1A");
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.AddComponent<AudioListener>();

            // Canvas (Screen Space Overlay)
            var canvasGo = CreateCanvas();
            var scaler   = canvasGo.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (scaler == null) scaler = canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode            = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution    = new Vector2(1920, 1080);

            // MainMenuController on Canvas
            var mmcGo = new GameObject("MainMenuController");
            mmcGo.transform.SetParent(canvasGo.transform, false);
            AddComponentByTypeName(mmcGo, "BlacHole.UI.MainMenu.MainMenuController");

            // EventSystem
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
            Debug.Log($"[SceneBuilder] MainMenu scene saved → {MainMenuScenePath}");
        }

        private static void CreateGameScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic      = true;
            cam.orthographicSize  = 10f;
            cam.backgroundColor   = HexColor("050510");
            cam.clearFlags        = CameraClearFlags.SolidColor;
            cam.depth             = -1f;
            camGo.AddComponent<AudioListener>();

            // GameSessionManager
            var gsmGo = new GameObject("GameSessionManager");
            AddComponentByTypeName(gsmGo, "BlacHole.Core.GameSessionManager");

            // MapGenerator
            var mapGo = new GameObject("MapGenerator");
            AddComponentByTypeName(mapGo, "BlacHole.Gameplay.MapGeneration.MapGenerator");

            // Canvas + HUDController
            var canvasGo = CreateCanvas();
            var hudGo    = new GameObject("HUDController");
            hudGo.transform.SetParent(canvasGo.transform, false);
            AddComponentByTypeName(hudGo, "BlacHole.UI.HUD.HUDController");

            // EventSystem
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, GameScenePath);
            Debug.Log($"[SceneBuilder] Game scene saved → {GameScenePath}");
        }

        // ── Build Settings ───────────────────────────────────────────────────

        private static void RegisterScenes()
        {
            var paths    = new[] { BootScenePath, MainMenuScenePath, GameScenePath };
            var existing = EditorBuildSettings.scenes.ToList();

            // Remove stale entries for our three paths, then re-insert in order.
            existing.RemoveAll(s => paths.Contains(s.path));

            var newEntries = paths
                .Where(p => File.Exists(p))
                .Select(p => new EditorBuildSettingsScene(p, true));

            existing.InsertRange(0, newEntries);
            EditorBuildSettings.scenes = existing.ToArray();
            Debug.Log("[SceneBuilder] Build Settings updated: Boot(0) MainMenu(1) Game(2)");
        }

        // ── Auto-wire configs ────────────────────────────────────────────────

        private static void TryAutoWireConfigs()
        {
            if (!File.Exists(BootScenePath)) return;

            var scene = EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Additive);
            try
            {
                Component bootstrap = null;
                foreach (var go in scene.GetRootGameObjects())
                {
                    foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
                    {
                        if (mb.GetType().Name == "GameBootstrap") { bootstrap = mb; break; }
                    }
                    if (bootstrap != null) break;
                }

                if (bootstrap == null) return;

                if (!AssetDatabase.IsValidFolder(DataDir)) return;

                var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { DataDir });
                var so    = new SerializedObject(bootstrap);

                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var asset     = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                    if (asset == null) continue;

                    var typeName  = asset.GetType().Name;
                    if (string.IsNullOrEmpty(typeName)) continue;
                    var fieldName = char.ToLower(typeName[0]) + typeName.Substring(1);

                    var prop = so.FindProperty(fieldName) ?? so.FindProperty(typeName);
                    if (prop != null && prop.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        prop.objectReferenceValue = asset;
                        Debug.Log($"[SceneBuilder] Auto-wired {typeName} → GameBootstrap.{fieldName}");
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

        // ── Helpers ──────────────────────────────────────────────────────────

        private static GameObject CreateCanvas()
        {
            var canvasGo = new GameObject("Canvas");
            var canvas   = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            return canvasGo;
        }

        private static void CreateEventSystem()
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
        }

        /// <summary>Adds a component by its full type name. Logs a warning if the type is not found.</summary>
        private static Component AddComponentByTypeName(GameObject go, string fullTypeName)
        {
            var type = System.Type.GetType($"{fullTypeName}, Assembly-CSharp")
                    ?? System.Type.GetType(fullTypeName);
            if (type == null)
            {
                Debug.LogWarning($"[SceneBuilder] Type not found: '{fullTypeName}'. Component not added to '{go.name}'.");
                return null;
            }
            return go.AddComponent(type);
        }

        private static Color HexColor(string hex)
        {
            if (!ColorUtility.TryParseHtmlString($"#{hex}", out var color))
            {
                Debug.LogWarning($"[SceneBuilder] Could not parse color '#{hex}'. Using black.");
                color = Color.black;
            }
            return color;
        }
    }
}
