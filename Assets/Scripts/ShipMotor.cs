using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class ShipMotor : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private float acceleration = 24f;
    [SerializeField] private Vector2 limits = new Vector2(8, 4);
    private Rigidbody2D body;
    private Vector2 input;
    private Vector2 velocity;
    private void Awake() => body = GetComponent<Rigidbody2D>();
    public void SetInput(Vector2 value) => input = Vector2.ClampMagnitude(value, 1);
    private void FixedUpdate()
    {
        velocity = Vector2.MoveTowards(velocity, input * speed, acceleration * Time.fixedDeltaTime);
        var next = body.position + velocity * Time.fixedDeltaTime;
        next.x = Mathf.Clamp(next.x, -limits.x, limits.x);
        next.y = Mathf.Clamp(next.y, -limits.y, limits.y);
        body.MovePosition(next);
    }
}
