using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerDash : MonoBehaviour
{
    [Header("Configuración del Dash")]
    [Tooltip("La velocidad/fuerza constante a la que se mueve el personaje durante el dash")]
    public float dashSpeed = 25f;
    [Tooltip("Cuánto tiempo en segundos el personaje ignora la gravedad")]
    public float dashDuration = 0.25f;
    [Tooltip("Cuánta estamina consume cada uso")]
    public float dashStaminaCost = 25f;
    [Tooltip("Tecla para activar el Dash")]
    public KeyCode dashKey = KeyCode.LeftAlt;

    [Header("Referencias")]
    [Tooltip("Transform que indica hacia dónde está mirando el frente del jugador")]
    public Transform orientation;

    private Rigidbody rb;
    private PlayerStamina stamina;

    // Variable pública (solo lectura) para que otros scripts sepan si estamos dasheando
    public bool isDashing { get; private set; }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Buscamos el script de estamina que creamos en el paso anterior
        stamina = GetComponent<PlayerStamina>();
    }

    private void Update()
    {
        // Solo podemos hacer dash si presionamos la tecla y no estamos ya en medio de uno
        if (Input.GetKeyDown(dashKey) && !isDashing)
        {
            AttemptDash();
        }
    }

    private void AttemptDash()
    {
        // 1. Verificamos si tenemos estamina suficiente usando el método TryConsumeBurst
        if (stamina != null && !stamina.TryConsumeBurst(dashStaminaCost))
        {
            // Opcional: Aquí podrías poner un sonido de "error" indicando falta de estamina
            return;
        }

        // 2. Iniciamos la acción
        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;

        // Guardamos el estado original de la gravedad por si a futuro tenemos zonas de gravedad 0
        bool originalGravity = rb.useGravity;

        // Desactivamos la gravedad y frenamos en seco cualquier caída en el eje Y
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Calculamos la dirección del Dash. 
        // Lee el Input (WASD) para dashear hacia los lados. Si no tocas nada, dashea hacia adelante.
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 direction = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;
        if (direction == Vector3.zero)
        {
            direction = orientation.forward;
        }

        // Aplicamos la velocidad pura y lineal
        rb.velocity = direction * dashSpeed;

        // Esperamos la fracción de segundo que dura el dash
        yield return new WaitForSeconds(dashDuration);

        // Restauramos la gravedad y el estado
        rb.useGravity = originalGravity;
        isDashing = false;

        // Freno post-dash: Reducimos un poco la inercia al terminar para no salir disparados infinitamente,
        // pero mantenemos la suficiente para encadenar con un salto (como pediste).
        rb.velocity = new Vector3(rb.velocity.x * 0.5f, rb.velocity.y, rb.velocity.z * 0.5f);
    }
}