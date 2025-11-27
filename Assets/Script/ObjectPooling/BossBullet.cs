using UnityEngine;
using System.Collections;

public class BossBullet : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 3f;
    private Pool pool;
    private Health health;

    void OnEnable()
    {
        pool = FindFirstObjectByType<Pool>();
        StartCoroutine(DisableAfterTime());
    }

    IEnumerator DisableAfterTime()
    {
        yield return new WaitForSeconds(lifetime);
        pool.ReturnObject("Boss_Bullet", gameObject);
    }

    private void Start()
    {
        health = FindAnyObjectByType<Health>();
    }

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            // Gây damage ở đây
            health.Damage(10);
            pool.ReturnObject("Boss_Bullet", gameObject);
        }
    }
}
