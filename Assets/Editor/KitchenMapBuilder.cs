#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class KitchenMapBuilder
{
    private const string ScenePath = "Assets/Scenes/KitchenCourse.unity";
    private const string MaterialFolder = "Assets/_Materials/KitchenCourse";

    [MenuItem("Tools/Kitchen Gimmicks/Build Kitchen Course")]
    public static void BuildCourse()
    {
        EnsureFolder("Assets/_Materials");
        EnsureFolder(MaterialFolder);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "KitchenCourse";

        Material counter = Material("Counter", new Color(0.74f, 0.53f, 0.32f));
        Material counterEdge = Material("CounterEdge", new Color(0.34f, 0.18f, 0.09f));
        Material metal = Material("Metal", new Color(0.58f, 0.64f, 0.68f), 0.8f);
        Material darkMetal = Material("DarkMetal", new Color(0.13f, 0.15f, 0.17f), 0.75f);
        Material cartRed = Material("CartRed", new Color(0.72f, 0.08f, 0.06f), 0.35f);
        Material honey = Material("Honey", new Color(1f, 0.48f, 0.02f), 0.25f);
        Material soap = Material("Soap", new Color(0.10f, 0.75f, 0.84f), 0.35f);
        Material porcelain = Material("Porcelain", new Color(0.91f, 0.95f, 1f), 0.55f);
        Material fan = Material("FanPurple", new Color(0.55f, 0.17f, 0.72f), 0.3f);
        Material green = Material("GoalGreen", new Color(0.14f, 0.72f, 0.26f), 0.25f);
        Material playerMaterial = Material("Player", new Color(1f, 0.88f, 0.12f), 0.55f);

        GameObject environment = new("ENVIRONMENT");
        GameObject gameplay = new("GAMEPLAY");
        GameObject decoration = new("DECORATION");

        BuildCounter(environment.transform, counter, counterEdge);
        BuildCart(decoration.transform, cartRed, metal);
        BuildHoneySection(gameplay.transform, honey);
        BuildSinkSection(gameplay.transform, decoration.transform, metal, porcelain, soap, darkMetal);
        BuildFanSection(gameplay.transform, decoration.transform, fan, metal);
        BuildGoal(gameplay.transform, green, darkMetal, porcelain);

        GameObject player = Primitive("PlayerBall", PrimitiveType.Sphere,
            new Vector3(-23f, 1.35f, 0f), Vector3.one * 1.3f, playerMaterial, gameplay.transform);
        Rigidbody body = player.AddComponent<Rigidbody>();
        body.mass = 1.2f;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        player.AddComponent<PlayerBall>();

        CreateCheckpoint("StartCheckpoint", new Vector3(-23f, 1f, 0f), new Vector3(3f, 2f, 5f), gameplay.transform);
        CreateCheckpoint("SinkCheckpoint", new Vector3(2f, 1f, 0f), new Vector3(3f, 2f, 8f), gameplay.transform);
        CreateCheckpoint("FanCheckpoint", new Vector3(17f, 1f, 0f), new Vector3(3f, 2f, 8f), gameplay.transform);

        Camera camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(KitchenCameraFollow))
            .GetComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.position = player.transform.position + new Vector3(0f, 11f, -13f);
        camera.fieldOfView = 58f;
        camera.backgroundColor = new Color(0.55f, 0.78f, 0.94f);
        camera.GetComponent<KitchenCameraFollow>().SetTarget(player.transform);

        GameObject lightObject = new("Directional Light", typeof(Light));
        Light light = lightObject.GetComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.35f;
        light.shadows = LightShadows.Soft;
        lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.62f, 0.72f, 0.82f);
        RenderSettings.ambientEquatorColor = new Color(0.38f, 0.35f, 0.32f);
        RenderSettings.ambientGroundColor = new Color(0.15f, 0.11f, 0.09f);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
        Selection.activeGameObject = player;
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));
        Debug.Log($"Kitchen course created: {ScenePath}");
    }

    private static void BuildCounter(Transform parent, Material top, Material edge)
    {
        Primitive("CounterTop", PrimitiveType.Cube, new Vector3(5f, -0.35f, 0f),
            new Vector3(62f, 1f, 14f), top, parent);
        Primitive("FrontEdge", PrimitiveType.Cube, new Vector3(5f, -1.25f, -7.15f),
            new Vector3(62f, 1.2f, 0.45f), edge, parent);
        Primitive("BackSplash", PrimitiveType.Cube, new Vector3(5f, 2.2f, 7.15f),
            new Vector3(62f, 5f, 0.45f), edge, parent);
    }

    private static void BuildCart(Transform parent, Material red, Material metal)
    {
        GameObject cart = new("ShoppingCart_START");
        cart.transform.SetParent(parent);
        cart.transform.position = new Vector3(-24f, 1.1f, 0f);

        Primitive("CartBase", PrimitiveType.Cube, new Vector3(0f, 0f, 0f), new Vector3(5.5f, 0.25f, 4.8f), red, cart.transform);
        Primitive("CartBack", PrimitiveType.Cube, new Vector3(-2.6f, 1.25f, 0f), new Vector3(0.25f, 2.7f, 4.8f), red, cart.transform);
        Primitive("CartSideL", PrimitiveType.Cube, new Vector3(0f, 0.85f, -2.3f), new Vector3(5.5f, 1.8f, 0.25f), metal, cart.transform);
        Primitive("CartSideR", PrimitiveType.Cube, new Vector3(0f, 0.85f, 2.3f), new Vector3(5.5f, 1.8f, 0.25f), metal, cart.transform);
        for (int i = -1; i <= 1; i += 2)
        for (int j = -1; j <= 1; j += 2)
            Primitive("Wheel", PrimitiveType.Cylinder, new Vector3(i * 1.9f, -0.45f, j * 1.8f),
                new Vector3(0.55f, 0.22f, 0.55f), metal, cart.transform, new Vector3(90f, 0f, 0f));
    }

    private static void BuildHoneySection(Transform parent, Material honey)
    {
        Vector3[] positions =
        {
            new(-17.5f, 0.2f, -2.7f), new(-15f, 0.2f, 1.7f), new(-12.5f, 0.2f, -1.2f),
            new(-10f, 0.2f, 3.1f), new(-8.2f, 0.2f, -3.2f)
        };
        Vector3[] scales =
        {
            new(3.4f, .14f, 2.1f), new(2.5f, .14f, 3.2f), new(3.8f, .14f, 2.3f),
            new(2.7f, .14f, 2.1f), new(3f, .14f, 2.8f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject zone = Primitive($"Honey_{i + 1}", PrimitiveType.Cylinder,
                positions[i], scales[i], honey, parent);
            zone.GetComponent<Collider>().isTrigger = true;
            zone.AddComponent<HoneyZone>();
        }
    }

    private static void BuildSinkSection(Transform gameplay, Transform decoration,
        Material metal, Material porcelain, Material soap, Material darkMetal)
    {
        GameObject sink = new("Sink");
        sink.transform.SetParent(decoration);
        sink.transform.position = new Vector3(5f, 0.3f, 0f);

        Primitive("SinkFloor", PrimitiveType.Cube, new Vector3(0f, -0.1f, 0f), new Vector3(15f, 0.35f, 10f), metal, sink.transform);
        Primitive("SinkLeft", PrimitiveType.Cube, new Vector3(0f, 1.1f, -5f), new Vector3(15f, 2.4f, 0.55f), metal, sink.transform);
        Primitive("SinkRight", PrimitiveType.Cube, new Vector3(0f, 1.1f, 5f), new Vector3(15f, 2.4f, 0.55f), metal, sink.transform);
        Primitive("SinkEnd", PrimitiveType.Cube, new Vector3(7.25f, 1.1f, 0f), new Vector3(0.55f, 2.4f, 10f), metal, sink.transform);

        GameObject soapZone = Primitive("SoapWater", PrimitiveType.Cube,
            new Vector3(5f, 0.18f, 0f), new Vector3(14f, 0.25f, 9f), soap, gameplay);
        soapZone.GetComponent<Collider>().isTrigger = true;
        soapZone.AddComponent<SoapZone>();

        // Fork-shaped jump pad at the lip of the sink.
        GameObject forkPad = new("ForkJumpPad");
        forkPad.transform.SetParent(gameplay);
        forkPad.transform.position = new Vector3(-3.8f, 0.45f, 0f);
        Primitive("Handle", PrimitiveType.Cube, Vector3.zero, new Vector3(5f, 0.25f, 1.1f), metal, forkPad.transform);
        for (int i = -1; i <= 1; i++)
            Primitive("Tine", PrimitiveType.Cube, new Vector3(2.7f, 0f, i * 0.52f), new Vector3(1.2f, 0.22f, 0.2f), metal, forkPad.transform);
        BoxCollider trigger = forkPad.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(6.5f, 1.2f, 2.2f);
        forkPad.AddComponent<ForkJumpPad>();

        Primitive("Plate", PrimitiveType.Cylinder, new Vector3(4f, 0.65f, -1.4f),
            new Vector3(3.8f, 0.3f, 3.8f), porcelain, decoration);
        Primitive("Bowl", PrimitiveType.Cylinder, new Vector3(8.5f, 0.85f, 2.5f),
            new Vector3(2.5f, 0.75f, 2.5f), porcelain, decoration);
        Primitive("Cup", PrimitiveType.Cylinder, new Vector3(1f, 1f, 2.7f),
            new Vector3(1.5f, 1.5f, 1.5f), darkMetal, decoration);
        Primitive("Sponge", PrimitiveType.Cube, new Vector3(7f, 0.75f, -3f),
            new Vector3(2.5f, 1f, 1.7f), soap, decoration, new Vector3(0f, 25f, 0f));
    }

    private static void BuildFanSection(Transform gameplay, Transform decoration, Material fan, Material metal)
    {
        GameObject fanRoot = new("TableFan");
        fanRoot.transform.SetParent(decoration);
        fanRoot.transform.position = new Vector3(19f, 3.2f, 5.7f);
        fanRoot.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        Primitive("Stand", PrimitiveType.Cylinder, new Vector3(0f, -2.2f, 0f), new Vector3(2.3f, 0.35f, 2.3f), fan, fanRoot.transform);
        Primitive("Neck", PrimitiveType.Cylinder, new Vector3(0f, -1.2f, 0f), new Vector3(0.35f, 1.7f, 0.35f), metal, fanRoot.transform);
        Primitive("Hub", PrimitiveType.Sphere, Vector3.zero, Vector3.one * 1.25f, fan, fanRoot.transform);
        for (int i = 0; i < 4; i++)
            Primitive("Blade", PrimitiveType.Cube, Vector3.zero, new Vector3(0.45f, 3.9f, 1f),
                fan, fanRoot.transform, new Vector3(i * 90f, 0f, 0f));

        GameObject wind = new("FanWindZone");
        wind.transform.SetParent(gameplay);
        wind.transform.position = new Vector3(22f, 1.5f, 0f);
        wind.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        BoxCollider zone = wind.AddComponent<BoxCollider>();
        zone.isTrigger = true;
        zone.size = new Vector3(11f, 4f, 16f);
        wind.AddComponent<FanZone>();

        // Narrow lane and bumpers make the side wind readable and recoverable.
        Primitive("FanLaneLeft", PrimitiveType.Cube, new Vector3(22f, 0.65f, -5.5f), new Vector3(11f, 1.3f, 0.45f), metal, decoration);
        Primitive("FanLaneRight", PrimitiveType.Cube, new Vector3(22f, 0.65f, 5.5f), new Vector3(11f, 1.3f, 0.45f), metal, decoration);
    }

    private static void BuildGoal(Transform parent, Material green, Material dark, Material light)
    {
        GameObject goal = new("GOAL");
        goal.transform.SetParent(parent);
        goal.transform.position = new Vector3(32f, 0.9f, 0f);
        BoxCollider trigger = goal.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(2f, 3f, 11f);
        goal.AddComponent<KitchenGoal>();

        Primitive("GoalArchLeft", PrimitiveType.Cube, new Vector3(0f, 2f, -5f), new Vector3(0.5f, 4f, 0.5f), green, goal.transform);
        Primitive("GoalArchRight", PrimitiveType.Cube, new Vector3(0f, 2f, 5f), new Vector3(0.5f, 4f, 0.5f), green, goal.transform);
        for (int i = 0; i < 8; i++)
            Primitive("GoalBanner", PrimitiveType.Cube, new Vector3(0f, 4f, -4.4f + i * 1.25f),
                new Vector3(0.35f, 0.8f, 1.25f), i % 2 == 0 ? dark : light, goal.transform);
    }

    private static void CreateCheckpoint(string name, Vector3 position, Vector3 size, Transform parent)
    {
        GameObject checkpoint = new(name);
        checkpoint.transform.SetParent(parent);
        checkpoint.transform.position = position;
        BoxCollider collider = checkpoint.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = size;
        checkpoint.AddComponent<Checkpoint>();
    }

    private static GameObject Primitive(string name, PrimitiveType type, Vector3 position,
        Vector3 scale, Material material, Transform parent, Vector3 rotation = default)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position;
        obj.transform.localEulerAngles = rotation;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        return obj;
    }

    private static Material Material(string name, Color color, float smoothness = 0.2f)
    {
        string path = $"{MaterialFolder}/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = color;
        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string name = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent))
            EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static void AddSceneToBuildSettings(string path)
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        foreach (EditorBuildSettingsScene scene in scenes)
        {
            if (scene.path == path)
                return;
        }

        ArrayUtility.Add(ref scenes, new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes;
    }
}
#endif
