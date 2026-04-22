using UnityEngine;
using UnityEngine.UI; // Para interactuar con Canvas/Sliders

public class StaminaUI : MonoBehaviour
{
    [Tooltip("Arrastra aquí el script de PlayerStamina de tu jugador")]
    public PlayerStamina playerStamina;

    [Tooltip("Arrastra aquí el Slider de tu Canvas")]
    public Slider staminaSlider;

    private void Update()
    {
        if (playerStamina != null && staminaSlider != null)
        {
            // Actualiza la barra usando el valor normalizado (0 a 1)
            staminaSlider.value = playerStamina.GetStaminaNormalized();
        }
    }
}