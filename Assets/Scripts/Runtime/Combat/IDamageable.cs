namespace Blackhole
{
    public interface IDamageable
    {
        bool IsDead { get; }
        void TakeHit();
    }
}
