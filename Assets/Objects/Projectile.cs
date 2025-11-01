using UnityEngine;

/// <summary>
/// This script is attached to a projectile prefab.
/// It flies towards a target and deals damage on (near) impact.
/// </summary>
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float hitDistance = 0.5f; // How close to get before "hitting"
    [SerializeField] private float maxLifetime = 5f; // Time in seconds before self-destructing

    private PlayerHealth target;
    private float damage;

    /// <summary>
    /// Called by the attacker to set the projectile's target and damage.
    /// </summary>
    public void Seek(PlayerHealth _target, float _damage)
    {
        target = _target;
        damage = _damage;

        // Start the self-destruct timer
        Destroy(gameObject, maxLifetime);
    }

    void Update()
    {
        // If the target is dead or gone, destroy the projectile
        if (target == null || target.isDead)
        {
            Destroy(gameObject);
            return;
        }

        // --- Move towards the target ---
        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.LookAt(target.transform.position);

        // --- Check for hit ---
        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget < hitDistance)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        // Deal damage (if target isn't already dead)
        if (target != null && !target.isDead)
        {
            target.TakeDamage(damage);
        }

        // Destroy the projectile
        Destroy(gameObject);
    }
}
