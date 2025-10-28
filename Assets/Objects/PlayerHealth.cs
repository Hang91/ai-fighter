using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Required for UI elements like Slider
using TMPro; // Required for TextMeshPro

/// <summary>
/// Manages the health of a player (or any object).
/// Handles taking damage, playing damage/death animations, and updating the health bar.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Renderer))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Stats")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    
    public bool isDead { get; private set; } = false;

    // A simple event to notify other scripts (like the attacker) that this player has died.
    public event System.Action OnDeath;

    [Header("Animation")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float damageFlashTime = 0.1f;

    [Header("UI (Health Bar)")]
    [Tooltip("Drag the child Canvas GameObject here")]
    [SerializeField] private GameObject healthBarCanvas; 
    [Tooltip("Drag the Slider component from the child canvas")]
    [SerializeField] private Slider healthSlider;
    [Tooltip("Drag the TextMeshProUGUI component from the child canvas")]
    [SerializeField] private TextMeshProUGUI hpText; // Changed from Text to TextMeshProUGUI
    
    // Component References
    private Renderer rend;
    private Rigidbody rb;
    private Color originalColor;
    
    private Camera mainCamera; // To make the health bar face the camera

    void Awake()
    {
        // Get component references for animations
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
        
        mainCamera = Camera.main; // Find the main camera
    }

    void Start()
    {
        currentHealth = maxHealth;
        isDead = false;
        
        // --- Initialize Health Bar ---
        UpdateHealthBar();
        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(true);
        }
    }
    
    void LateUpdate()
    {
        // Make the health bar always face the camera
        // We use LateUpdate to ensure the camera has finished its movement for the frame
        if (!isDead && healthBarCanvas != null && mainCamera != null)
        {
            // This makes the canvas look directly at the camera
            healthBarCanvas.transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }

    /// <summary>
    /// Public method to apply damage to this player.
    /// </summary>
    /// <param name="damageAmount">The amount of damage to take.</param>
    public void TakeDamage(float damageAmount)
    {
        // Don't take damage if already dead
        if (isDead)
        {
            return;
        }

        currentHealth -= damageAmount;
        
        Debug.Log(gameObject.name + " takes " + damageAmount + " damage. " + currentHealth + "/" + maxHealth + " HP remaining.");

        // --- Play Damage Animation ---
        StartCoroutine(FlashDamageEffect());
        
        // --- Update Health Bar ---
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            // Update the slider's value (normalized between 0 and 1)
            healthSlider.value = currentHealth / maxHealth;
        }

        if (hpText != null)
        {
            // Update the text to show "current / max"
            // The ":0" formatting removes decimal places
            hpText.text = $"{currentHealth:0} / {maxHealth:0}";
        }
    }

    /// <summary>
    /// Coroutine for the "flash red" damage effect.
    /// </summary>
    private IEnumerator FlashDamageEffect()
    {
        rend.material.color = damageColor;
        yield return new WaitForSeconds(damageFlashTime);
        rend.material.color = originalColor;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log(gameObject.name + " has been defeated!");

        // Trigger the OnDeath event
        OnDeath?.Invoke();

        // --- Hide Health Bar ---
        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(false);
        }

        // --- Play Defeat Animation ---
        // We make the cube tumble over by unfreezing its rotation
        // and giving it a little push.
        if (rb != null)
        {
            // Unfreeze rotation so it can tumble
            rb.constraints = RigidbodyConstraints.None;

            // Give it a small push upwards and backwards to make it topple
            rb.AddForce(Vector3.up * 2f - transform.forward * 1f, ForceMode.Impulse);
            // Give it some spin
            rb.AddTorque(transform.right * 10f, ForceMode.Impulse);
        }
    }
}

