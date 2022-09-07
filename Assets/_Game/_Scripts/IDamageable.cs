public interface IDamageable
{
        float currentHealth { get; }

        void ApplyDamage(float damage);
}