using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyScene : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private GameObject canvas;

    [Header("Buttons")]
    [SerializeField] private Button exitButton;

    [Header("Info")]
    [SerializeField] private TextMeshProUGUI lobbyNameLabel;
    [SerializeField] private TextMeshProUGUI lobbyJoinCodeLabel;
    [SerializeField] private TextMeshProUGUI ownerLabel;
    [Space()]
    [SerializeField] private List<TextMeshProUGUI> playerNameLabels;

    private bool isEnabled;
    private bool isEventsSubscribed;

    private void Start()
    {
        canvas.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isEnabled)
                Disable();
            else
                Enable();
        }
    }

    private void OnDestroy()
    {
        UnsubsribeEvents();
    }

    public void Enable()
    {
        if (isEnabled) return;

        lobbyNameLabel.text = "Lobby Name: " + LobbyManager.Instance.CurrentLobby.Name;
        lobbyJoinCodeLabel.text = "Join Code: " + LobbyManager.Instance.CurrentLobby.LobbyCode;
        ownerLabel.text = "Owner: " + LobbyManager.Instance.PlayerNames[0];

        UpdatePlayerNames(LobbyManager.Instance.PlayerNames);
        SubsribeEvents();

        canvas.SetActive(true);
        Cursor.lockState = CursorLockMode.None;

        PlayerController.Instance.SetActive(false);

        isEnabled = true;
    }

    public void Disable()
    {
        if (!isEnabled) return;

        UnsubsribeEvents();

        canvas.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;

        PlayerController.Instance.SetActive(true);

        isEnabled = false;
    }

    private void UpdatePlayerNames(List<string> playerNames)
    {
        for (int i = 0; i < playerNameLabels.Count; i++)
        {
            if (i < playerNames.Count)
            {
                playerNameLabels[i].text = $"{i + 1}. {playerNames[i]}";
                playerNameLabels[i].gameObject.SetActive(true);
            }
            else
            {
                playerNameLabels[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnExitButtonClicked()
    {
        RelayManager.Instance.Disconnect();
    }

    private void SubsribeEvents()
    {
        if (isEventsSubscribed) return;

        LobbyManager.Instance.OnPlayerNamesUpdated += UpdatePlayerNames;
        exitButton.onClick.AddListener(OnExitButtonClicked);

        isEventsSubscribed = true;
    }

    private void UnsubsribeEvents()
    {
        if (!isEventsSubscribed) return;

        LobbyManager.Instance.OnPlayerNamesUpdated -= UpdatePlayerNames;
        exitButton.onClick.RemoveListener(OnExitButtonClicked);

        isEventsSubscribed = false;
    }
}