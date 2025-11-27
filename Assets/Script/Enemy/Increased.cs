using UnityEngine;

public class Increased : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Health>(out var health))
        {
            health.AddGold(1000);
            health.IncreaseStress(10);
            health.Heal(10);
            health.HealMN(10);
        }

    }
}
