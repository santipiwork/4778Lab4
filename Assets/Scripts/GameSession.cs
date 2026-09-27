using System;
using UnityEngine;

/// <summary>Session rules only; has no knowledge of prefabs, input, HUD or Cinemachine.</summary>
public sealed class GameSession : IGameStatus
{
    public const int MeteorsPerBoss = 5;
    public bool IsGameOver { get; private set; }
    public int DestroyedMeteors { get; private set; }
    public int BossesDestroyed { get; private set; }
    public int ActiveBosses { get; private set; }
    public event Action BossRequested;
    public event Action<bool, Vector3> AsteroidDestroyed;
    public void RecordDestruction(bool boss, Vector3 position)
    {
        if (IsGameOver) return;
        if (boss) { BossesDestroyed++; RemoveBoss(); }
        else DestroyedMeteors++;
        AsteroidDestroyed?.Invoke(boss, position);
        if (!boss && DestroyedMeteors % MeteorsPerBoss == 0) BossRequested?.Invoke();
    }
    public void AddBoss() => ActiveBosses++;
    public void RemoveBoss() => ActiveBosses = Math.Max(0, ActiveBosses - 1);
    public void EndGame() => IsGameOver = true;
}
