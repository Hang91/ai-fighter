using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AreaDamageSpell : MonoBehaviour
{
    [Header("Spell Settings")]
    [SerializeField] private float spellRadius = 3f;
    [SerializeField] private float delayBeforeDamage = 0.5f;
    [SerializeField] private float spellDuration = 1.0f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Visuals")]
    [SerializeField] private GameObject visualEffectPrefab; // <-- This is the one we're checking

    private float spellDamage;
    private int casterTeamID;

    public void SetupSpell(float damage, int teamID)
    {
        spellDamage = damage;
        casterTeamID = teamID;

        // --- NEW DEBUGGING LOGS ---
        if (visualEffectPrefab != null)
        {
            Debug.Log("AreaDamageSpell: visualEffectPrefab is assigned. Spawning visual.", gameObject);
            GameObject visual = Instantiate(visualEffectPrefab, transform.position, Quaternion.identity, transform);
            
            // This scales the visual to match the damage radius
            visual.transform.localScale = new Vector3(spellRadius * 2, 0.1f, spellRadius * 2); 
            // We multiply by 2 because scale is diameter, but radius is... radius.
            // Y is 0.1f to make it a flat disc. Adjust as needed.
        }
        else
        {
            Debug.LogWarning("AreaDamageSpell: visualEffectPrefab is NOT assigned. No visual will be shown.", gameObject);
        }
        // --- END OF NEW LOGS ---

        StartCoroutine(ExecuteSpell());
    }

    private IEnumerator ExecuteSpell()
    {
        yield return new WaitForSeconds(delayBeforeDamage);

        // --- NEW DEBUG LOG ---
        Debug.Log($"AreaDamageSpell: Dealing {spellDamage} damage in a {spellRadius}m radius.", gameObject);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, spellRadius, playerLayer);
        int hitCount = 0;

        foreach (Collider hitCollider in hitColliders)
        {
            PlayerHealth player = hitCollider.GetComponent<PlayerHealth>();
            if (player != null && !player.isDead && player.teamID != casterTeamID)
            {
                player.TakeDamage(spellDamage);
                hitCount++;
            }
        }

        if(hitCount > 0)
        {
            Debug.Log($"AreaDamageSpell: Hit {hitCount} enemies.", gameObject);
        }

        yield return new WaitForSeconds(spellDuration - delayBeforeDamage);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, spellRadius);
    }
}

