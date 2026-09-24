using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Builds the MainGame scene. Menu: Lab2 → Build Scene
/// </summary>
public static class Lab2SceneSetup
{
    const string ScenePath = "Assets/Scenes/MainGame.unity";

    [MenuItem("Lab2/Build Scene")]
    public static void BuildFromMenu()
    {
        BuildScene();
        EditorUtility.DisplayDialog(
            "Lab2",
            "MainGame scene built.\n\nPress Play.\nWASD/Arrows to move.\nJ = +10 score, L = lose, K = win, R = restart after game over.",
            "OK");
    }

    public static void BuildFromCommandLine()
    {
        try
        {
            BuildScene();
            Debug.Log("MainGame scene built successfully.");
            EditorApplication.Exit(0);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Scene setup failed: " + ex);
            EditorApplication.Exit(1);
        }
    }

    static Material CreateColorMaterial(string assetPath, Color color)
    {
        var shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Lit");
        var mat = new Material(shader);
        mat.color = color;
        AssetDatabase.CreateAsset(mat, assetPath);
        return mat;
    }

    static void BuildScene()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        var bounce = new PhysicsMaterial("WallBounce")
        {
            bounciness = 0.95f,
            dynamicFriction = 0.05f,
            staticFriction = 0.05f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Maximum
        };
        AssetDatabase.CreateAsset(bounce, "Assets/Materials/WallBounce.asset");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.6f);

        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = Color.white;
        light.intensity = 1.1f;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        var arenaGo = new GameObject("Arena");
        var arena = arenaGo.AddComponent<ArenaBuilder>();
        arena.BuildArena();

        foreach (Transform child in arenaGo.transform)
        {
            if (!child.name.StartsWith("Wall"))
                continue;
            var col = child.GetComponent<Collider>();
            if (col != null)
                col.sharedMaterial = bounce;
        }

        var player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 0.5f, 0f);
        player.transform.localScale = Vector3.one;

        var playerMat = CreateColorMaterial("Assets/Materials/PlayerMaterial.mat", new Color(0.2f, 0.85f, 1f));
        player.GetComponent<Renderer>().sharedMaterial = playerMat;

        var prb = player.AddComponent<Rigidbody>();
        prb.mass = 1f;
        prb.useGravity = false;
        prb.linearDamping = 1.2f;
        prb.angularDamping = 5f;
        prb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        prb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        prb.interpolation = RigidbodyInterpolation.Interpolate;

        player.AddComponent<PlayerController>();
        player.GetComponent<Collider>().sharedMaterial = bounce;

        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.07f, 0.12f);
        cam.fieldOfView = 55f;
        camGo.AddComponent<AudioListener>();
        var follow = camGo.AddComponent<CameraFollow>();
        follow.SetTarget(player.transform);
        camGo.transform.position = player.transform.position + new Vector3(0f, 22f, -10f);
        camGo.transform.LookAt(player.transform);

        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        Text scoreText = CreateUiText(canvasGo.transform, "ScoreText", "Score: 0",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f), new Vector2(320f, 50f),
            TextAnchor.UpperLeft, 28);

        GameObject gameOverPanel = CreatePanel(canvasGo.transform, "GameOverPanel", new Color(0f, 0f, 0f, 0.7f));
        CreateUiText(gameOverPanel.transform, "Title", "GAME OVER",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(500f, 60f),
            TextAnchor.MiddleCenter, 42);
        Text goScore = CreateUiText(gameOverPanel.transform, "FinalScore", "Score: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(400f, 40f),
            TextAnchor.MiddleCenter, 24);
        CreateButton(gameOverPanel.transform, "RestartButton", "Restart (R)", new Vector2(0f, -70f));

        GameObject winPanel = CreatePanel(canvasGo.transform, "WinPanel", new Color(0f, 0.15f, 0.05f, 0.75f));
        CreateUiText(winPanel.transform, "Title", "YOU WIN!",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(500f, 60f),
            TextAnchor.MiddleCenter, 42);
        Text winScore = CreateUiText(winPanel.transform, "FinalScore", "Score: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(400f, 40f),
            TextAnchor.MiddleCenter, 24);
        CreateButton(winPanel.transform, "RestartButton", "Restart (R)", new Vector2(0f, -70f));

        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);

        var gmGo = new GameObject("GameManager");
        var gm = gmGo.AddComponent<GameManager>();

        var so = new SerializedObject(gm);
        so.FindProperty("scoreText").objectReferenceValue = scoreText;
        so.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
        so.FindProperty("winPanel").objectReferenceValue = winPanel;
        so.FindProperty("gameOverScoreText").objectReferenceValue = goScore;
        so.FindProperty("winScoreText").objectReferenceValue = winScore;
        so.ApplyModifiedPropertiesWithoutUndo();

        WireRestartButton(gameOverPanel.transform.Find("RestartButton"), gm);
        WireRestartButton(winPanel.transform.Find("RestartButton"), gm);

        var npcRoot = new GameObject("NPCRoot");
        npcRoot.transform.position = Vector3.zero;

        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(ScenePath);
    }

    static Text CreateUiText(Transform parent, string name, string content,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size,
        TextAnchor align, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(anchorMin.x == 0.5f ? 0.5f : anchorMin.x, anchorMin.y == 1f ? 1f : 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var text = go.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.alignment = align;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    static void CreateButton(Transform parent, string name, string label, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(200f, 44f);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.45f, 0.85f, 1f);
        go.AddComponent<Button>();

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var trt = textGo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        var text = textGo.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.fontSize = 20;
    }

    static void WireRestartButton(Transform buttonTransform, GameManager gm)
    {
        if (buttonTransform == null)
            return;
        var btn = buttonTransform.GetComponent<Button>();
        if (btn == null)
            return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(gm.OnRestartButton);
    }
}
