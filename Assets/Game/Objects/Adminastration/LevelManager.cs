using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System;


public class LevelManager : NetworkBehaviour
{
    [Header("Settings")]
    public GameObject playerPrefab;
    public float clientSpawnDelay = 0.5f;
    private List<Transform> activePlayers = new List<Transform>();
    public event Action onPlayerRegistered;


    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        if (playerPrefab == null)
        {
            Debug.LogError(">>> LevelManager: FEHLER! Player Prefab fehlt im Inspector! <<<");
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            StartCoroutine(SpawnPlayerWithDelay(clientId)); 
        }
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null && IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($">>> Client {clientId} verbunden. Starte Verzögerung... <<<");
        StartCoroutine(SpawnPlayerWithDelay(clientId));
    }

    private IEnumerator SpawnPlayerWithDelay(ulong clientId)
    {
        // 1. Warten
        yield return new WaitForSeconds(clientSpawnDelay);

        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            SpawnPlayer(clientId);
        }
        else
        {
            Debug.LogWarning($"Client {clientId} hat während des Delays die Verbindung verloren.");
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        Vector3 spawnPos = new Vector3(0, 2, 0); 
        GameObject playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        RegisterPlayer(playerInstance.transform);
    }
    public void RegisterPlayer(Transform playerTransform)
    {
        if (!activePlayers.Contains(playerTransform))
        {
            activePlayers.Add(playerTransform);
            onPlayerRegistered?.Invoke();
        }
    }
    public void UnregisterPlayer(Transform playerTransform)
    {
        if (activePlayers.Contains(playerTransform))
        {
            activePlayers.Remove(playerTransform);
        }
    }
    public List<Transform> GetActivePlayers()
    {
        return activePlayers;
    }
}