using UnityEngine;
using System.Collections;

/// <summary>
/// Handles Melee, Ranged, and Mage combat.
/// Finds the closest enemy and moves to attack them.
/// Will find a new target if the current one is null or dies.
/// </summary>
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(Rigidbody))]
public class AutoAttacker : MonoBehaviour
{
    // Enum to define the attacker's type
    public enum AttackerType { Melee, Ranged, Mage }

    [Header("Unit Type")]
    [SerializeField] private AttackerType attackerType = AttackerType.Melee;

    [Header("Combat Stats")]
    [SerializeField] private float minAttackDamage = 8f;
    [SerializeField] private float maxAttackDamage = 12f;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private float attackRange = 1f; // For melee, this is engage range.
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Ranged Settings (If Ranged)")]
    [SerializeField] private GameObject projectilePrefab; // The "bullet" to fire
    // Ranged units will use 'attackRange' as their firing distance

    [Header("Mage Settings (If Mage)")]
    [SerializeField] private GameObject areaDamageSpellPrefab; // The AoE spell to cast
    [SerializeField] private float mageCastRange = 8f; // Mages will stop at this distance to cast
    
    [Header("Common Settings")]
    [SerializeField] private Transform firePoint; // Optional: Where projectiles/spells fire from

    private PlayerHealth target;
    private PlayerHealth myHealth;
    private Rigidbody rb;
    private int myTeamID;
    private float lastAttackTime;
    private float freezePositionY = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        myHealth = GetComponent<PlayerHealth>();

        // --- NEW ---
        // Constrain the player to a 2D plane (XZ) and prevent tipping over
        // This keeps them from falling or flying, and stops them from tipping.
        // Y is always 0.
        rb.constraints = RigidbodyConstraints.FreezeRotationX | 
                           RigidbodyConstraints.FreezeRotationZ;
        // --- END NEW ---

        // If no firePoint is assigned, just use the player's main transform
        if (firePoint == null)
        {
            // Create an empty GameObject as a child if firePoint is not explicitly set in editor
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = Vector3.up * 1.5f; // Slightly above the player
            firePoint = fp.transform;
        }
    }

    void Start()
    {
        myTeamID = myHealth.teamID;

        // Ranged attackers should use their 'attackRange' as their primary range
        // Mages have a separate 'mageCastRange'
        if (attackerType == AttackerType.Ranged)
        {
            // We'll use attackRange for Ranged, no change needed here, just in the loop.
        }
        else if (attackerType == AttackerType.Mage)
        {
            // Use mageCastRange
        }
        
        StartCoroutine(FightAndMoveLoop());
    }

    private void FindNewTarget()
    {
        PlayerHealth[] allPlayers = FindObjectsOfType<PlayerHealth>();
        PlayerHealth closestEnemy = null;
        float minDistance = Mathf.Infinity;

        foreach (PlayerHealth player in allPlayers)
        {
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
            if (target == null || target.isDead)
            {
                FindNewTarget();
                if (target == null)
                {
                    rb.linearVelocity = Vector3.zero;
                    yield return null; 
                    continue; 
                }
            }

            // Determine the effective range based on unit type
            float currentEngageRange = attackRange; // Default for Melee
            if (attackerType == AttackerType.Ranged)
            {
                currentEngageRange = attackRange; // Ranged uses attackRange
            }
            else if (attackerType == AttackerType.Mage)
            {
                currentEngageRange = mageCastRange;
            }

            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (distanceToTarget > currentEngageRange)
            {
                // Move towards target
                Vector3 direction = (target.transform.position - transform.position).normalized;
                rb.linearVelocity = new Vector3(direction.x * moveSpeed, 0, direction.z * moveSpeed); // Y velocity is 0
                transform.LookAt(target.transform.position);
            }
            else
            {
                // Stop moving, we are in range
                rb.linearVelocity = Vector3.zero;
                transform.LookAt(target.transform.position); 

                if (Time.time > lastAttackTime + attackCooldown)
                {
                    Attack();
                }
            }

            yield return null;
        }
    }

    private void Attack()
    {
        if (target == null || target.isDead) return;

        lastAttackTime = Time.time;
        float randomDamage = Random.Range(minAttackDamage, maxAttackDamage);

        switch (attackerType)
        {
            case AttackerType.Melee:
                Debug.Log(gameObject.name + " (Melee) attacks " + target.name + " for " + randomDamage.ToString("F1") + " damage.");
                target.TakeDamage(randomDamage);
                break;

            case AttackerType.Ranged:
                if (projectilePrefab == null)
                {
                    Debug.LogError("Ranged Attacker has no projectile prefab assigned!");
                    return;
                }
                Debug.Log(gameObject.name + " (Ranged) fires at " + target.name);
                GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                Projectile projectile = projectileGO.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.Seek(target, randomDamage);
                }
                break;

            case AttackerType.Mage:
                if (areaDamageSpellPrefab == null)
                {
                    Debug.LogError("Mage Attacker has no Area Damage Spell prefab assigned!");
                    return;
                }
                Debug.Log(gameObject.name + " (Mage) casts spell at " + target.name + "'s location.");
                
                // Mage casts the spell at the *target's current position*
                GameObject spellGO = Instantiate(areaDamageSpellPrefab, target.transform.position, Quaternion.identity);
                AreaDamageSpell spell = spellGO.GetComponent<AreaDamageSpell>();
                if (spell != null)
                {
                    // Pass damage and the *caster's team* to avoid friendly fire
                    spell.SetupSpell(randomDamage, myTeamID);
                }
                break;
        }
    }
}

