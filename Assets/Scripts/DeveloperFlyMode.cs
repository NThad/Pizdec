using UnityEngine;

public class DeveloperFlyMode : MonoBehaviour
{
    [Header("Developer Fly Mode")]
    public float moveSpeed = 50f;        // Обычная скорость
    public float fastMoveSpeed = 120f;   // Ускоренная (Shift)
    public float lookSpeed = 3f;

    private bool isEnabled = true;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.5f;
        }

        // Убираем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!isEnabled) return;

        // Переключение режима (F1)
        if (Input.GetKeyDown(KeyCode.F1))
        {
            isEnabled = !isEnabled;
            Cursor.lockState = isEnabled ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isEnabled;
            Debug.Log("Developer Fly Mode: " + (isEnabled ? "ВКЛ" : "ВЫКЛ"));
            return;
        }

        if (!isEnabled) return;

        // Движение
        float speed = Input.GetKey(KeyCode.LeftShift) ? fastMoveSpeed : moveSpeed;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move * speed * Time.deltaTime);

        // Полёт вверх/вниз
        if (Input.GetKey(KeyCode.Space))
            controller.Move(Vector3.up * speed * Time.deltaTime);
        if (Input.GetKey(KeyCode.LeftControl))
            controller.Move(Vector3.down * speed * Time.deltaTime);

        // Вращение камеры
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        transform.Rotate(Vector3.up * mouseX, Space.World);
        transform.Rotate(Vector3.left * mouseY, Space.Self);
    }
}