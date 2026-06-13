using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 1.5f;
    public float gravity = -9.81f;
    public float mouseSensitivity = 300f;

    public Transform playerCamera;

    private float xRotation = 0f;
    private float yVelocity = 0f;

    private CharacterController controller;

    public float maxStamina = 100f;
    public float stamina = 100f;

    public float staminaDrain = 20f;
    public float staminaRegen = 15f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // --- МЫШЬ ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // --- ДВИЖЕНИЕ ---
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        bool isRunning =
            Input.GetKey(KeyCode.LeftShift)
            && stamina > 0
            && z > 0;

        float currentSpeed =
            isRunning
            ? sprintSpeed
            : speed;
        if (isRunning)
        {
            stamina -= staminaDrain * Time.deltaTime;
        }
        else
        {
            stamina += staminaRegen * Time.deltaTime;
        }

        stamina = Mathf.Clamp(
            stamina,
            0,
            maxStamina
        );

        Debug.Log(stamina);
        
        Vector3 move = transform.right * x + transform.forward * z;

        // --- ГРАВИТАЦИЯ ---
        if (controller.isGrounded && yVelocity < 0)
        {
            yVelocity = -2f;
        }

        yVelocity += gravity * Time.deltaTime;

        // --- ПРЫЖОК ---
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            yVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        Vector3 velocity = move * currentSpeed + Vector3.up * yVelocity;

        controller.Move(velocity * Time.deltaTime);

        // --- ВЗАИМОДЕЙСТВИЕ ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(playerCamera.position, playerCamera.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 3f))
            {
                Interactable interactable = hit.collider.GetComponent<Interactable>();

                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }



    }
}