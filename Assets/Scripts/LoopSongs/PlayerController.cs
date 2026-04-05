using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;

    [Header("Salto")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    private CharacterController _controller;
    private Vector3 _verticalVelocity;
    private bool _isGrounded;

    private InputSystem_Actions _input;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _input = new InputSystem_Actions();
    }

    private void OnEnable()  => _input.Player.Enable();
    private void OnDisable() => _input.Player.Disable();

    private void Update()
    {
        _isGrounded = _controller.isGrounded;

        if (_isGrounded && _verticalVelocity.y < 0f)
            _verticalVelocity.y = -2f;

        // Movimiento horizontal
        Vector2 moveInput = _input.Player.Move.ReadValue<Vector2>();
        float speed = _input.Player.Sprint.IsPressed() ? runSpeed : walkSpeed;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        _controller.Move(move * speed * Time.deltaTime);

        // Salto
        if (_input.Player.Jump.WasPressedThisFrame() && _isGrounded)
            _verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // Gravedad
        _verticalVelocity.y += gravity * Time.deltaTime;
        _controller.Move(_verticalVelocity * Time.deltaTime);
    }
}
