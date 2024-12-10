using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class HealthSystem : NetworkBehaviour
{
    public event Action OnDamaged;
    public event Action OnDied;

    private float maxHealth = 10f;
    //private NetworkVariable<float> health = new();

    public float Health { get; private set; }
    public bool IsAlive => Health > 0f;

    private void Awake()
    {
        Health = maxHealth;
    }

    public void Damage(float damage)
    {
        if (!IsSpawned || damage <= 0f) return;

        Health -= damage;
        OnDamaged?.Invoke();

        if (Health <= 0f)
        {
            Health = 0f;
            OnDied?.Invoke();
        }

        DamageServerRpc(damage, new ServerRpcParams());
    }

    [ServerRpc(RequireOwnership = false)]
    private void DamageServerRpc(float damage, ServerRpcParams rpcParams)
    {
        List<ulong> targetClientIds = NetworkManager.ConnectedClientsIds.ToList();

        if (rpcParams.Receive.SenderClientId != NetworkManager.LocalClientId)
        {
            Health -= damage;
            OnDamaged?.Invoke();

            if (Health <= 0f)
            {
                Health = 0f;
                OnDied?.Invoke();
            }

            targetClientIds.Remove(rpcParams.Receive.SenderClientId);
            targetClientIds.Remove(NetworkManager.LocalClientId);
        }
        else
        {
            targetClientIds.Remove(rpcParams.Receive.SenderClientId);
        }

        DamageClientRpc(damage, new ClientRpcParams()
        {
            Send = new ClientRpcSendParams()
            {
                TargetClientIds = targetClientIds
            }
        });
    }

    [ClientRpc]
    private void DamageClientRpc(float damage, ClientRpcParams rpcParams)
    {
        Health -= damage;
        OnDamaged?.Invoke();

        if (Health <= 0f)
        {
            Health = 0f;
            OnDied?.Invoke();
        }
    }

    public void ResetHealth()
    {
        if (!IsSpawned) return;

        Health = maxHealth;

        ResetHealthServerRpc(new ServerRpcParams());
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetHealthServerRpc(ServerRpcParams rpcParams)
    {
        List<ulong> targetClientIds = NetworkManager.ConnectedClientsIds.ToList();

        if (rpcParams.Receive.SenderClientId != NetworkManager.LocalClientId)
        {
            Health = maxHealth;

            targetClientIds.Remove(rpcParams.Receive.SenderClientId);
            targetClientIds.Remove(NetworkManager.LocalClientId);
        }
        else
        {
            targetClientIds.Remove(rpcParams.Receive.SenderClientId);
        }

        ResetHealthClientRpc(new ClientRpcParams()
        {
            Send = new ClientRpcSendParams()
            {
                TargetClientIds = targetClientIds
            }
        });
    }

    [ClientRpc]
    private void ResetHealthClientRpc(ClientRpcParams rpcParams)
    {
        Health = maxHealth;
    }
}