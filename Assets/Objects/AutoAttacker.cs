using UnityEngine;
using System.Collections;

/// <summary>
/// Finds the closest enemy and moves to attack them.
/// Will find a new target if the current one is null or dies.
/// </summary>
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(Rigidbody))]
public class AutoAttacker : MonoBehaviour
{
    [Header("Combat Stats")]
    // Updated from a single 'attackDamage' to a min/max range
    [SerializeField] private float minAttackDamage = 8f;
    [SerializeField] private float maxAttackDamage = 12f;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private float attackRange = 1f;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    private PlayerHealth target; // Target is now private and found automatically
    private PlayerHealth myHealth;
    private Rigidbody rb;
    private int myTeamID;
    private float lastAttackTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        myHealth = GetComponent<PlayerHealth>();
    }

    void Start()
    {
        myTeamID = myHealth.teamID;
        StartCoroutine(FightAndMoveLoop());
    }

    /// <summary>
    /// Finds the closest living enemy on the opposing team.
    /// </summary>
    private void FindNewTarget()
    {
        PlayerHealth[] allPlayers = FindObjectsOfType<PlayerHealth>();
        PlayerHealth closestEnemy = null;
        float minDistance = Mathf.Infinity;

        foreach (PlayerHealth player in allPlayers)
        {
            // Skip if they are dead or on my team
            if (player.isDead || player.teamID == myTeamID)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = player;
            }
        }

        target = closestEnemy;
    }

    private IEnumerator FightAndMoveLoop()
    {
        while (!myHealth.isDead)
        {
            // If I don't have a target, or my target is dead, find a new one.
            if (target == null || target.isDead)
            {
                FindNewTarget();

                // If FindNewTarget() couldn't find anyone (all enemies dead), 
                // just wait and check again next frame.
                if (target == null)
                {
                    rb.velocity = Vector3.zero; // Stop moving
                    yield return null; 
                    continue; 
                }
            }

            // --- Movement Logic ---
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (distanceToTarget > attackRange)
            {
                // Move towards target
                Vector3 direction = (target.transform.position - transform.position).normalized;
                rb.velocity = new Vector3(direction.x * moveSpeed, rb.velocity.y, direction.z * moveSpeed);
                
                // Look at the target
                transform.LookAt(target.transform.position);
            }
            else
            {
                // Stop moving, we are in range
                rb.velocity = Vector3.zero;

                // --- Attack Logic ---
                if (Time.time > lastAttackTime + attackCooldown)
                {
                    Attack();
                }
            }

            yield return null; // Wait for the next frame
        }
    }

    private void Attack()
    {
        if (target == null || target.isDead) return;

        lastAttackTime = Time.time;
        
        // --- DAMAGE IS NOW RANDOMIZED ---
        float randomDamage = Random.Range(minAttackDamage, maxAttackDamage);

        Debug.Log(gameObject.name + " (Team " + myTeamID + ") attacks " + target.name + " for " + randomDamage.ToString("F1") + " damage.");
        target.TakeDamage(randomDamage);
    }
}

