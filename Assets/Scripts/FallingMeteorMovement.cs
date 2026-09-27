using UnityEngine;

public sealed class FallingMeteorMovement : MeteorMovement
{
    [SerializeField] private float speed = 2f;
    protected override Vector2 GetVelocity(Vector2 position, float elapsed) => Vector2.down * speed;
}
