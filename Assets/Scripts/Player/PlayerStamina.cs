using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float staminaRegenRate = 15f; // Stamina gained per second when resting
    
    [Header("Current State")]
    public float currentStamina;

    [Header("UI Reference")]
    public Slider staminaSlider;

    [Header("Events")]
    // Event fired when stamina changes, useful for custom logic
    public UnityEvent<float, float> OnStaminaChanged;

    private void Start()
    {
        currentStamina = maxStamina;
        UpdateUI();
    }

    /// <summary>
    /// Regenerates stamina over time. Should be called when the player is not consuming stamina (e.g., walking or idle).
    /// </summary>
    public void RegenerateStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
            UpdateUI();
        }
    }

    /// <summary>
    /// Consumes stamina in a burst (e.g., for a dash).
    /// </summary>
    /// <param name="amount">The amount of stamina to consume.</param>
    /// <returns>True if there was enough stamina, false otherwise.</returns>
    public bool ConsumeStaminaBurst(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Consumes stamina continuously over time (e.g., for sprinting or wall-running).
    /// </summary>
    /// <param name="rate">The amount of stamina to consume per second.</param>
    /// <returns>True if stamina is still available (greater than zero).</returns>
    public bool ConsumeStaminaContinuous(float rate)
    {
        if (currentStamina > 0)
        {
            currentStamina -= rate * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0);
            UpdateUI();
            return currentStamina > 0;
        }
        return false;
    }

    public bool HasStamina(float amount)
    {
        return currentStamina >= amount;
    }

    private void UpdateUI()
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina / maxStamina;
        }
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }
}
