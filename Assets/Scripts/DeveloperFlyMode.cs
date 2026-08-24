using UnityEngine;

/// <summary>
/// Дебаг-камера для облёта карты. Не персонаж: noclip, без гравитации и без CharacterController.
/// F1 — вкл/выкл. Пока включена: PlayerMovement выключен.
/// WASD — полёт по взгляду, Space/E — вверх, Ctrl/Q — вниз, Shift — ускорение, колесо — скорость.
/// </summary>
public class DeveloperFlyMode : MonoBehaviour
{
    [Header("Управление")]
    public KeyCode toggleKey = KeyCode.F1;
    public bool startEnabled = true;
    public float moveSpeed = 25f;
    public float fastMultiplier = 4f;
    public float lookSensitivity = 2.2f;
    public float minPitch = -89f;
    public float maxPitch = 89f;

    [Header("Камера (если пусто — дочерняя или Main)")]
    public Camera flyCamera;

    bool flyEnabled;
    float yaw;
    float pitch;
    float currentSpeed;

    CharacterController characterController;
    PlayerMovement playerMovement;
    Transform pitchPivot;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();

        if (flyCamera == null)
            flyCamera = GetComponentInChildren<Camera>();
        if (flyCamera == null)
            flyCamera = Camera.main;

        pitchPivot = flyCamera != null ? flyCamera.transform : transform;

        Vector3 euler = transform.eulerAngles;
        yaw = euler.y;
        pitch = 0f;
        currentSpeed = moveSpeed;

        flyEnabled = startEnabled;
        ApplyFlyState();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            flyEnabled = !flyEnabled;
            ApplyFlyState();
            Debug.Log(flyEnabled
                ? "Debug Fly: ВКЛ (F1 выключить)"
                : "Debug Fly: ВЫКЛ (F1 включить)");
        }

        if (!flyEnabled)
            return;

        yaw += Input.GetAxisRaw("Mouse X") * lookSensitivity;
        pitch -= Input.GetAxisRaw("Mouse Y") * lookSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Тело только по Y — не задираем игрока вверх. Наклон только у камеры.
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (pitchPivot != null && pitchPivot != transform)
            pitchPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
            currentSpeed = Mathf.Clamp(currentSpeed * (scroll > 0f ? 1.15f : 0.87f), 2f, 200f);

        float speed = currentSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            speed *= fastMultiplier;

        Vector3 local = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) local += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) local += Vector3.back;
        if (Input.GetKey(KeyCode.A)) local += Vector3.left;
        if (Input.GetKey(KeyCode.D)) local += Vector3.right;

        float up = 0f;
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space)) up += 1f;
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl)) up -= 1f;

        Vector3 world = Vector3.zero;
        if (local.sqrMagnitude > 0.001f)
        {
            local.Normalize();
            // Полёт туда, куда смотрит камера (включая вверх/вниз по взгляду)
            Quaternion look = Quaternion.Euler(pitch, yaw, 0f);
            world += look * local;
        }

        world += Vector3.up * up;

        if (world.sqrMagnitude > 0.001f)
            transform.position += world.normalized * speed * Time.unscaledDeltaTime;
    }

    void ApplyFlyState()
    {
        if (playerMovement != null)
            playerMovement.enabled = !flyEnabled;

        if (characterController != null)
            characterController.enabled = !flyEnabled;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnGUI()
    {
        if (!flyEnabled) return;

        const int pad = 12;
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.Box(new Rect(pad, pad, 420, 78), GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(new Rect(pad + 10, pad + 8, 400, 22), "DEBUG FLY  |  F1 выключить");
        GUI.Label(new Rect(pad + 10, pad + 30, 400, 40),
            "WASD лететь  |  Space/E вверх  |  Ctrl/Q вниз\nShift ускорение  |  колесо — скорость  " + currentSpeed.ToString("0"));
    }
}
