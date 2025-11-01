using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Required for UI elements like Slider
using TMPro; // Required for TextMeshPro

/// <summary>
/// Manages player health, team, animations, and UI.
/// Reports its death to the GameManager.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Renderer))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Team")]
    [SerializeField] public int teamID = 1; // Set this to 1 or 2 in the Inspector

    [Header("Health Stats")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    
    public bool isDead { get; private set; } = false;

    // Event that passes the teamID of the player who died
    public event System.Action<int> OnPlayerDied;

    [Header("Animation")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float damageFlashTime = 0.1f;

    [Header("UI (Health Bar)")]
    [Tooltip("Drag the child Canvas GameObject here")]
    [SerializeField] private GameObject healthBarCanvas; 
    [Tooltip("Drag the Slider component from the child canvas")]
    [SerializeField] private Slider healthSlider;
    [Tooltip("Drag the TextMeshProUGUI component from the child canvas")]
    [SerializeField] private TextMeshProUGUI hpText;
    
    // Component References
    private Renderer rend;
    private Rigidbody rb;
    private Color originalColor;
    private Camera mainCamera; 

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
        mainCamera = Camera.main; 
    }

    void Start()
    {
        currentHealth = maxHealth;
        isDead = false;
        UpdateHealthBar();
        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(true);
        }
    }
    
    void LateUpdate()
    {
        if (!isDead && healthBarCanvas != null && mainCamera != null)
        {
            healthBarCanvas.transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " (Team " + teamID + ") takes " + damageAmount + " damage.");
        
        StartCoroutine(FlashDamageEffect());
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
            healthSlider.value = currentHealth / maxHealth;
        }
        if (hpText != null)
        {
            hpText.text = $"{currentHealth:0} / {maxHealth:0}";
        }
    }

    private IEnumerator FlashDamageEffect()
    {
        rend.material.color = damageColor;
        yield return new WaitForSeconds(damageFlashTime);
        rend.material.color = originalColor;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log(gameObject.name + " (Team " + teamID + ") has been defeated!");

        // --- Report death to the GameManager ---
        OnPlayerDied?.Invoke(teamID);

        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(false);
        }

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.AddForce(Vector3.up * 2f - transform.forward * 1f, ForceMode.Impulse);
            rb.AddTorque(transform.right * 10f, ForceMode.Impulse);
        }
    }
}

