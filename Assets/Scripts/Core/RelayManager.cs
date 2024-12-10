using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    public string JoinCode { get; private set; }

    public bool IsServer => NetworkManager.Singleton.IsServer;
    public bool IsClient => NetworkManager.Singleton.IsClient;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public async Task<bool> CreateAllocationAsync(int maxPlayers = 5)
    {
        if (IsClient)
            return false;

        Allocation allocation;
        string joinCode;

        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch (RelayServiceException ex)
        {
            Debug.LogException(ex);
            return false;
        }

        UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        unityTransport.SetRelayServerData
        (
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData
        );

        JoinCode = joinCode;

        NetworkManager.Singleton.StartHost();

        return true;
    }

    public async Task<bool> JoinAllocationByCodeAsync(string joinCode)
    {
        if (IsClient)
            return false;

        JoinAllocation allocation;

        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (RelayServiceException ex)
        {
            Debug.LogException(ex);
            return false;
        }

        UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        unityTransport.SetRelayServerData
        (
            allocation.RelayServer.IpV4,
            (ushort) allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData,
            allocation.HostConnectionData
        );

        JoinCode = joinCode;

        NetworkManager.Singleton.StartClient();

        return true;
    }

    public void Disconnect()
    {
        if (!IsClient)
            return;

        NetworkManager.Singleton.Shutdown();
    }
}