using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootScene : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.Singleton.OnClientStarted += () =>
        {
            if (NetworkManager.Singleton.IsServer)
                NetworkManager.Singleton.SceneManager.LoadScene("3D Lobby", LoadSceneMode.Single);
        };

        NetworkManager.Singleton.OnClientStopped += async isHost =>
        {
            await LobbyManager.Instance.DisconnectAsync();
            SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        };

        SceneManager.LoadScene("Menu");
    }
}