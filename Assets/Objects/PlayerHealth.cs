using UnityEngine;
using UnityEngine.UI;
using TMPro; // Using TextMeshPro
using System.Collections;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))] // Make sure there is a collider
public class PlayerHealth : MonoBehaviour
{
    [Header("Team")]
    public int teamID = 1;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    public float currentHealth { get; private set; }
    public bool isDead { get; private set; }

    [Header("UI References")]
    [SerializeField] private Canvas healthBarCanvas;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI hpText; // Changed from Text to TextMeshProUGUI

    [Header("Damage Effect")]
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float damageFlashDuration = 0.15f;

    private Renderer playerRenderer;
    private Rigidbody rb;
    private Color originalColor;
    private Collider col; // Reference to the player's collider

    // Event to announce death
    public delegate void PlayerDied(int teamID);
    public event PlayerDied OnPlayerDied;

    void Awake()
    {
        playerRenderer = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>(); // Get the collider
        originalColor = playerRenderer.material.color;
        currentHealth = maxHealth;
        isDead = false;

        UpdateHealthUI();
    }

    void Update()
    {
        // Keep the health bar facing the camera
        // Only do this if we are alive
        if (!isDead && healthBarCanvas != null && Camera.main != null)
        {
            healthBarCanvas.transform.LookAt(Camera.main.transform);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();
        
        // Only flash if not dead
        if (currentHealth > 0)
        {
            StartCoroutine(FlashDamageEffect());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }
        if (hpText != null)
        {
            hpText.text = $"{currentHealth:F0} / {maxHealth:F0}";
        }
    }

    private IEnumerator FlashDamageEffect()
    {
        playerRenderer.material.color = damageFlashColor;
        yield return new WaitForSeconds(damageFlashDuration);
        playerRenderer.material.color = originalColor;
    }

    private void Die()
    {
        if (isDead) return; // Prevent Die() from being called multiple times
        isDead = true;

        Debug.Log(gameObject.name + " has been defeated.");

        // --- NEW DEATH: DISAPPEAR ---
        
        // 1. Disable all components that make the player "active"
        if (col != null) col.enabled = false; // Stops blocking movement and attacks
        if (playerRenderer != null) playerRenderer.enabled = false; // Becomes invisible
        if (rb != null) rb.isKinematic = true; // Stops all physics calculations
        if (healthBarCanvas != null) healthBarCanvas.gameObject.SetActive(false); // Hides UI
        
        // Disable this script to stop the Update() loop from running
        this.enabled = false;

        // Disable the attacker script so it stops its loops
        AutoAttacker attacker = GetComponent<AutoAttacker>();
        if(attacker != null) attacker.enabled = false;
        
        // 2. Announce death to GameManager
        OnPlayerDied?.Invoke(teamID);

        // 3. Destroy the object after a delay to ensure all scripts finish
        Destroy(gameObject, 2f); 
        // --- END NEW ---
    }
}

