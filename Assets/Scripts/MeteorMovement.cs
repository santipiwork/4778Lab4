using UnityEngine;

/// <summary>Extend this contract to change enemy motion without editing damage or spawning.</summary>
[RequireComponent(typeof(Rigidbody2D))]
public abstract class MeteorMovement : MonoBehaviour
{
    private Rigidbody2D body;
    private Meteor meteor;
    private IGameStatus status;
    private float elapsed;
    protected Rigidbody2D Body => body;
    protected Transform Ship { get; private set; }
    protected virtual void Awake() { body = GetComponent<Rigidbody2D>(); meteor = GetComponent<Meteor>(); }
    public void Initialize(IGameStatus game, Transform ship = null)
    {
        status = game;
        if (ship != null) Ship = ship;
    }
    protected abstract Vector2 GetVelocity(Vector2 position, float elapsed);
    protected virtual void ApplyFacing() { }
    private void FixedUpdate()
    {
        if (status == null || status.IsGameOver)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }
        elapsed += Time.fixedDeltaTime;
        body.MovePosition(body.position + GetVelocity(body.position, elapsed) * Time.fixedDeltaTime);
        ApplyFacing();
        if (body.position.y < -13) meteor.Despawn();
    }
}
