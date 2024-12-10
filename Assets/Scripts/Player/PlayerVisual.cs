using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private TextMeshPro playerName;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private HealthSystem healthSystem;
    private Coroutine getDamageCoroutine;

    private bool isEventsSubscribed;

    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(PlayerController.Instance.transform.position - transform.position);
    }

    public void Init(HealthSystem healthSystem)
    {
        this.healthSystem = healthSystem;
        SubscribeEvents();
    }

    public void Enable()
    {
        gameObject.SetActive(true);
        SubscribeEvents();
    }

    public void Disable()
    {
        gameObject.SetActive(false);
        UnsubscribeEvents();
    }

    public void SetPlayerName(string value)
    {
        playerName.text = value;
    }

    public void PlayGetDamageAnimation()
    {
        if (!gameObject.activeInHierarchy) return;

        if (getDamageCoroutine != null)
            StopCoroutine(getDamageCoroutine);

        getDamageCoroutine = StartCoroutine(GetDamageAnimation());
    }

    private IEnumerator GetDamageAnimation()
    {
        Color color = new(1f, 1f, 1f, 0f);
        while (color.a < 1f)
        {
            color.a += Time.deltaTime;
            spriteRenderer.color = color;
            yield return null;
        }
    }

    private void SubscribeEvents()
    {
        if (isEventsSubscribed) return;

        healthSystem.OnDamaged += PlayGetDamageAnimation;

        isEventsSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!isEventsSubscribed) return;

        healthSystem.OnDamaged -= PlayGetDamageAnimation;

        isEventsSubscribed = false;
    }
}