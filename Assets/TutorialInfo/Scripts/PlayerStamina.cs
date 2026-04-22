using UnityEngine;
using UnityEngine.Events; // Necesario para los eventos

public class PlayerStamina : MonoBehaviour
{
    [Header("Configuración de Estamina")]
    public float maxStamina = 100f;
    public float currentStamina { get; private set; }

    [Tooltip("Cuánta estamina se recupera por segundo al estar quieto o caminando")]
    public float regenRate = 20f;

    [Header("Eventos")]
    [Tooltip("Se dispara cuando la estamina llega a 0 (Ideal para soltarse del Wall-run)")]
    public UnityEvent OnStaminaDepleted;

    private void Start()
    {
        // Inicializamos la estamina al máximo al empezar
        currentStamina = maxStamina;
    }

    /// <summary>
    /// Consume estamina de forma continua (ej: durante el Wall-run o al Correr).
    /// Debe llamarse dentro de un Update en el script de movimiento.
    /// </summary>
    public void ConsumeStamina(float amountPerSecond)
    {
        if (currentStamina > 0)
        {
            currentStamina -= amountPerSecond * Time.deltaTime;

            // Si llega a 0 o menos, forzamos el límite y avisamos al resto del juego
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                OnStaminaDepleted?.Invoke(); // Llama a cualquier función conectada aquí
            }
        }
    }

    /// <summary>
    /// Consume una cantidad de estamina de golpe (ej: al hacer el Dash).
    /// Devuelve 'true' si había suficiente estamina, o 'false' si no se pudo ejecutar.
    /// </summary>
    public bool TryConsumeBurst(float burstAmount)
    {
        if (currentStamina >= burstAmount)
        {
            currentStamina -= burstAmount;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Regenera la estamina. 
    /// Debe llamarse cuando el jugador está en estado Idle o Caminando.
    /// </summary>
    public void RegenerateStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += regenRate * Time.deltaTime;
            if (currentStamina > maxStamina)
            {
                currentStamina = maxStamina;
            }
        }
    }

    // --- MÉTODOS DE UTILIDAD PARA LA UI ---

    // Devuelve un valor entre 0 y 1, ideal para barras de progreso en la UI
    public float GetStaminaNormalized()
    {
        return currentStamina / maxStamina;
    }
}