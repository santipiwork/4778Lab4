using UnityEngine;
using UnityEngine.UI;

/// <summary>Read-only presentation; camera-space UI is also visible in offscreen gameplay recordings.</summary>
public sealed class GameHud : MonoBehaviour
{
    private IGameStatus status;
    private Text score;
    private Text warning;
    private GameObject gameOverPanel;

    public void Initialize(IGameStatus game)
    {
        status = game;
        var canvas = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler)).GetComponent<Canvas>();
        canvas.transform.SetParent(transform);
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        canvas.planeDistance = 1;
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 1;
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Label(canvas.transform, font, "METEOR WATCH", new Vector2(28, -18), new Vector2(500, 44), 28, true);
        score = Label(canvas.transform, font, "", new Vector2(30, -64), new Vector2(900, 30), 17);
        warning = Label(canvas.transform, font, "", new Vector2(30, -95), new Vector2(900, 30), 17);
        var controls = Label(canvas.transform, font,
            "WASD / ARROWS  Move     SPACE  Fire     GAMEPAD  Left stick + A / RT",
            new Vector2(30, 20), new Vector2(1100, 32), 17);
        controls.rectTransform.anchorMin = controls.rectTransform.anchorMax = new Vector2(0, 0);
        controls.rectTransform.pivot = Vector2.zero;

        var panel = new GameObject("Game over", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(500, 150);
        panel.GetComponent<Image>().color = new Color(0.02f, 0.04f, 0.09f, 0.94f);
        var message = Label(panel.transform, font, "SHIP LOST\n\nR / GAMEPAD START TO RESTART",
            new Vector2(0, 0), rect.sizeDelta, 24, true);
        message.alignment = TextAnchor.MiddleCenter;
        gameOverPanel = panel;
        panel.SetActive(false);
        Update();
    }

    private void Update()
    {
        if (status == null) return;
        score.text = $"METEORS  {status.DestroyedMeteors}    /    BOSSES  {status.BossesDestroyed}    /    LIFE  {(status.IsGameOver ? 0 : 1)}";
        warning.text = status.ActiveBosses > 0 ? "BIG METEOR INBOUND  //  5 HITS TO DESTROY" : $"NEXT BIG METEOR  {status.DestroyedMeteors % GameSession.MeteorsPerBoss} / {GameSession.MeteorsPerBoss}";
        warning.color = status.ActiveBosses > 0 ? new Color(1, 0.7f, 0.35f) : new Color(0.5f, 0.75f, 0.9f);
        gameOverPanel.SetActive(status.IsGameOver);
    }

    private static Text Label(Transform parent, Font font, string content, Vector2 position, Vector2 size, int fontSize, bool bold = false)
    {
        var label = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        label.transform.SetParent(parent, false);
        label.font = font;
        label.text = content;
        label.fontSize = fontSize;
        label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        label.color = new Color(0.85f, 0.93f, 1);
        label.raycastTarget = false;
        var rect = label.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return label;
    }
}
