using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float rotationSpeed = 500f;

    private Rigidbody rb;
    private Transform cameraTransform;
    private PlayerInput input;

    private bool isInputActionsSubscribed;
    private Vector2 rotation;

    public bool IsInitialized { get; private set; }

    public void Init(Camera camera, Rigidbody rigidbody, PlayerInput input)
    {
        cameraTransform = camera.transform;
        rb = rigidbody;

        this.input = input;
        SubscribeInputActions();

        IsInitialized = true;
    }

    public void Enable()
    {
        if (!IsInitialized) return;

        SubscribeInputActions();
    }

    public void Disable()
    {
        if (!IsInitialized) return;

        rb.velocity = Vector3.zero;
        UnsubscribeInputActions();
    }

    private void Rotate(Vector2 rotationInput)
    {
        rotation += rotationSpeed * Time.deltaTime * rotationInput;
        rotation.x = Mathf.Clamp(rotation.x, -90f, 90f);

        cameraTransform.eulerAngles = rotation;
    }

    private void Move(Vector3 moveInput)
    {
        Vector3 direction = moveInput.z * cameraTransform.forward + moveInput.x * cameraTransform.right + moveInput.y * Vector3.up;
        rb.AddForce(moveSpeed * direction, ForceMode.VelocityChange);
    }

    private void SubscribeInputActions()
    {
        if (isInputActionsSubscribed) return;

        input.OnMove += Move;
        input.OnRotate += Rotate;

        isInputActionsSubscribed = true;
    }

    private void UnsubscribeInputActions()
    {
        if (!isInputActionsSubscribed) return;

        input.OnMove -= Move;
        input.OnRotate -= Rotate;

        isInputActionsSubscribed = false;
    }
}