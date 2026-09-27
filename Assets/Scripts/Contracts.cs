using System;
using UnityEngine;

public interface IShipInput
{
    Vector2 Move { get; }
    bool FireHeld { get; }
}
public interface IDamageable { void TakeDamage(int amount); }
public interface IGameStatus
{
    bool IsGameOver { get; }
    int DestroyedMeteors { get; }
    int BossesDestroyed { get; }
    int ActiveBosses { get; }
    event Action<bool, Vector3> AsteroidDestroyed;
}
