using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>The sole Input System adapter. Each session owns its action instance.</summary>
public sealed class ShooterInput : MonoBehaviour, IShipInput
{
    [SerializeField] private InputActionAsset actions;
    private InputActionAsset instance;
    private InputAction move;
    private InputAction fire;
    private InputAction restart;
    public Vector2 Move => move?.ReadValue<Vector2>() ?? Vector2.zero;
    public bool FireHeld => fire != null && fire.IsPressed();
    public bool RestartPressed => restart != null && restart.WasPressedThisFrame();
    private void Awake()
    {
        if (actions == null)
        {
            Debug.LogError("Assign ShooterControls to ShooterInput before playing.", this);
            enabled = false;
            return;
        }
        instance = Instantiate(actions);
        move = instance.FindAction("Gameplay/Move", true);
        fire = instance.FindAction("Gameplay/Fire", true);
        restart = instance.FindAction("Gameplay/Restart", true);
    }
    private void OnEnable() => instance?.Enable();
    private void OnDisable() => instance?.Disable();
    private void OnDestroy() => Destroy(instance);
}
