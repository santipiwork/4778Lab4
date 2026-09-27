using UnityEngine;

public sealed class MeteorSpawner : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float interval = 2f;
    private GameSession session;
    private GameObject smallPrefab;
    private GameObject bossPrefab;
    private Transform ship;
    private float nextSpawn;
    private int pattern;
    public void Initialize(GameSession game, GameObject small, GameObject boss, Transform playerShip)
    {
        session = game;
        smallPrefab = small;
        bossPrefab = boss;
        ship = playerShip;
        session.BossRequested += SpawnBoss;
        nextSpawn = Time.time + 1f;
    }
    private void Update()
    {
        if (session == null || session.IsGameOver || Time.time < nextSpawn) return;
        nextSpawn = Time.time + interval;
        Spawn(smallPrefab, Random.Range(-7f, 7f));
    }
    private void SpawnBoss() { session.AddBoss(); Spawn(bossPrefab, Random.Range(-4f, 4f)); }
    private void Spawn(GameObject prefab, float x)
    {
        var meteor = Instantiate(prefab, new Vector3(x, 9, 0), Quaternion.identity);
        meteor.GetComponent<Meteor>().Initialize(session);
        var movement = meteor.GetComponent<MeteorMovement>();
        movement.Initialize(session, ship);
        if (movement is OrbitMeteorMovement orbit)
        {
            int index = pattern++;
            orbit.UsePattern((OrbitMotion.Solution)(index % 3), index % 2 == 0 ? 1f : -1f);
        }
    }
    private void OnDestroy() { if (session != null) session.BossRequested -= SpawnBoss; }
}
