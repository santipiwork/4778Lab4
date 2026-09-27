using UnityEngine;

/// <summary>
/// Lab 3 (Week 4) enemy motion, moved from the XZ plane onto this shooter's XY plane.
/// LookAt, LookRotation, RotateAround, RotateTowards, and FromToRotation are not used.
/// </summary>
public static class OrbitMotion
{
    public enum Solution
    {
        QuaternionOffset,
        CrossTangent,
        TrsMatrix
    }

    public static float SpeedFromSqrDistance(float sqrDistance, float nearDistance, float farDistance, float nearSpeed, float farSpeed)
    {
        float nearSqr = nearDistance * nearDistance;
        float farSqr = farDistance * farDistance;
        float t = Mathf.InverseLerp(nearSqr, farSqr, sqrDistance);
        return Mathf.Lerp(nearSpeed, farSpeed, t);
    }

    public static Vector2 Step(
        Vector2 position,
        Vector2 center,
        Solution solution,
        float preferredRadius,
        float radiusCatchUp,
        float approachSpeed,
        float orbitSign,
        float nearSpeed,
        float farSpeed,
        float nearDistance,
        float farDistance,
        float deltaTime)
    {
        Vector2 offset = position - center;
        float linearSpeed = SpeedFromSqrDistance(offset.sqrMagnitude, nearDistance, farDistance, nearSpeed, farSpeed);
        Vector2 rotated = solution switch
        {
            Solution.CrossTangent => OrbitWithCross(offset, linearSpeed, orbitSign, deltaTime),
            Solution.TrsMatrix => OrbitWithTrs(offset, center, linearSpeed, orbitSign, deltaTime),
            _ => OrbitWithQuaternion(offset, linearSpeed, orbitSign, deltaTime)
        };
        return center + ApplyRadius(rotated, preferredRadius, radiusCatchUp, approachSpeed, deltaTime);
    }

    // Sprite up (Vector2.up) turns toward the ship. Yaw is Dot/Cross + Atan2, then Euler Z.
    public static float FacingDegrees(Vector2 position, Vector2 center)
    {
        Vector2 toShip = center - position;
        if (toShip.sqrMagnitude < 0.0001f) return 0f;
        Vector3 dir = new Vector3(toShip.x, toShip.y, 0f).normalized;
        float cos = Vector3.Dot(Vector3.up, dir);
        float sin = Vector3.Dot(Vector3.forward, Vector3.Cross(Vector3.up, dir));
        return Mathf.Atan2(sin, cos) * Mathf.Rad2Deg;
    }

    private static Vector2 OrbitWithQuaternion(Vector2 offset, float linearSpeed, float orbitSign, float deltaTime)
    {
        float radius = Mathf.Max(offset.magnitude, 0.001f);
        float degrees = (linearSpeed / radius) * Mathf.Rad2Deg * orbitSign * deltaTime;
        Vector3 rotated = Quaternion.Euler(0f, 0f, degrees) * new Vector3(offset.x, offset.y, 0f);
        return new Vector2(rotated.x, rotated.y);
    }

    private static Vector2 OrbitWithCross(Vector2 offset, float linearSpeed, float orbitSign, float deltaTime)
    {
        Vector2 radial = offset.sqrMagnitude > 0.0001f ? offset.normalized : Vector2.right;
        Vector3 tangent = Vector3.Cross(Vector3.forward, new Vector3(radial.x, radial.y, 0f)) * orbitSign;
        return offset + new Vector2(tangent.x, tangent.y) * linearSpeed * deltaTime;
    }

    private static Vector2 OrbitWithTrs(Vector2 offset, Vector2 center, float linearSpeed, float orbitSign, float deltaTime)
    {
        float radius = Mathf.Max(offset.magnitude, 0.001f);
        float degrees = (linearSpeed / radius) * Mathf.Rad2Deg * orbitSign * deltaTime;
        Matrix4x4 orbit = Matrix4x4.TRS(new Vector3(center.x, center.y, 0f), Quaternion.Euler(0f, 0f, degrees), Vector3.one);
        Vector3 world = orbit.MultiplyPoint3x4(new Vector3(offset.x, offset.y, 0f));
        return new Vector2(world.x - center.x, world.y - center.y);
    }

    private static Vector2 ApplyRadius(Vector2 offset, float preferredRadius, float radiusCatchUp, float approachSpeed, float deltaTime)
    {
        // A zero offset cannot be normalized. Keep a tiny heading so the spiral can continue.
        float currentRadius = offset.magnitude;
        if (currentRadius < 0.001f)
        {
            offset = Vector2.right * 0.001f;
            currentRadius = 0.001f;
        }

        // Join the orbit from outside, then keep circling inward until the meteor hits the ship.
        float radius = currentRadius > preferredRadius
            ? Mathf.MoveTowards(currentRadius, preferredRadius, radiusCatchUp * deltaTime)
            : Mathf.MoveTowards(currentRadius, 0f, approachSpeed * deltaTime);
        return offset.normalized * radius;
    }
}
