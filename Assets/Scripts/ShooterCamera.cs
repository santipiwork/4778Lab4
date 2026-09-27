using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera), typeof(CinemachinePositionComposer), typeof(CinemachineBasicMultiChannelPerlin))]
public sealed class ShooterCamera : MonoBehaviour
{
    [SerializeField] private float normalSize = 8f;
    [SerializeField] private float bossSize = 10.5f;
    [SerializeField] private float zoomSpeed = 3f;
    [SerializeField] private float shakeDuration = 0.3f;
    private CinemachineCamera cameraController;
    private CinemachineBasicMultiChannelPerlin noise;
    private IGameStatus status;
    private float shakeRemaining;
    private float shakeStrength;
    public void Initialize(Transform ship, IGameStatus game)
    {
        cameraController = GetComponent<CinemachineCamera>();
        noise = GetComponent<CinemachineBasicMultiChannelPerlin>();
        cameraController.Follow = ship;
        status = game;
        status.AsteroidDestroyed += Shake;
    }
    private void Update()
    {
        if (status == null) return;
        var targetSize = status.ActiveBosses > 0 ? bossSize : normalSize;
        cameraController.Lens.OrthographicSize = Mathf.Lerp(cameraController.Lens.OrthographicSize,
            targetSize, 1 - Mathf.Exp(-zoomSpeed * Time.deltaTime));
        shakeRemaining = Mathf.Max(0, shakeRemaining - Time.deltaTime);
        noise.AmplitudeGain = shakeStrength * shakeRemaining / shakeDuration;
    }
    private void Shake(bool boss, Vector3 position)
    {
        shakeStrength = Mathf.Max(noise.AmplitudeGain, boss ? 1.5f : 0.7f);
        shakeRemaining = shakeDuration;
    }
    private void OnDestroy() { if (status != null) status.AsteroidDestroyed -= Shake; }
}
