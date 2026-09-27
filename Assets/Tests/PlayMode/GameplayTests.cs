#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class GameplayTests
{
    private GameManager manager;
    private Keyboard keyboard;
    private InputSettings.EditorInputBehaviorInPlayMode previousEditorBehavior;
    private InputSettings.BackgroundBehavior previousBackgroundBehavior;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Headless test runners have no focused Game view. Route synthetic device events to the game.
        previousEditorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
        previousBackgroundBehavior = InputSystem.settings.backgroundBehavior;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        keyboard = InputSystem.AddDevice<Keyboard>();
        yield return SceneManager.LoadSceneAsync("Week5Lab");
        yield return null;
        manager = Object.FindFirstObjectByType<GameManager>();
        manager.GetComponent<MeteorSpawner>().enabled = false;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        InputSystem.RemoveDevice(keyboard);
        InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorBehavior;
        InputSystem.settings.backgroundBehavior = previousBackgroundBehavior;
        yield return null;
    }

    [UnityTest]
    public IEnumerator NewInputMovesFiresAndCameraTracksShip()
    {
        var ship = manager.Ship;
        var start = ship.transform.position;
        var cameraStart = Camera.main.transform.position;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D, Key.W, Key.Space));
        yield return new WaitForSeconds(0.6f);
        Assert.That(ship.transform.position.x, Is.GreaterThan(start.x + 1));
        Assert.That(ship.transform.position.y, Is.GreaterThan(start.y + 1));
        Assert.That(Object.FindObjectsByType<Laser>(FindObjectsSortMode.None).Length, Is.GreaterThan(0));
        Assert.That(manager.GetComponent<ShooterInput>().Move.magnitude, Is.EqualTo(1).Within(0.01f));
        Assert.That(Camera.main.transform.position.x, Is.GreaterThan(cameraStart.x + 0.1f));
        var cm = Object.FindFirstObjectByType<CinemachineCamera>();
        Assert.That(cm.Follow, Is.EqualTo(ship.transform));
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
    }

    [UnityTest]
    public IEnumerator FiveKillsSpawnBossFiveProjectilesDestroyItAndZoomReturns()
    {
        var noise = Object.FindFirstObjectByType<CinemachineBasicMultiChannelPerlin>();
        var cm = Object.FindFirstObjectByType<CinemachineCamera>();
        for (var i = 0; i < 5; i++)
        {
            var meteor = SpawnMeteor("Meteor", new Vector3(0, 3, 0));
            SpawnLaser(meteor.transform.position);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
        }
        yield return null;
        Assert.That(manager.Session.DestroyedMeteors, Is.EqualTo(5));
        Assert.That(manager.Session.ActiveBosses, Is.EqualTo(1));
        Assert.That(noise.AmplitudeGain, Is.GreaterThan(0));
        var boss = Object.FindFirstObjectByType<BigMeteor>();
        Assert.That(boss.RemainingHits, Is.EqualTo(5));
        yield return new WaitForSeconds(0.7f);
        Assert.That(cm.Lens.OrthographicSize, Is.GreaterThan(9));
        Assert.That(noise.AmplitudeGain, Is.Zero);
        for (var i = 0; i < 4; i++)
        {
            SpawnLaser(boss.transform.position);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(boss.RemainingHits, Is.EqualTo(4 - i));
        }
        SpawnLaser(boss.transform.position);
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        Assert.That(manager.Session.BossesDestroyed, Is.EqualTo(1));
        Assert.That(manager.Session.ActiveBosses, Is.Zero);
        Assert.That(manager.Session.DestroyedMeteors, Is.EqualTo(5));
        yield return new WaitForSeconds(1);
        Assert.That(cm.Lens.OrthographicSize, Is.LessThan(8.2f));
    }

    [UnityTest]
    public IEnumerator OneProjectileCannotKillTwoOverlappingMeteors()
    {
        SpawnMeteor("Meteor", Vector3.up * 3);
        SpawnMeteor("Meteor", Vector3.up * 3);
        SpawnLaser(Vector3.up * 3);
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        Assert.That(manager.Session.DestroyedMeteors, Is.EqualTo(1));
    }

    [UnityTest]
    public IEnumerator CollisionEndsOnlyLifeStopsSpawningAndRestartResetsSession()
    {
        manager.GetComponent<MeteorSpawner>().enabled = true;
        var survivor = SpawnMeteor("Meteor", new Vector3(6, 5, 0));
        SpawnMeteor("Meteor", manager.Ship.transform.position);
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        Assert.That(manager.Session.IsGameOver, Is.True);
        var count = Object.FindObjectsByType<Meteor>(FindObjectsSortMode.None).Length;
        yield return new WaitForFixedUpdate();
        var frozenPosition = survivor.GetComponent<Rigidbody2D>().position;
        yield return new WaitForSeconds(2.2f);
        Assert.That(Object.FindObjectsByType<Meteor>(FindObjectsSortMode.None).Length, Is.EqualTo(count));
        Assert.That(Vector2.Distance(survivor.GetComponent<Rigidbody2D>().position, frozenPosition), Is.LessThan(0.01f));
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R));
        yield return new WaitForSeconds(0.3f);
        var restarted = Object.FindFirstObjectByType<GameManager>();
        Assert.That(restarted, Is.Not.EqualTo(manager));
        Assert.That(restarted.Session.IsGameOver, Is.False);
        Assert.That(restarted.Session.DestroyedMeteors, Is.Zero);
        Assert.That(restarted.Session.ActiveBosses, Is.Zero);
        Assert.That(restarted.Ship, Is.Not.Null);
    }

    private Meteor SpawnMeteor(string name, Vector3 position)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/{name}.prefab");
        var meteor = Object.Instantiate(prefab, position, Quaternion.identity).GetComponent<Meteor>();
        meteor.Initialize(manager.Session);
        meteor.GetComponent<MeteorMovement>().Initialize(manager.Session, manager.Ship.transform);
        return meteor;
    }
    private void SpawnLaser(Vector3 position)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Laser.prefab");
        Object.Instantiate(prefab, position, Quaternion.identity).GetComponent<Laser>().Initialize(manager.Session);
    }
}
#endif
