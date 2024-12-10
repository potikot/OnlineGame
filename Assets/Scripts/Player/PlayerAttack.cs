using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private float damage = 1f;

    private Transform cameraTransform;
    private PlayerInput input;

    private bool isInputActionsSubscribed;

    public bool IsInitialized { get; private set; }

    public void Init(Camera camera, PlayerInput input)
    {
        cameraTransform = camera.transform;

        this.input = input;
        SubscribeInputActions();

        IsInitialized = true;
    }

    public void Enable() => SubscribeInputActions();
    public void Disable() => UnsubscribeInputActions();

    private void Attack()
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hitInfo, 100f))
            if (hitInfo.collider.TryGetComponent(out HealthSystem healthSystem))
                healthSystem.Damage(damage);
    }

    private void SubscribeInputActions()
    {
        if (isInputActionsSubscribed) return;

        input.OnAttack += Attack;

        isInputActionsSubscribed = true;
    }

    private void UnsubscribeInputActions()
    {
        if (!isInputActionsSubscribed) return;

        input.OnAttack -= Attack;

        isInputActionsSubscribed = false;
    }
}