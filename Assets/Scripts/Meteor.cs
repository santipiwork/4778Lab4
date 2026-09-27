using UnityEngine;

/// <summary>Damage and lifetime shared by all meteor types, independent of movement and presentation.</summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Meteor : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1)] private int hitPoints = 1;
    [SerializeField] private bool isBoss;
    private Health health;
    private GameSession session;
    private bool removed;
    public bool IsBoss => isBoss;
    public int RemainingHits => health?.Remaining ?? hitPoints;

    public void Initialize(GameSession game)
    {
        session = game;
        health = new Health(hitPoints);
    }

    public void TakeDamage(int amount)
    {
        if (removed || session == null || session.IsGameOver) return;
        if (!health.ApplyDamage(amount)) return;
        removed = true;
        session.RecordDestruction(isBoss, transform.position);
        RemoveObject();
    }

    public void Despawn()
    {
        if (removed) return;
        removed = true;
        if (isBoss) session?.RemoveBoss();
        RemoveObject();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (removed || session == null || session.IsGameOver) return;
        var ship = other.GetComponentInParent<Player>();
        if (ship == null) return;
        ship.Kill();
        Despawn();
    }

    private void RemoveObject()
    {
        gameObject.SetActive(false); // Prevent duplicate callbacks in the same physics step.
        Destroy(gameObject);
    }
}
