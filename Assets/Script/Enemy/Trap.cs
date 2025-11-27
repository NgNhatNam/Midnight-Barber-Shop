using UnityEngine;

public class Trap : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Health>(out var health))
        {
            health.SpendGold(1000);
            health.DecreaseStress(10);
            health.Damage(10);
            health.Tired(20);
            

        }

    }
}
