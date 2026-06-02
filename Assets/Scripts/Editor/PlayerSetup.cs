#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerSetup : EditorWindow
{
    [MenuItem("Tools/Setup 3D Player")]
    public static void SetupPlayer()
    {
        // 1. Create a 3D Capsule for the Player
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0, 1f, 0);

        // Remove the default capsule collider and add our own if needed, 
        // or just keep it. It has one by default.
        
        // 2. Attach a Rigidbody and freeze X/Z rotations
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb == null)
            rb = player.AddComponent<Rigidbody>();
        
        rb.mass = 1f;
        rb.drag = 0f;
        rb.angularDrag = 0.05f;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        // Freeze X and Z rotations
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;

        // 3. Create the Orientation child object
        GameObject orientation = new GameObject("Orientation");
        orientation.transform.SetParent(player.transform);
        orientation.transform.localPosition = Vector3.zero;

        // 4. Attach all generated scripts and automatically link the references
        PlayerStamina staminaScript = player.AddComponent<PlayerStamina>();
        PlayerMovement movementScript = player.AddComponent<PlayerMovement>();
        PlayerDash dashScript = player.AddComponent<PlayerDash>();
        WallRunner wallRunnerScript = player.AddComponent<WallRunner>();

        // Link Orientation
        movementScript.orientation = orientation.transform;
        dashScript.orientation = orientation.transform;
        wallRunnerScript.orientation = orientation.transform;

        // Set up layers if they don't exist, fallback to "Default" for wall/ground just so it's not empty
        movementScript.whatIsGround = 1; // Default layer
        wallRunnerScript.whatIsWall = 1; // Default layer
        wallRunnerScript.whatIsGround = 1; // Default layer

        // 5. Create a basic Canvas with a Stamina UI Slider and link it
        GameObject canvasObj = new GameObject("PlayerCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create Slider
        GameObject sliderObj = new GameObject("StaminaSlider");
        sliderObj.transform.SetParent(canvasObj.transform);
        
        Slider slider = sliderObj.AddComponent<Slider>();
        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0);
        sliderRect.anchorMax = new Vector2(0.5f, 0);
        sliderRect.pivot = new Vector2(0.5f, 0);
        sliderRect.anchoredPosition = new Vector2(0, 50); // 50px from bottom
        sliderRect.sizeDelta = new Vector2(300, 20);

        // Add background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        RectTransform bgRect = bgImg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Add Fill Area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform);
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(5, 5);
        fillAreaRect.offsetMax = new Vector2(-5, -5);

        // Add Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform);
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = Color.green;
        RectTransform fillRect = fillImg.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        slider.fillRect = fillRect;
        slider.interactable = false; // Just for display
        slider.transition = Selectable.Transition.None;

        // Setup Event System if missing
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        // Link Slider to PlayerStamina directly
        staminaScript.staminaSlider = slider;

        // Select the newly created player object
        Selection.activeGameObject = player;
        
        Debug.Log("3D Player successfully set up! Please ensure you assign the correct LayerMasks (whatIsGround, whatIsWall) on the Player scripts.");
    }
}
#endif
