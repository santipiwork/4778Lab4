using UnityEngine;

public sealed class ShipWeapon : MonoBehaviour
{
    [SerializeField, Min(0.05f)] private float shotInterval = 0.25f;
    private GameObject projectilePrefab;
    private GameSession session;
    private float nextShotTime;
    public void Initialize(GameObject prefab, GameSession game) { projectilePrefab = prefab; session = game; }
    public void TryFire()
    {
        if (session == null || session.IsGameOver || Time.time < nextShotTime) return;
        nextShotTime = Time.time + shotInterval;
        var shot = Instantiate(projectilePrefab, transform.position + Vector3.up * 0.7f, Quaternion.identity);
        shot.GetComponent<Laser>().Initialize(session);
    }
}
