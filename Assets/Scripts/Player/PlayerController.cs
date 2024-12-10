using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private PlayerVisual visual;

    private PlayerInput input;

    private PlayerMovement movement;
    private PlayerAttack attack;

    private Camera cam;

    public static PlayerController Instance { get; private set; }

    public override void OnNetworkSpawn()
    {
        visual.Init(healthSystem);

        if (IsOwner)
        {
            if (Instance == null) Instance = this;
            else
            {
                Debug.LogError("Multiple owned player objects on scene");
                return;
            }

            visual.Disable();

            InstantiateComponents();
            InitializeComponents();

            healthSystem.OnDied += () =>
            {
                healthSystem.ResetHealth();
                transform.position = 3f * Vector3.up;
            };
        }
        else
        {
            SetPlayerName();

            async void SetPlayerName()
            {
                await LobbyManager.Instance.UpdateLobby();
                LobbyManager.Instance.UpdatePlayerNames();
                visual.SetPlayerName(GetOwnerPlayerName());
            }
        }
    }

    public void SetActive(bool enable)
    {
        if (enable)
        {
            input.Enable();
            movement.Enable();
        }
        else
        {
            input.Disable();
            movement.Disable();
        }
    }

    private string GetOwnerPlayerName()
    {
        int nameIndex = -1;
        IReadOnlyList<ulong> clientIds = NetworkManager.ConnectedClientsIds;
        for (int i = 0; i < clientIds.Count; i++)
        {
            if (clientIds[i] == OwnerClientId)
            {
                nameIndex = i;
                break;
            }
        }

        Debug.Log(nameIndex == -1 ? string.Empty : LobbyManager.Instance.PlayerNames[nameIndex]);
        return nameIndex == -1 ? string.Empty : LobbyManager.Instance.PlayerNames[nameIndex];
    }

    private void InitializeComponents()
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.isKinematic = false;

        visual.Init(healthSystem);
        movement.Init(cam, rigidbody, input);
        attack.Init(cam, input);
    }

    private void InstantiateComponents()
    {
        cam = InstantiateComponentAsChildren<Camera>();
        input = InstantiateComponentAsChildren<PlayerInput>();

        movement = InstantiateComponentAsChildren<PlayerMovement>();
        attack = InstantiateComponentAsChildren<PlayerAttack>();
    }

    private T InstantiateComponentAsChildren<T>() where T : Component
    {
        GameObject gameObject = new(typeof(T).Name);
        gameObject.transform.parent = transform;

        return gameObject.AddComponent<T>();
    }
}