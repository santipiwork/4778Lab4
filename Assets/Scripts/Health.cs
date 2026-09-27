using System;

/// <summary>Pure damage rules, independent of Unity callbacks or scene objects.</summary>
public sealed class Health
{
    public int Remaining { get; private set; }
    public Health(int maximum)
    {
        if (maximum < 1) throw new ArgumentOutOfRangeException(nameof(maximum));
        Remaining = maximum;
    }
    // True exactly once, when this hit changes the target from alive to dead.
    public bool ApplyDamage(int amount)
    {
        if (amount <= 0 || Remaining == 0) return false;
        Remaining = Math.Max(0, Remaining - amount);
        return Remaining == 0;
    }
}
