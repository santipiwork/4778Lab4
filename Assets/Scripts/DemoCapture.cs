#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;

/// <summary>Opt-in recording driver for development builds. Plays through the same Input System as a human.</summary>
public sealed class DemoCapture : MonoBehaviour
{
    private Gamepad pad;
    private string output;
    private int frame;
    private float elapsed;
    private float deathTime = -1;
    private bool bossDefeated;
    private bool restarted;
    private GameManager manager;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Launch()
    {
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, "-demoCapture");
        if (index < 0 || index + 1 >= args.Length) return;
        var recorder = new GameObject("Automated gameplay recording").AddComponent<DemoCapture>();
        recorder.output = args[index + 1];
        DontDestroyOnLoad(recorder.gameObject);
    }

    private IEnumerator Start()
    {
        Directory.CreateDirectory(output);
        Application.runInBackground = true;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        UnityEngine.Random.InitState(4778);
        pad = InputSystem.AddDevice<Gamepad>();
        InputSystem.onBeforeUpdate += Drive;
        Time.captureFramerate = 30;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
        while (elapsed < 70 && !(restarted && elapsed > deathTime + 6))
        {
            yield return new WaitForEndOfFrame();
            // Hidden windows may skip their backbuffer. Submit an explicit camera render instead.
            var target = RenderTexture.GetTemporary(1280, 720, 24, RenderTextureFormat.ARGB32);
            Canvas.ForceUpdateCanvases();
            RenderPipeline.SubmitRenderRequest(Camera.main, new RenderPipeline.StandardRequest { destination = target });
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            var texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            texture.Apply();
            RenderTexture.active = previous;
            File.WriteAllBytes(Path.Combine(output, $"frame-{frame:D5}.png"), texture.EncodeToPNG());
            Destroy(texture);
            RenderTexture.ReleaseTemporary(target);
            frame++;
            elapsed += 1f / 30;
        }
        File.WriteAllText(Path.Combine(output, "capture.txt"),
            $"Automated input demonstration. Frames: {frame}. Boss defeated: {bossDefeated}. Death observed: {deathTime >= 0}. Restarted: {restarted}.");
        Application.Quit();
    }

    private void Drive()
    {
        if (pad == null) return;
        if (manager == null) manager = FindFirstObjectByType<GameManager>();
        if (manager == null || manager.Session == null) return;
        bossDefeated |= manager.Session.BossesDestroyed > 0;
        var state = new GamepadState();
        if (manager.Session.IsGameOver)
        {
            if (deathTime < 0) deathTime = elapsed;
            if (elapsed > deathTime + 2)
            {
                state = state.WithButton(GamepadButton.Start);
                restarted = true;
            }
        }
        else if (manager.Ship != null)
        {
            var ship = manager.Ship.transform.position;
            Meteor target = null;
            var best = float.MaxValue;
            foreach (var meteor in FindObjectsByType<Meteor>(FindObjectsSortMode.None))
            {
                var priority = Vector2.Distance(meteor.transform.position, ship);
                if (meteor.IsBoss) priority -= 40f;
                if (priority < best) { best = priority; target = meteor; }
            }
            var destination = target != null ? target.transform.position : new Vector3(Mathf.Sin(elapsed) * 2, 2, 0);
            var aim = destination - ship;
            var movement = new Vector2(Mathf.Clamp(aim.x * 2f, -1, 1), Mathf.Clamp((aim.y - 2.2f) * 2f, -1, 1));
            state.leftStick = Vector2.ClampMagnitude(movement, 1);
            if (target != null && aim.y > 0.8f && Mathf.Abs(aim.x) < 0.55f)
                state = state.WithButton(GamepadButton.South);
        }
        InputSystem.QueueStateEvent(pad, state);
    }

    private void OnDestroy()
    {
        InputSystem.onBeforeUpdate -= Drive;
        if (pad != null) InputSystem.RemoveDevice(pad);
        Time.captureFramerate = 0;
    }
}
#endif
