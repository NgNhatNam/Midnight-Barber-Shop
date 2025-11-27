using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 10f;
    public float lifeTime = 1f;

    
    private Pool pool;
    private Coroutine lifeCoroutine;
    private AudioController audioController;
    private void Awake()
    {
        audioController = FindAnyObjectByType<AudioController>();
    }

    void OnEnable()
    {
        audioController.PlaySFX(audioController.playerShoot, false);
        pool = FindFirstObjectByType<Pool>();
        lifeCoroutine = StartCoroutine(LifeTimer());
    }

     void OnDisable()
    {
        if(lifeCoroutine != null)
        {
            StopCoroutine(lifeCoroutine);  
            lifeCoroutine = null;
        }
    }

    private IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(lifeTime);

        pool.ReturnObject("Bullet", gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu trúng Boss
        if (collision.CompareTag("Boss"))
        {
            var enemy = collision.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.EnemyDamage((int)damage);
            }

            // Trả lại về pool sau khi trúng
            pool.ReturnObject("Bullet", gameObject);
        }
    }
}
