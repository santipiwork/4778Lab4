using System.IO;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

/// <summary>Repeatable authoring tool. All resulting references are saved in ordinary scene/prefab assets.</summary>
public static class ShooterSetup
{
    [MenuItem("Tools/Space Shooter/Configure Project")]
    public static void Configure()
    {
        ConfigurePrefab("Player", false);
        ConfigurePrefab("Meteor", false);
        ConfigurePrefab("BigMeteor", true);
        ConfigurePrefab("Laser", false);
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Week5Lab.unity");
        var actions = CreateInput();
        var manager = Object.FindFirstObjectByType<GameManager>();
        var input = Ensure<ShooterInput>(manager.gameObject);
        SetObject(input, "actions", actions);
        Ensure<MeteorSpawner>(manager.gameObject);
        Ensure<GameHud>(manager.gameObject);
        Ensure<SpaceBackdrop>(manager.gameObject);
        var camera = Camera.main;
        Ensure<UniversalAdditionalCameraData>(camera.gameObject);
        camera.orthographic = true;
        camera.orthographicSize = 8;
        camera.backgroundColor = new Color(0.012f, 0.022f, 0.05f);
        Ensure<CinemachineBrain>(camera.gameObject);
        var rig = Object.FindFirstObjectByType<ShooterCamera>();
        if (rig == null) rig = new GameObject("Ship Tracking Camera").AddComponent<ShooterCamera>();
        var cm = rig.GetComponent<CinemachineCamera>();
        cm.transform.position = new Vector3(0, 1, -10);
        cm.Lens.OrthographicSize = 8;
        cm.Lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
        var composer = rig.GetComponent<CinemachinePositionComposer>();
        composer.CameraDistance = 10;
        composer.TargetOffset = new Vector3(0, 4, 0);
        composer.Damping = new Vector3(0.8f, 1f, 0);
        composer.Composition.DeadZone.Enabled = false;
        var noise = rig.GetComponent<CinemachineBasicMultiChannelPerlin>();
        const string noisePath = "Assets/Settings/DestructionNoise.asset";
        var profile = AssetDatabase.LoadAssetAtPath<NoiseSettings>(noisePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<NoiseSettings>();
            AssetDatabase.CreateAsset(profile, noisePath);
        }
        profile.PositionNoise = new[] { new NoiseSettings.TransformNoiseParams {
            X = new NoiseSettings.NoiseParams { Amplitude = 0.5f, Frequency = 18 },
            Y = new NoiseSettings.NoiseParams { Amplitude = 0.5f, Frequency = 22 } } };
        profile.OrientationNoise = new[] { new NoiseSettings.TransformNoiseParams {
            Z = new NoiseSettings.NoiseParams { Amplitude = 0.6f, Frequency = 15 } } };
        EditorUtility.SetDirty(profile);
        noise.NoiseProfile = profile;
        noise.AmplitudeGain = 0;
        noise.FrequencyGain = 1;
        SetObject(manager, "cameraRig", rig);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scene.path, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("SHOOTER_SETUP_COMPLETE");
    }

    private static void ConfigurePrefab(string name, bool boss)
    {
        var path = $"Assets/Prefabs/{name}.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        var body = root.GetComponent<Rigidbody2D>();
        body.gravityScale = 0;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        if (name == "Player") { Ensure<ShipMotor>(root); Ensure<ShipWeapon>(root); }
        if (name == "Meteor" || boss)
        {
            var legacy = root.GetComponent<FallingMeteorMovement>();
            if (legacy != null) Object.DestroyImmediate(legacy);
            var movement = Ensure<OrbitMeteorMovement>(root);
            var serializedMotion = new SerializedObject(movement);
            serializedMotion.FindProperty("solution").enumValueIndex = boss ? 1 : 0;
            serializedMotion.FindProperty("preferredRadius").floatValue = boss ? 7.5f : 5f;
            serializedMotion.FindProperty("radiusCatchUp").floatValue = boss ? 2f : 3.5f;
            serializedMotion.FindProperty("approachSpeed").floatValue = boss ? 0.35f : 0.7f;
            serializedMotion.FindProperty("orbitSign").floatValue = boss ? -1f : 1f;
            serializedMotion.FindProperty("nearSpeed").floatValue = boss ? 2.6f : 6.5f;
            serializedMotion.FindProperty("farSpeed").floatValue = boss ? 1.2f : 2.2f;
            serializedMotion.FindProperty("nearDistance").floatValue = 2.5f;
            serializedMotion.FindProperty("farDistance").floatValue = 10f;
            serializedMotion.FindProperty("turnLerp").floatValue = boss ? 6f : 10f;
            serializedMotion.ApplyModifiedPropertiesWithoutUndo();
            var meteor = new SerializedObject(root.GetComponent<Meteor>());
            meteor.FindProperty("hitPoints").intValue = boss ? 5 : 1;
            meteor.FindProperty("isBoss").boolValue = boss;
            meteor.ApplyModifiedPropertiesWithoutUndo();
        }
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static InputActionAsset CreateInput()
    {
        const string path = "Assets/Settings/ShooterControls.inputactions";
        var asset = ScriptableObject.CreateInstance<InputActionAsset>();
        var map = asset.AddActionMap("Gameplay");
        var move = map.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
        move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
        move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        move.AddBinding("<Gamepad>/leftStick");
        move.AddBinding("<Gamepad>/dpad");
        var fire = map.AddAction("Fire", InputActionType.Button);
        fire.AddBinding("<Keyboard>/space");
        fire.AddBinding("<Gamepad>/buttonSouth");
        fire.AddBinding("<Gamepad>/rightTrigger");
        var restart = map.AddAction("Restart", InputActionType.Button);
        restart.AddBinding("<Keyboard>/r");
        restart.AddBinding("<Gamepad>/start");
        File.WriteAllText(path, asset.ToJson());
        Object.DestroyImmediate(asset);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        var imported = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
        if (imported == null) throw new System.InvalidOperationException("ShooterControls failed to import.");
        return imported;
    }

    private static T Ensure<T>(GameObject target) where T : Component
        => target.GetComponent<T>() ?? target.AddComponent<T>();
    private static void SetObject(Object target, string field, Object value)
    {
        if (value == null) throw new System.InvalidOperationException($"Cannot assign null to {target.name}.{field}");
        var serialized = new SerializedObject(target);
        serialized.FindProperty(field).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
