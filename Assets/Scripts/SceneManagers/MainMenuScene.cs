using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuScene : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button findLobbyButton;
    [SerializeField] private Button exitButton;

    [Header("Popup")]
    [SerializeField] private StringInputPopup stringInputPopup;

    [Header("Info")]
    [SerializeField] private TextMeshProUGUI playerNameLabel;

    private void Start()
    {
        if (!Authentication.IsAuthenticated)
        {
            Action<string> callback = async playerName =>
            {
                bool succes = await Authentication.AuthenticateAsync(playerName);
                if (succes)
                {
                    playerNameLabel.text = $"Player Name: {playerName}";
                    Debug.Log($"Successfully authenticated (playerName: {playerName})");
                }
                else Debug.LogError($"Authentication failed (playerName: {playerName})");
            };

            stringInputPopup.Show(callback, "Enter player name...");
        }
        else
            playerNameLabel.text = $"Player Name: {Authentication.PlayerName}";

        createLobbyButton.onClick.AddListener(OnCreateLobbyButtonClicked);
        findLobbyButton.onClick.AddListener(OnFindLobbyButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    private void OnCreateLobbyButtonClicked()
    {
        Action<string> callback = async lobbyName =>
        {
            bool succes = await LobbyManager.Instance.CreateLobbyAsync(lobbyName);
            if (succes) Debug.Log($"Lobby created (lobbyName: {lobbyName})");
            else        Debug.LogError($"Lobby create failed (lobbyName: {lobbyName})");

            succes = await RelayManager.Instance.CreateAllocationAsync();
            if (succes)
            {
                GUIUtility.systemCopyBuffer = LobbyManager.Instance.CurrentLobby.LobbyCode;

                Debug.Log($"Relay created (joinCode: {RelayManager.Instance.JoinCode})");
                await LobbyManager.Instance.SetRelayJoinCodeAsync(RelayManager.Instance.JoinCode);
            }
            else
            {
                Debug.LogError("Relay create failed");
                await LobbyManager.Instance.DisconnectAsync();
                Debug.LogError($"Disconnected from lobby (lobbyName: {lobbyName})");
            }
        };

        stringInputPopup.Show(callback, "Enter lobby name...");
    }

    private void OnFindLobbyButtonClicked()
    {
        Action<string> callback = async lobbyCode =>
        {
            bool succes = await LobbyManager.Instance.JoinLobbyByCodeAsync(lobbyCode);
            if (succes) Debug.Log($"Lobby joined (lobbyCode: {lobbyCode})");
            else        Debug.LogError($"Lobby join failed (lobbyCode: {lobbyCode})");

            string relayJoinCode = LobbyManager.Instance.RelayJoinCode;
            succes = await RelayManager.Instance.JoinAllocationByCodeAsync(relayJoinCode);

            if (succes) Debug.Log($"Relay joined (joinCode: {relayJoinCode})");
            else Debug.LogError($"Relay join failed (joinCode: {relayJoinCode})");
        };

        stringInputPopup.Show(callback, "Enter lobby code...");
    }

    private void OnExitButtonClicked() => Application.Quit();
}