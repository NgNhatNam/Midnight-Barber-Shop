using UnityEngine;

public class Trap : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Health>(out var health))
        {
            health.SpendGold(1000);
            health.IncreaseStress(100);
            health.Heal(100);
            health.Tired(100);
            

        }

    }
}
