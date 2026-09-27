using UnityEngine;

/// <summary>
/// Lab 3 EnemyController orbit: distance-scaled speed, three orbit solutions, and facing
/// the ship without LookRotation. Prefab data chooses radius and speed; the spawner
/// picks which solution and direction each meteor uses.
/// </summary>
public sealed class OrbitMeteorMovement : MeteorMovement
{
    [SerializeField] private OrbitMotion.Solution solution = OrbitMotion.Solution.QuaternionOffset;
    [SerializeField] private float preferredRadius = 5f;
    [SerializeField] private float radiusCatchUp = 3.5f;
    [SerializeField] private float approachSpeed = 0.7f;
    [SerializeField] private float orbitSign = 1f;
    [SerializeField] private float nearSpeed = 6.5f;
    [SerializeField] private float farSpeed = 2.2f;
    [SerializeField] private float nearDistance = 2.5f;
    [SerializeField] private float farDistance = 10f;
    [SerializeField] private float turnLerp = 10f;

    protected override void Awake()
    {
        base.Awake();
        Body.constraints &= ~RigidbodyConstraints2D.FreezeRotation;
    }

    public void UsePattern(OrbitMotion.Solution orbitSolution, float direction)
    {
        solution = orbitSolution;
        orbitSign = Mathf.Sign(direction) >= 0f ? 1f : -1f;
    }

    protected override Vector2 GetVelocity(Vector2 position, float elapsed)
    {
        if (Ship == null) return Vector2.zero;
        float dt = Time.fixedDeltaTime;
        if (dt <= 0f) return Vector2.zero;
        Vector2 center = Ship.position;
        Vector2 next = OrbitMotion.Step(position, center, solution, preferredRadius, radiusCatchUp, approachSpeed, orbitSign,
            nearSpeed, farSpeed, nearDistance, farDistance, dt);
        return (next - position) / dt;
    }

    protected override void ApplyFacing()
    {
        if (Ship == null) return;
        float targetAngle = OrbitMotion.FacingDegrees(Body.position, Ship.position);
        Quaternion target = Quaternion.Euler(0f, 0f, targetAngle);
        Quaternion current = Quaternion.Euler(0f, 0f, Body.rotation);
        Quaternion turned = Quaternion.Slerp(current, target, 1f - Mathf.Exp(-turnLerp * Time.fixedDeltaTime));
        Body.MoveRotation(turned.eulerAngles.z);
    }
}
