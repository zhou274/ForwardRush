#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CreateCampfireEffect
{
    private const string EFFECT_FOLDER = "Assets/GameRes/Effect";
    private const string MATERIAL_FOLDER = EFFECT_FOLDER + "/Materials";
    private const string PREFAB_PATH = EFFECT_FOLDER + "/Campfire.prefab";
    private const string SORTING_LAYER = "Player";
    private const int SORTING_ORDER_BASE = 0;

    [MenuItem("Tools/Create Campfire Effect")]
    public static void Create()
    {
        EnsureFolders();

        var fireMat = CreateAdditiveMaterial("FireAdditive");
        var smokeMat = CreateAlphaMaterial("SmokeAlpha");
        if (fireMat == null || smokeMat == null)
        {
            Debug.LogError("[Campfire] Failed to create materials. Ensure particle shaders are included in build settings (GraphicsSettings → Always Included Shaders).");
            return;
        }

        var root = new GameObject("Campfire");
        root.transform.position = Vector3.zero;

        CreateLight(root);

        var fireCore = CreateParticleSystem("FireCore", fireMat, 10, ConfigureFireCore);
        fireCore.transform.SetParent(root.transform);
        fireCore.transform.localPosition = Vector3.zero;

        var fireOuter = CreateParticleSystem("FireOuter", fireMat, 5, ConfigureFireOuter);
        fireOuter.transform.SetParent(root.transform);
        fireOuter.transform.localPosition = Vector3.zero;

        var smoke = CreateParticleSystem("Smoke", smokeMat, 15, ConfigureSmoke);
        smoke.transform.SetParent(root.transform);
        smoke.transform.localPosition = Vector3.zero;

        var sparks = CreateParticleSystem("Sparks", fireMat, 20, ConfigureSparks);
        sparks.transform.SetParent(root.transform);
        sparks.transform.localPosition = Vector3.zero;

        PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
        Object.DestroyImmediate(root);

        AssetDatabase.Refresh();
        Debug.Log("[Campfire] Created: " + PREFAB_PATH);
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/GameRes"))
            AssetDatabase.CreateFolder("Assets", "GameRes");
        if (!AssetDatabase.IsValidFolder(EFFECT_FOLDER))
            AssetDatabase.CreateFolder("Assets/GameRes", "Effect");
        if (!AssetDatabase.IsValidFolder(MATERIAL_FOLDER))
            AssetDatabase.CreateFolder(EFFECT_FOLDER, "Materials");
    }

    private static Shader FindShader(params string[] names)
    {
        foreach (var name in names)
        {
            var shader = Shader.Find(name);
            if (shader != null)
                return shader;
        }
        Debug.LogError("[Campfire] No valid shader found. Tried: " + string.Join(", ", names));
        return null;
    }

    private static Material CreateAdditiveMaterial(string name)
    {
        var path = MATERIAL_FOLDER + "/" + name + ".mat";
        var shader = FindShader("Particles/Additive", "Mobile/Particles/Additive",
            "Legacy Shaders/Particles/Additive", "Universal Render Pipeline/Particles/Unlit",
            "Hidden/Internal-ParticleAdd");
        if (shader == null) return null;
        var mat = new Material(shader);
        AssetDatabase.CreateAsset(mat, path);
        return AssetDatabase.LoadAssetAtPath<Material>(path);
    }

    private static Material CreateAlphaMaterial(string name)
    {
        var path = MATERIAL_FOLDER + "/" + name + ".mat";
        var shader = FindShader("Particles/Alpha Blended", "Mobile/Particles/Alpha Blended",
            "Legacy Shaders/Particles/Alpha Blended", "Universal Render Pipeline/Particles/Unlit",
            "Hidden/Internal-TransparentColored");
        if (shader == null) return null;
        var mat = new Material(shader);
        AssetDatabase.CreateAsset(mat, path);
        return AssetDatabase.LoadAssetAtPath<Material>(path);
    }

    private static GameObject CreateParticleSystem(string name, Material material, int sortingOrder, System.Action<ParticleSystem> configure)
    {
        var go = new GameObject(name);
        var ps = go.AddComponent<ParticleSystem>();
        configure(ps);

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = material;
        renderer.sortingLayerName = SORTING_LAYER;
        renderer.sortingOrder = SORTING_ORDER_BASE + sortingOrder;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        return go;
    }

    private static void CreateLight(GameObject root)
    {
        var go = new GameObject("FireGlow");
        go.transform.SetParent(root.transform);
        go.transform.localPosition = new Vector3(0, 0.3f, 0);

        var lt = go.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(1f, 0.5f, 0.05f);
        lt.intensity = 0.7f;
        lt.range = 8f;
        lt.renderMode = LightRenderMode.ForcePixel;

        go.AddComponent<LightFlicker>();
    }

    private static void ConfigureFireCore(ParticleSystem ps)
    {
        var main = ps.main;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.9f, 0.2f),
            new Color(1f, 1f, 0.6f)
        );
        main.maxParticles = 60;

        var emission = ps.emission;
        emission.rateOverTime = 50f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 8f;
        shape.radius = 0.08f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var colGrad = new Gradient();
        colGrad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.95f, 0.3f), 0f),
                new GradientColorKey(new Color(1f, 0.5f, 0f), 0.4f),
                new GradientColorKey(new Color(1f, 0.1f, 0f), 0.8f),
                new GradientColorKey(new Color(0f, 0f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.3f),
                new GradientAlphaKey(0.2f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(colGrad);

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        var sizeCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.5f, 0.8f),
            new Keyframe(1f, 0.2f)
        );
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var rotationOverLifetime = ps.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-45f, 45f);
    }

    private static void ConfigureFireOuter(ParticleSystem ps)
    {
        var main = ps.main;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.1f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.4f, 0f),
            new Color(1f, 0.6f, 0.1f)
        );
        main.maxParticles = 40;

        var emission = ps.emission;
        emission.rateOverTime = 25f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 14f;
        shape.radius = 0.12f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var colGrad = new Gradient();
        colGrad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.55f, 0f), 0f),
                new GradientColorKey(new Color(1f, 0.2f, 0f), 0.5f),
                new GradientColorKey(new Color(0.2f, 0f, 0f), 0.85f),
                new GradientColorKey(new Color(0f, 0f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0.6f, 0.3f),
                new GradientAlphaKey(0.1f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(colGrad);

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        var sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.4f),
            new Keyframe(0.3f, 1f),
            new Keyframe(0.7f, 0.7f),
            new Keyframe(1f, 0.1f)
        );
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var rotationOverLifetime = ps.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-30f, 30f);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = new ParticleSystem.MinMaxCurve(0.15f);
        noise.frequency = 0.6f;
        noise.scrollSpeed = 0.4f;
        noise.positionAmount = new ParticleSystem.MinMaxCurve(0.08f);
    }

    private static void ConfigureSmoke(ParticleSystem ps)
    {
        var main = ps.main;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.4f, 0.4f, 0.4f, 0.3f),
            new Color(0.6f, 0.6f, 0.6f, 0.5f)
        );
        main.gravityModifier = -0.05f;
        main.maxParticles = 20;

        var emission = ps.emission;
        emission.rateOverTime = 6f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 12f;
        shape.radius = 0.15f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var colGrad = new Gradient();
        colGrad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.35f, 0.35f, 0.35f), 0f),
                new GradientColorKey(new Color(0.5f, 0.5f, 0.5f), 0.15f),
                new GradientColorKey(new Color(0.7f, 0.7f, 0.7f), 0.5f),
                new GradientColorKey(new Color(1f, 1f, 1f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.25f, 0.15f),
                new GradientAlphaKey(0.12f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(colGrad);

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        var sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.3f),
            new Keyframe(0.2f, 1f),
            new Keyframe(0.6f, 2.5f),
            new Keyframe(1f, 4f)
        );
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = new ParticleSystem.MinMaxCurve(0.3f);
        noise.frequency = 0.3f;
        noise.scrollSpeed = 0.15f;
    }

    private static void ConfigureSparks(ParticleSystem ps)
    {
        var main = ps.main;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.9f, 0.2f),
            new Color(1f, 1f, 0.6f)
        );
        main.gravityModifier = 0.3f;
        main.maxParticles = 25;

        var emission = ps.emission;
        emission.rateOverTime = 12f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 10f;
        shape.radius = 0.08f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var colGrad = new Gradient();
        colGrad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.95f, 0.3f), 0f),
                new GradientColorKey(new Color(1f, 0.5f, 0f), 0.4f),
                new GradientColorKey(new Color(0.5f, 0.05f, 0f), 0.8f),
                new GradientColorKey(new Color(0.1f, 0.1f, 0.1f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.3f),
                new GradientAlphaKey(0.2f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(colGrad);

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        var sizeCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.5f, 0.8f),
            new Keyframe(1f, 0f)
        );
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var rotationOverLifetime = ps.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-60f, 60f);
    }
}
#endif
