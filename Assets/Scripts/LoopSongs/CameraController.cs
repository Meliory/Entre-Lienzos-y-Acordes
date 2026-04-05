using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Sensibilidad")]
    [SerializeField] private float sensitivityX = 0.15f;
    [SerializeField] private float sensitivityY = 0.15f;

    [Header("Límite vertical")]
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    [Tooltip("El Transform del cuerpo del jugador (rota horizontalmente)")]
    [SerializeField] private Transform playerBody;

    private float _pitch; // Rotación vertical acumulada

    private InputSystem_Actions _input;

    private void Awake()
    {
        _input = new InputSystem_Actions();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()  => _input.Player.Enable();
    private void OnDisable() => _input.Player.Disable();

    private void Update()
    {
        Vector2 look = _input.Player.Look.ReadValue<Vector2>();

        // Rotación vertical (cámara)
        _pitch -= look.y * sensitivityY;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

        // Rotación horizontal (cuerpo del jugador)
        playerBody.Rotate(Vector3.up * look.x * sensitivityX);
    }
}
