using NUnit.Framework;
using UnityEngine;

public sealed class GameRulesTests
{
    [Test]
    public void BossSurvivesFourShotsAndDiesOnFifthOnly()
    {
        var health = new Health(5);
        for (var i = 0; i < 4; i++) Assert.That(health.ApplyDamage(1), Is.False);
        Assert.That(health.Remaining, Is.EqualTo(1));
        Assert.That(health.ApplyDamage(1), Is.True);
        Assert.That(health.ApplyDamage(1), Is.False);
    }

    [Test]
    public void RegularMeteorDiesInOneShotAndInvalidDamageDoesNothing()
    {
        var health = new Health(1);
        Assert.That(health.ApplyDamage(-1), Is.False);
        Assert.That(health.ApplyDamage(0), Is.False);
        Assert.That(health.Remaining, Is.EqualTo(1));
        Assert.That(health.ApplyDamage(1), Is.True);
    }

    [Test]
    public void EveryFiveRegularKillsRequestExactlyOneBoss()
    {
        var session = new GameSession();
        var requests = 0;
        session.BossRequested += () => requests++;
        for (var i = 1; i <= 11; i++)
        {
            session.RecordDestruction(false, Vector3.zero);
            Assert.That(requests, Is.EqualTo(i / 5));
        }
    }

    [Test]
    public void BossKillsDoNotAdvanceRegularMeteorProgress()
    {
        var session = new GameSession();
        session.AddBoss();
        session.RecordDestruction(true, Vector3.zero);
        Assert.That(session.DestroyedMeteors, Is.Zero);
        Assert.That(session.BossesDestroyed, Is.EqualTo(1));
        Assert.That(session.ActiveBosses, Is.Zero);
    }

    [Test]
    public void EscapedBossDoesNotScoreOrShakeAndOtherBossKeepsZoomActive()
    {
        var session = new GameSession();
        var effects = 0;
        session.AsteroidDestroyed += (_, __) => effects++;
        session.AddBoss(); session.AddBoss(); session.RemoveBoss();
        Assert.That(session.ActiveBosses, Is.EqualTo(1));
        Assert.That(session.BossesDestroyed, Is.Zero);
        Assert.That(effects, Is.Zero);
        session.RemoveBoss(); session.RemoveBoss();
        Assert.That(session.ActiveBosses, Is.Zero);
    }

    [Test]
    public void DeathStopsScoringSpawnsAndDestructionEvents()
    {
        var session = new GameSession();
        for (var i = 0; i < 4; i++) session.RecordDestruction(false, Vector3.zero);
        var effects = 0;
        var requests = 0;
        session.AsteroidDestroyed += (_, __) => effects++;
        session.BossRequested += () => requests++;
        session.EndGame(); session.EndGame();
        session.RecordDestruction(false, Vector3.zero);
        Assert.That(session.IsGameOver, Is.True);
        Assert.That(session.DestroyedMeteors, Is.EqualTo(4));
        Assert.That(effects, Is.Zero);
        Assert.That(requests, Is.Zero);
    }

    [Test]
    public void CloserOrbitIsFasterThanAFarOrbit()
    {
        float near = OrbitMotion.SpeedFromSqrDistance(2.5f * 2.5f, 2.5f, 10f, 6.5f, 2.2f);
        float far = OrbitMotion.SpeedFromSqrDistance(10f * 10f, 2.5f, 10f, 6.5f, 2.2f);
        Assert.That(near, Is.EqualTo(6.5f).Within(0.001f));
        Assert.That(far, Is.EqualTo(2.2f).Within(0.001f));
        Assert.That(near, Is.GreaterThan(far));
    }

    [Test]
    public void AllThreeLabSolutionsOrbitAndCatchThePreferredRadius()
    {
        var start = new Vector2(10f, 0f);
        var center = Vector2.zero;
        foreach (OrbitMotion.Solution solution in System.Enum.GetValues(typeof(OrbitMotion.Solution)))
        {
            Vector2 positive = OrbitMotion.Step(start, center, solution, 5f, 5f, 0.7f, 1f, 5f, 5f, 2.5f, 10f, 1f);
            Vector2 negative = OrbitMotion.Step(start, center, solution, 5f, 5f, 0.7f, -1f, 5f, 5f, 2.5f, 10f, 1f);
            Assert.That(positive.y, Is.GreaterThan(0.2f), solution.ToString());
            Assert.That(negative.y, Is.LessThan(-0.2f), solution.ToString());
            float radius = Vector2.Distance(positive, center);
            Assert.That(radius, Is.LessThan(10f), solution.ToString());
            Assert.That(radius, Is.GreaterThan(4.5f), solution.ToString());
        }
    }

    [Test]
    public void FacingUsesTheSpriteUpAxisAndDoesNotTeleportOffAContact()
    {
        Assert.That(OrbitMotion.FacingDegrees(new Vector2(0f, -4f), Vector2.zero), Is.EqualTo(0f).Within(0.01f));
        Assert.That(OrbitMotion.FacingDegrees(new Vector2(4f, 0f), Vector2.zero), Is.EqualTo(90f).Within(0.01f));
        Vector2 stepped = OrbitMotion.Step(Vector2.zero, Vector2.zero, OrbitMotion.Solution.QuaternionOffset,
            5f, 3.5f, 0.7f, 1f, 6.5f, 2.2f, 2.5f, 10f, 0.02f);
        Assert.That(stepped.magnitude, Is.LessThan(0.2f));
    }

    [Test]
    public void OnceOnOrbitTheMeteorSpiralsTowardTheShip()
    {
        Vector2 position = new Vector2(5f, 0f);
        for (var i = 0; i < 20; i++)
        {
            position = OrbitMotion.Step(position, Vector2.zero, OrbitMotion.Solution.QuaternionOffset,
                5f, 3.5f, 1f, 1f, 6.5f, 2.2f, 2.5f, 10f, 0.1f);
        }

        Assert.That(position.magnitude, Is.EqualTo(3f).Within(0.05f));
        Vector2 stillOutside = OrbitMotion.Step(new Vector2(10f, 0f), Vector2.zero, OrbitMotion.Solution.QuaternionOffset,
            5f, 2f, 1f, 1f, 6.5f, 2.2f, 2.5f, 10f, 0.5f);
        Assert.That(stillOutside.magnitude, Is.EqualTo(9f).Within(0.05f));
    }
}
