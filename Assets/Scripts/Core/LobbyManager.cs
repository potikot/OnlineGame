using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    private bool isSendHeartBeatPing = false;
    private float heartBeatTimeout = 15f;
    private float heartBeatTimer;

    public static LobbyManager Instance { get; private set; }

    public event Action<List<string>> OnPlayerNamesUpdated;

    public Lobby CurrentLobby { get; private set; }
    public List<string> PlayerNames { get; private set; } = new();

    public bool InLobby => CurrentLobby != null;
    public string RelayJoinCode => CurrentLobby == null ? string.Empty : CurrentLobby.Data[KEY_RELAY_JOIN_CODE].Value;

    private bool IsServer => NetworkManager.Singleton.IsServer;

    public const string KEY_PLAYER_NAME = "pn";
    public const string KEY_RELAY_JOIN_CODE = "rjc";

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void Update() => ProcessHeartBeatPing();

    public async Task<bool> CreateLobbyAsync(string lobbyName, int maxPlayers = 5)
    {
        if (InLobby || string.IsNullOrEmpty(lobbyName))
            return false;

        CreateLobbyOptions createLobbyOptions = new()
        {
            IsPrivate = false,
            Player = GeneratePlayerData(),
            Data = GenerateLobbyData()
        };

        try
        {
            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            UpdatePlayerNames();

            return true;
        }
        catch (LobbyServiceException exception)
        {
            Debug.LogException(exception);
            return false;
        }
    }

    public async Task<bool> JoinLobbyByCodeAsync(string joinCode)
    {
        if (InLobby || string.IsNullOrEmpty(joinCode))
            return false;

        JoinLobbyByCodeOptions joinLobbyByCodeOptions = new()
        {
            Player = GeneratePlayerData()
        };

        try
        {
            CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode, joinLobbyByCodeOptions);
            UpdatePlayerNames();

            return true;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        if (!InLobby)
            return;

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, Authentication.PlayerId);
            CurrentLobby = null;
            UpdatePlayerNames();
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
    }

    public async Task SetRelayJoinCodeAsync(string relayJoinCode)
    {
        if (!InLobby || !IsServer)
            return;

        UpdateLobbyOptions updateLobbyOptions = new()
        {
            Data = new Dictionary<string, DataObject>
            {
                { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
            }
        };

        try
        {
            CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, updateLobbyOptions);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
    }

    private async void ProcessHeartBeatPing()
    {
        if (!isSendHeartBeatPing || CurrentLobby == null)
            return;

        heartBeatTimer += Time.deltaTime;
        if (heartBeatTimer >= heartBeatTimeout)
        {
            heartBeatTimer = 0f;

            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogException(ex);
            }
        }
    }

    public void StartHeartBeatPing()
    {
        if (!IsServer)
            return;

        isSendHeartBeatPing = true;
        heartBeatTimer = heartBeatTimeout;
    }

    public void StopHeartBeatPing()
    {
        isSendHeartBeatPing = false;
    }

    private bool isLobbyUpdating;
    public async Task UpdateLobby()
    {
        if (!InLobby || isLobbyUpdating)
            return;

        isLobbyUpdating = true;
        CurrentLobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
        isLobbyUpdating = false;
    }

    public void UpdatePlayerNames()
    {
        PlayerNames.Clear();

        if (InLobby)
            foreach (var player in CurrentLobby.Players)
                PlayerNames.Add(player.Data[KEY_PLAYER_NAME].Value);

        OnPlayerNamesUpdated?.Invoke(PlayerNames);
    }

    private Player GeneratePlayerData()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, Authentication.PlayerName) }
            }
        };
    }

    private Dictionary<string, DataObject> GenerateLobbyData()
    {
        return new Dictionary<string, DataObject>
        {
            { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, string.Empty) }
        };
    }

    private async void OnClientConnected(ulong clientId)
    {
        await UpdateLobby();
        UpdatePlayerNames();
    }

    private async void OnClientDisconnected(ulong clientId)
    {
        await UpdateLobby();
        UpdatePlayerNames();
    }
}