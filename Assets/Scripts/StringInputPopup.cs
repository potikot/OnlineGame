using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StringInputPopup : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button cancelButton;

    private TextMeshProUGUI placeholderLabel;

    private Action<string> onSubmit;

    private void Awake()
    {
        placeholderLabel = inputField.placeholder.GetComponent<TextMeshProUGUI>();

        Hide();
    }

    private void OnEnable()
    {
        submitButton.onClick.AddListener(OnSubmit);
        inputField.onSubmit.AddListener(OnSubmit);

        cancelButton.onClick.AddListener(Hide);
    }

    private void OnDisable()
    {
        submitButton.onClick.RemoveListener(OnSubmit);
        inputField.onSubmit.RemoveListener(OnSubmit);
    }

    public void Show(Action<string> callback, string placeholderText = "Enter text...", bool hide = true)
    {
        inputField.SetTextWithoutNotify(string.Empty);
        placeholderLabel.text = placeholderText;

        onSubmit = callback;

        if (hide)
        {
            if (onSubmit == null)
                onSubmit = text => Hide();
            else
                onSubmit += text => Hide();
        }

        gameObject.SetActive(true);

        inputField.ActivateInputField();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }


    private void OnSubmit() => OnSubmit(inputField.text);

    private void OnSubmit(string text)
    {
        onSubmit?.Invoke(text);
    }
}