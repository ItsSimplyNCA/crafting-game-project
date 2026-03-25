using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPSController : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Transform cameraHolder;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -25f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 2.2f;
    [SerializeField] private float maxLookAngle = 85f;
    [SerializeField] private bool lockCursorOnStart = true;

    private CharacterController controller;
    private float verticalVelocity;
    private float cameraPitch;
    private bool inputEnabled = true;

    public bool InputEnabled => inputEnabled;

    private void Awake() {
        controller = GetComponent<CharacterController>();

        if (cameraHolder == null && Camera.main != null) {
            cameraHolder = Camera.main.transform;
        }

        if (lockCursorOnStart) {
            LockCursor();
        }
    }

    private void Update() {
        if (!inputEnabled) {
            return;
        }

        Look();
        Move();
    }

    public void SetInputEnabled(bool enabled) {
        inputEnabled = enabled;
    }

    private void Look() {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);

        if (cameraHolder != null) {
            cameraHolder.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }

        transform.Rotate(Vector3.up * mouseX);
    }

    private void Move() {
        if (controller.isGrounded && verticalVelocity < 0f) {
            verticalVelocity = -2f;
        }

        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z).normalized * speed;

        if (Input.GetButtonDown("Jump") && controller.isGrounded) {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    public static void LockCursor() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public static void UnlockCursor() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
