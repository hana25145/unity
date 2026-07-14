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

        ConfigureSoapTextureMask();
        NormalizeZones<FanZone>();
        NormalizeZones<WaterZone>();
        ConfigureSampleFan();

        GameObject additions = new("GAMEPLAY_ADDITIONS");
        ConfigureHoneyTextureMask();
        CreateForkJumpTrigger(additions.transform);
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
        ValidateTriggers<ForkJumpPad>();
        HoneyZone[] honeyZones = Object.FindObjectsByType<HoneyZone>(FindObjectsSortMode.None);
        if (honeyZones.Length != 1)
            throw new System.InvalidOperationException($"Expected one texture-masked honey zone, found {honeyZones.Length}.");
        SerializedObject honeySettings = new(honeyZones[0]);
        Renderer mask = honeySettings.FindProperty("maskRenderer").objectReferenceValue as Renderer;
        Texture2D maskTexture = mask != null && mask.sharedMaterial != null
            ? mask.sharedMaterial.mainTexture as Texture2D
            : null;
        if (maskTexture == null || !maskTexture.isReadable)
            throw new System.InvalidOperationException("Honey texture mask is missing or not readable.");
        SoapZone soap = Object.FindFirstObjectByType<SoapZone>();
        SerializedObject soapSettings = new(soap);
        Renderer soapMask = soapSettings.FindProperty("maskRenderer").objectReferenceValue as Renderer;
        Texture2D soapTexture = soapMask != null && soapMask.sharedMaterial != null
            ? soapMask.sharedMaterial.mainTexture as Texture2D
            : null;
        float soapDamping = soapSettings.FindProperty("slipperyDamping").floatValue;
        if (soapTexture == null || !soapTexture.isReadable || soapDamping >= .25f)
            throw new System.InvalidOperationException("Soap texture mask or slippery damping is invalid.");
        FanZone fan = Object.FindFirstObjectByType<FanZone>();
        SerializedObject fanSettings = new(fan);
        Vector3 windDirection = fanSettings.FindProperty("worldWindDirection").vector3Value;
        float windAcceleration = fanSettings.FindProperty("windAcceleration").floatValue;
        if (Vector3.Dot(windDirection.normalized, Vector3.left) < .99f || windAcceleration > 20f)
            throw new System.InvalidOperationException("SampleScene fan must use a moderate leftward crosswind.");
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
        {
            Collider collider = zone.GetComponent<Collider>();
            report.AppendLine($"FAN {zone.name}: position={zone.transform.position}, direction={zone.transform.forward}, " +
                $"bounds={(collider != null ? collider.bounds.ToString() : "none")}");
        }
        foreach (WaterZone zone in Object.FindObjectsByType<WaterZone>(FindObjectsSortMode.None))
            report.AppendLine($"WATER {zone.name}: position={zone.transform.position}");
        GameObject fork = GameObject.Find("fork");
        if (fork != null)
        {
            Renderer forkRenderer = fork.GetComponentInChildren<Renderer>();
            report.AppendLine(forkRenderer != null
                ? $"FORK {fork.name}: position={fork.transform.position}, bounds={forkRenderer.bounds}"
                : $"FORK {fork.name}: position={fork.transform.position}");
        }
        foreach (BoxCollider collider in Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None))
        {
            if (!collider.isTrigger && collider.bounds.max.z > 110f)
                report.AppendLine($"END COLLIDER {collider.name}: bounds={collider.bounds}");
        }

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

    private static void ConfigureSampleFan()
    {
        FanZone[] fans = Object.FindObjectsByType<FanZone>(FindObjectsSortMode.None);
        if (fans.Length != 1)
            throw new System.InvalidOperationException($"Expected one SampleScene fan zone, found {fans.Length}.");
        fans[0].Configure(Vector3.left, 18f);
    }

    private static void ConfigureSoapTextureMask()
    {
        SoapZone[] zones = Object.FindObjectsByType<SoapZone>(FindObjectsSortMode.None);
        if (zones.Length != 1)
            throw new System.InvalidOperationException($"Expected one SampleScene soap zone, found {zones.Length}.");

        GameObject soap = zones[0].gameObject;
        Collider oldCollider = zones[0].GetComponent<Collider>();
        Object.DestroyImmediate(zones[0]);
        if (oldCollider != null)
            Object.DestroyImmediate(oldCollider);

        MeshRenderer renderer = soap.GetComponent<MeshRenderer>();
        MeshFilter filter = soap.GetComponent<MeshFilter>();
        if (renderer == null || filter == null || filter.sharedMesh == null)
            throw new System.InvalidOperationException("Soap_zone needs a MeshRenderer and MeshFilter.");

        Texture texture = renderer.sharedMaterial != null ? renderer.sharedMaterial.mainTexture : null;
        TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        Bounds bounds = filter.sharedMesh.bounds;
        BoxCollider trigger = soap.AddComponent<BoxCollider>();
        trigger.center = bounds.center;
        trigger.size = bounds.size;
        trigger.isTrigger = true;
        SoapZone zone = soap.AddComponent<SoapZone>();
        zone.Configure(.6f, 1f, .05f);
        zone.ConfigureMask(renderer, .15f, 0, 2);
    }

    private static void CreateForkJumpTrigger(Transform parent)
    {
        GameObject fork = GameObject.Find("fork");
        Renderer renderer = fork != null ? fork.GetComponentInChildren<Renderer>() : null;
        if (fork == null || renderer == null)
            throw new System.InvalidOperationException("Could not find the SampleScene fork model.");

        Bounds bounds = renderer.bounds;
        GameObject trigger = new("ForkJumpTrigger", typeof(BoxCollider), typeof(ForkJumpPad));
        trigger.transform.SetParent(parent, false);
        trigger.transform.position = bounds.center + Vector3.up * .45f;
        trigger.transform.rotation = Quaternion.identity;
        BoxCollider collider = trigger.GetComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(bounds.size.x + .8f, 2f, bounds.size.z * .92f);
        trigger.GetComponent<ForkJumpPad>().Configure(8.5f, 8f, .45f);
    }

    private static void ConfigureHoneyTextureMask()
    {
        foreach (HoneyZone oldZone in Object.FindObjectsByType<HoneyZone>(FindObjectsSortMode.None))
        {
            Collider oldCollider = oldZone.GetComponent<Collider>();
            Object.DestroyImmediate(oldZone);
            if (oldCollider != null)
                Object.DestroyImmediate(oldCollider);
        }

        GameObject honey = GameObject.Find("honey");
        if (honey == null)
            throw new System.InvalidOperationException("Could not find the honey_spill_wide scene object.");

        MeshRenderer renderer = honey.GetComponent<MeshRenderer>();
        MeshFilter filter = honey.GetComponent<MeshFilter>();
        if (renderer == null || filter == null || filter.sharedMesh == null)
            throw new System.InvalidOperationException("The honey object needs a MeshRenderer and MeshFilter.");

        Texture texture = renderer.sharedMaterial != null ? renderer.sharedMaterial.mainTexture : null;
        string texturePath = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        Bounds bounds = filter.sharedMesh.bounds;
        Vector3 size = bounds.size;
        size.x = Mathf.Max(size.x, .2f);
        size.y = Mathf.Max(size.y, .2f);
        size.z = Mathf.Max(size.z, .2f);
        BoxCollider trigger = honey.AddComponent<BoxCollider>();
        trigger.center = bounds.center;
        trigger.size = size;
        trigger.isTrigger = true;
        HoneyZone zone = honey.AddComponent<HoneyZone>();
        zone.Configure(.65f, .7f, 1.25f);
        zone.ConfigureMask(renderer, .15f);
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
        goal.transform.position = new Vector3(7.5f, 1f, 108f);
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
