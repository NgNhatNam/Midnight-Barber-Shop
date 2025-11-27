using UnityEngine;
using System.Collections;

public class LaserAttack : MonoBehaviour
{
    public float duration = 2f;
    public float rotationSpeed = 100f;
    private bool rotating = false;
    private Pool pool;
    private Health health;

    void OnEnable()
    {
        pool = FindFirstObjectByType<Pool>();
        StartCoroutine(LaserLifetime());
    }

    IEnumerator LaserLifetime()
    {
        yield return new WaitForSeconds(duration);
        pool.ReturnObject("Laser", gameObject);
    }

    public void StartRotating() => rotating = true;
    public void StopRotating() => rotating = false;

    void Update()
    {
        if (rotating)
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
