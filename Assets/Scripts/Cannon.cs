using UnityEngine;

public class Cannon : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;

    [Header("Disparo")]
    [SerializeField] private float shootForce = 20f;
    [SerializeField] private float fireCooldown = 1f;

    private float nextFireTime;

    public void Shoot()
    {
        if (Time.time < nextFireTime)
            return;

        if (projectilePrefab == null || shootPoint == null)
        {
            Debug.LogWarning("Falta asignar el prefab o el Shoot Point.");
            return;
        }

        nextFireTime = Time.time + fireCooldown;

        GameObject projectile = Instantiate(
            projectilePrefab,
            shootPoint.position,
            shootPoint.rotation
        );

        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        if (projectileRb == null)
        {
            Debug.LogWarning("El prefab disparado necesita un Rigidbody.");
            Destroy(projectile);
            return;
        }

        projectileRb.AddForce(
            shootPoint.forward * shootForce,
            ForceMode.Impulse
        );
    }
}