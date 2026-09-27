using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Composition root: creates and connects the game's independent systems.</summary>
[RequireComponent(typeof(ShooterInput), typeof(MeteorSpawner), typeof(GameHud))]
public sealed class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject meteorPrefab;
    [SerializeField] private GameObject bigMeteorPrefab;
    [SerializeField] private ShooterCamera cameraRig;
    private ShooterInput controls;
    public GameSession Session { get; private set; }
    public Player Ship { get; private set; }

    private void Start()
    {
        Session = new GameSession();
        controls = GetComponent<ShooterInput>();
        Ship = Instantiate(playerPrefab, new Vector3(0, -3, 0), Quaternion.identity).GetComponent<Player>();
        Ship.Initialize(controls, Session);
        GetComponent<MeteorSpawner>().Initialize(Session, meteorPrefab, bigMeteorPrefab, Ship.transform);
        GetComponent<GameHud>().Initialize(Session);
        cameraRig.Initialize(Ship.transform, Session);
    }

    private void Update()
    {
        if (Session != null && Session.IsGameOver && controls.RestartPressed)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
