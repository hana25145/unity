#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SampleSceneGameBuilder
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string ModelFolder = "Assets/3d assets(made)";
    private const string MaterialFolder = "Assets/_Materials/KitchenCourse";

    [MenuItem("Tools/Kitchen Gimmicks/Upgrade Sample Scene Gameplay")]
    public static void UpgradeGameplay()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        PlayerBall player = Object.FindFirstObjectByType<PlayerBall>();
        if (player == null)
            throw new System.InvalidOperationException("SampleScene needs one PlayerBall before it can be upgraded.");

        GameObject previous = GameObject.Find("GAMEPLAY_ADDITIONS");
        if (previous != null)
            Object.DestroyImmediate(previous);
        foreach (KitchenGameManager manager in Object.FindObjectsByType<KitchenGameManager>(FindObjectsSortMode.None))
            Object.DestroyImmediate(manager.gameObject);

        NormalizeZones<SoapZone>();
        NormalizeZones<FanZone>();
        NormalizeZones<WaterZone>();

        GameObject additions = new("GAMEPLAY_ADDITIONS");
        CreateHoneyPuddleTriggers(additions.transform);
        GameObject managerObject = new("KitchenGameManager", typeof(KitchenGameManager));
        managerObject.transform.SetParent(additions.transform);
        managerObject.GetComponent<KitchenGameManager>().Configure(player);

        CreateCheckpoint("StartCheckpoint", new Vector3(7.5f, 0f, -2f), new Vector3(18f, 4f, 2f), additions.transform);
        CreateCheckpoint("SinkCheckpoint", new Vector3(3f, 0f, 38f), new Vector3(24f, 4f, 2f), additions.transform);
        CreateCheckpoint("FanCheckpoint", new Vector3(11f, 0f, 82f), new Vector3(28f, 4f, 2f), additions.transform);
        CreateFallHazard(additions.transform);
        CreateCollectibles(additions.transform);
        CreateGoal(additions.transform);

        MakeSampleSceneFirstInBuildSettings();
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));
        Debug.Log("SampleScene gameplay upgraded: manager, checkpoints, fall recovery, 8 ingredients, and goal added.");
    }

    [MenuItem("Tools/Kitchen Gimmicks/Validate Sample Scene Gameplay")]
    public static void ValidateGameplay()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        RequireSingle<PlayerBall>("player");
        RequireSingle<KitchenGameManager>("game manager");
        RequireSingle<KitchenGoal>("goal");

        IngredientCollectible[] ingredients = Object.FindObjectsByType<IngredientCollectible>(FindObjectsSortMode.None);
        if (ingredients.Length != 8)
            throw new System.InvalidOperationException($"Expected 8 ingredients, found {ingredients.Length}.");

        ValidateTriggers<Checkpoint>();
        ValidateTriggers<IngredientCollectible>();
        ValidateTriggers<KitchenGoal>();
        ValidateTriggers<WaterZone>();
        ValidateTriggers<HoneyZone>();
        ValidateTriggers<SoapZone>();
        ValidateTriggers<FanZone>();
        Debug.Log("SampleScene gameplay validation passed.");
    }

    [MenuItem("Tools/Kitchen Gimmicks/Audit Sample Scene")]
    public static void Audit()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        StringBuilder report = new();
        report.AppendLine($"SampleScene audit: {scene.rootCount} roots");

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Bounds bounds = default;
            bool hasBounds = false;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            report.AppendLine(hasBounds
                ? $"ROOT {root.name}: position={root.transform.position}, bounds={bounds.min}..{bounds.max}"
                : $"ROOT {root.name}: position={root.transform.position}");
        }

        foreach (PlayerBall player in Object.FindObjectsByType<PlayerBall>(FindObjectsSortMode.None))
            report.AppendLine($"PLAYER {player.name}: position={player.transform.position}");
        foreach (HoneyZone zone in Object.FindObjectsByType<HoneyZone>(FindObjectsSortMode.None))
        {
            Collider collider = zone.GetComponent<Collider>();
            Renderer renderer = zone.GetComponent<Renderer>();
            report.AppendLine($"HONEY {zone.name}: position={zone.transform.position}, " +
                $"trigger={(collider != null ? collider.bounds.ToString() : "none")}, " +
                $"visual={(renderer != null ? renderer.bounds.ToString() : "none")}");
        }
        foreach (SoapZone zone in Object.FindObjectsByType<SoapZone>(FindObjectsSortMode.None))
            report.AppendLine($"SOAP {zone.name}: position={zone.transform.position}");
        foreach (FanZone zone in Object.FindObjectsByType<FanZone>(FindObjectsSortMode.None))
            report.AppendLine($"FAN {zone.name}: position={zone.transform.position}");
        foreach (WaterZone zone in Object.FindObjectsByType<WaterZone>(FindObjectsSortMode.None))
            report.AppendLine($"WATER {zone.name}: position={zone.transform.position}");

        Debug.Log(report.ToString());
    }

    private static void CreateCollectibles(Transform parent)
    {
        string[] modelFiles =
        {
            "banana.obj", "tomato (1).obj", "egg.obj", "green_onion.obj",
            "peach.obj", "plum.obj", "tomato (1).obj", "banana.obj"
        };
        string[] materials =
        {
            "BananaModel", "TomatoModel", "EggModel", "GreenOnionModel",
            "PeachModel", "PlumModel", "TomatoModel", "BananaModel"
        };
        Vector3[] positions =
        {
            new(7.5f, .7f, 3f), new(14.5f, .7f, 14f), new(-5f, .7f, 27f),
            new(3f, .8f, 39f), new(26f, .8f, 51f), new(7f, .8f, 68f),
            new(11f, .8f, 91f), new(7.5f, .8f, 106f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject collectible = new($"Ingredient_{i + 1}", typeof(SphereCollider), typeof(IngredientCollectible));
            collectible.transform.SetParent(parent, false);
            collectible.transform.position = positions[i];
            SphereCollider collider = collectible.GetComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = .9f;

            Material material = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialFolder}/{materials[i]}.mat");
            CreateModel(modelFiles[i], collectible.transform, material, Vector3.one * .85f);
        }
    }

    private static void CreateHoneyPuddleTriggers(Transform parent)
    {
        foreach (HoneyZone oldZone in Object.FindObjectsByType<HoneyZone>(FindObjectsSortMode.None))
        {
            Collider oldCollider = oldZone.GetComponent<Collider>();
            Object.DestroyImmediate(oldZone);
            if (oldCollider != null)
                Object.DestroyImmediate(oldCollider);
        }

        GameObject root = new("HoneyPuddleTriggers");
        root.transform.SetParent(parent, false);
        Vector3[] positions =
        {
            new(-3.2f, -1.35f, 11.2f),
            new(16.5f, -1.35f, 10.7f),
            new(34.8f, -1.35f, 13.0f)
        };
        Vector3[] sizes =
        {
            new(15f, .8f, 7.5f),
            new(15f, .8f, 6.5f),
            new(14f, .8f, 7.5f)
        };
        float[] rotations = { -8f, 5f, -7f };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject puddle = new($"HoneyPuddle_{i + 1}", typeof(BoxCollider), typeof(HoneyZone));
            puddle.transform.SetParent(root.transform, false);
            puddle.transform.position = positions[i];
            puddle.transform.rotation = Quaternion.Euler(0f, rotations[i], 0f);
            BoxCollider collider = puddle.GetComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = sizes[i];
        }
    }

    private static void NormalizeZones<T>() where T : Component
    {
        foreach (T zone in Object.FindObjectsByType<T>(FindObjectsSortMode.None))
        {
            Collider collider = zone.GetComponent<Collider>();
            if (collider is MeshCollider)
            {
                GameObject owner = zone.gameObject;
                string settings = EditorJsonUtility.ToJson(zone);
                MeshFilter filter = zone.GetComponent<MeshFilter>();
                Bounds bounds = filter != null && filter.sharedMesh != null
                    ? filter.sharedMesh.bounds
                    : new Bounds(Vector3.zero, Vector3.one);
                Object.DestroyImmediate(zone);
                Object.DestroyImmediate(collider);
                BoxCollider box = owner.AddComponent<BoxCollider>();
                box.center = bounds.center;
                Vector3 size = bounds.size;
                size.x = Mathf.Max(size.x, .2f);
                size.y = Mathf.Max(size.y, .2f);
                size.z = Mathf.Max(size.z, .2f);
                box.size = size;
                collider = box;
                T replacement = (T)owner.AddComponent(typeof(T));
                EditorJsonUtility.FromJsonOverwrite(settings, replacement);
            }
            else if (collider == null)
            {
                collider = zone.gameObject.AddComponent<BoxCollider>();
            }
            collider.isTrigger = true;
        }
    }

    private static void CreateModel(string fileName, Transform parent, Material material, Vector3 scale)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>($"{ModelFolder}/{fileName}");
        if (asset == null)
            throw new System.IO.FileNotFoundException($"Missing kitchen model: {fileName}");

        GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
        model.name = "Visual";
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = scale;
        foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
        {
            Material[] replacements = new Material[Mathf.Max(1, renderer.sharedMaterials.Length)];
            for (int i = 0; i < replacements.Length; i++)
                replacements[i] = material;
            renderer.sharedMaterials = replacements;
        }
    }

    private static void CreateGoal(Transform parent)
    {
        Material green = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialFolder}/GoalGreen.mat");
        Material light = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialFolder}/Porcelain.mat");
        GameObject goal = new("GOAL", typeof(BoxCollider), typeof(KitchenGoal));
        goal.transform.SetParent(parent, false);
        goal.transform.position = new Vector3(7.5f, 1f, 115f);
        BoxCollider trigger = goal.GetComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(22f, 5f, 2.5f);

        Primitive("GoalLeft", new Vector3(-10f, 1.5f, 0f), new Vector3(.7f, 5f, .7f), green, goal.transform);
        Primitive("GoalRight", new Vector3(10f, 1.5f, 0f), new Vector3(.7f, 5f, .7f), green, goal.transform);
        Primitive("GoalTop", new Vector3(0f, 4f, 0f), new Vector3(20.7f, .8f, .7f), light, goal.transform);
    }

    private static void CreateFallHazard(Transform parent)
    {
        GameObject hazard = new("FallHazard", typeof(BoxCollider), typeof(WaterZone));
        hazard.transform.SetParent(parent, false);
        hazard.transform.position = new Vector3(5f, -12f, 53f);
        BoxCollider collider = hazard.GetComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(130f, 2f, 170f);
    }

    private static void CreateCheckpoint(string name, Vector3 position, Vector3 size, Transform parent)
    {
        GameObject checkpoint = new(name, typeof(BoxCollider), typeof(Checkpoint));
        checkpoint.transform.SetParent(parent, false);
        checkpoint.transform.position = position;
        BoxCollider collider = checkpoint.GetComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = size;
    }

    private static void Primitive(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
        primitive.name = name;
        primitive.transform.SetParent(parent, false);
        primitive.transform.localPosition = position;
        primitive.transform.localScale = scale;
        primitive.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void MakeSampleSceneFirstInBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new() { new EditorBuildSettingsScene(ScenePath, true) };
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.path != ScenePath)
                scenes.Add(scene);
        }
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void RequireSingle<T>(string label) where T : Object
    {
        T[] objects = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        if (objects.Length != 1)
            throw new System.InvalidOperationException($"Expected one {label}, found {objects.Length}.");
    }

    private static void ValidateTriggers<T>() where T : Component
    {
        T[] components = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        if (components.Length == 0)
            throw new System.InvalidOperationException($"No {typeof(T).Name} components found.");
        foreach (T component in components)
        {
            Collider collider = component.GetComponent<Collider>();
            if (collider == null || !collider.isTrigger)
                throw new System.InvalidOperationException($"{component.name} has an invalid {typeof(T).Name} trigger.");
        }
    }
}
#endif
