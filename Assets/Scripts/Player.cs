using UnityEngine;

/// <summary>Ship lifetime and coordination; locomotion, input and weapons are separate collaborators.</summary>
[RequireComponent(typeof(ShipMotor), typeof(ShipWeapon))]
public sealed class Player : MonoBehaviour
{
    [SerializeField] private GameObject laserPrefab;
    private IShipInput input;
    private GameSession session;
    private ShipMotor motor;
    private ShipWeapon weapon;
    private bool dead;

    public void Initialize(IShipInput controls, GameSession game)
    {
        input = controls;
        session = game;
        motor = GetComponent<ShipMotor>();
        weapon = GetComponent<ShipWeapon>();
        weapon.Initialize(laserPrefab, game);
    }

    private void Update()
    {
        if (input == null || dead || session.IsGameOver) return;
        motor.SetInput(input.Move);
        if (input.FireHeld) weapon.TryFire();
    }

    public void Kill()
    {
        if (dead) return;
        dead = true;
        motor.SetInput(Vector2.zero);
        session.EndGame();
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
