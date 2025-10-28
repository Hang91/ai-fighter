using UnityEngine;
using System.Collections;

/// <summary>
/// Automatically finds, moves towards, and attacks a target.
/// Requires PlayerHealth and Rigidbody components.
/// </summary>
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(Rigidbody))]
public class AutoAttacker : MonoBehaviour
{
    [Header("Attack Stats")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.0f; // Time in seconds between attacks

    [Header("Targeting")]
    [Tooltip("Drag the other player's GameObject here in the Inspector")]
    [SerializeField] private PlayerHealth target;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 1f; // The range to stop and attack
                                                   // Note: 1.0 might be tight, 1.5 might feel better

    // Component References
    private PlayerHealth myHealth;
    private Rigidbody rb;
    private float lastAttackTime;

    void Awake()
    {
        // Get the reference to our own health script
        myHealth = GetComponent<PlayerHealth>();
        // Get the reference to our Rigidbody for movement
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // Check if we have everything we need to start fighting
        if (target != null && myHealth != null && rb != null)
        {
            // Subscribe to the target's OnDeath event to know when to stop
            target.OnDeath += HandleTargetDeath;
            
            // Start the automatic attack loop
            StartCoroutine(FightAndMoveLoop());
        }
        else
        {
            if (target == null)
                Debug.LogWarning(gameObject.name + ": AutoAttacker has no target set!");
            if (myHealth == null)
                Debug.LogError(gameObject.name + ": AutoAttacker requires a PlayerHealth script on the same GameObject!");
            if (rb == null)
                Debug.LogError(gameObject.name + ": AutoAttacker requires a Rigidbody component!");
        }
    }

    private IEnumerator FightAndMoveLoop()
    {
        // This loop runs every frame to handle logic
        while (!myHealth.isDead)
        {
            // Stop everything if the target is gone
            if (target == null || target.isDead)
            {
                rb.linearVelocity = Vector3.zero; // Stop moving
                yield break; // Exit the coroutine
            }

            // --- Calculate distance and direction ---
            float distance = Vector3.Distance(transform.position, target.transform.position);
            Vector3 targetDirection = target.transform.position - transform.position;
            targetDirection.y = 0; // Keep the cube upright, don't look up/down

            // --- Rotation ---
            // Face the target
            if (targetDirection != Vector3.zero) // Avoid LookRotation error if at same spot
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                // You could Slerp here for smoother rotation, but this is fine
                transform.rotation = targetRotation; 
            }

            // --- Movement and Attacking ---
            if (distance <= attackRange)
            {
                // In Range: Stop and Attack
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); // Stop horizontal movement, but allow gravity

                // Check if attack is off cooldown
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                    lastAttackTime = Time.time;
                }
            }
            else
            {
                // Out of Range: Move towards target
                Vector3 moveDirection = targetDirection.normalized;
                // Move while respecting gravity (by preserving y velocity)
                rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
            }

            yield return null; // Wait for the next frame
        }
        
        // My health is dead, stop moving
        // rb.velocity = Vector3.zero; // <-- REMOVED this line
        // We remove this line so it doesn't fight with the
        // "tumble over" physics in PlayerHealth.cs's Die() method.
    }

    private void Attack()
    {
        if (target == null) return; // Safety check

        Debug.Log(gameObject.name + " attacks " + target.gameObject.name + " for " + attackDamage + " damage!");
        
        // Tell the target to take damage
        target.TakeDamage(attackDamage);
    }

    /// <summary>
    /// This method is called when the target's OnDeath event is fired.
    /// </summary>
    private void HandleTargetDeath()
    {
        if (!myHealth.isDead)
        {
            Debug.Log(gameObject.name + ": My target is dead! I am victorious!");
            // Stop the FightLoop coroutine
            StopAllCoroutines();
            rb.linearVelocity = Vector3.zero; // Stop moving
        }
    }

    // Unsubscribe from events when this object is destroyed to prevent memory leaks
    void OnDestroy()
    {
        if (target != null)
        {
            target.OnDeath -= HandleTargetDeath;
        }
    }
}


