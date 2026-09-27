using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class Laser : MonoBehaviour
{
    [SerializeField, Min(1)] private int damage = 1;
    [SerializeField] private float speed = 14f;
    [SerializeField] private float lifetime = 3f;
    private Rigidbody2D body;
    private GameSession session;
    private bool spent;

    private void Awake() => body = GetComponent<Rigidbody2D>();
    public void Initialize(GameSession game)
    {
        session = game;
        Destroy(gameObject, lifetime);
    }
    private void FixedUpdate()
    {
        if (session == null || session.IsGameOver || spent)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }
        body.MovePosition(body.position + Vector2.up * (speed * Time.fixedDeltaTime));
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (spent || session == null || session.IsGameOver) return;
        var target = other.GetComponentInParent<IDamageable>();
        if (target == null) return;
        spent = true;
        target.TakeDamage(damage);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
