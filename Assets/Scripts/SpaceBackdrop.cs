using UnityEngine;

/// <summary>World-space stars make the camera's tracking motion visible.</summary>
public sealed class SpaceBackdrop : MonoBehaviour
{
    private Texture2D texture;
    private Sprite sprite;
    private void Awake()
    {
        texture = new Texture2D(3, 3);
        texture.SetPixels(new[] { Color.clear, Color.white, Color.clear, Color.white, Color.white,
            Color.white, Color.clear, Color.white, Color.clear });
        texture.Apply();
        sprite = Sprite.Create(texture, new Rect(0, 0, 3, 3), Vector2.one * 0.5f, 30);
        var random = new System.Random(4778);
        for (var i = 0; i < 260; i++)
        {
            var star = new GameObject("Star").AddComponent<SpriteRenderer>();
            star.transform.SetParent(transform);
            star.transform.position = new Vector3((float)random.NextDouble() * 70 - 35,
                (float)random.NextDouble() * 60 - 30, 2);
            star.transform.localScale = Vector3.one * (0.25f + (float)random.NextDouble() * 0.8f);
            star.sprite = sprite;
            star.color = new Color(0.45f, 0.65f, 0.85f, 0.25f + (float)random.NextDouble() * 0.55f);
            star.sortingOrder = -10;
        }
    }
    private void OnDestroy() { Destroy(sprite); Destroy(texture); }
}
