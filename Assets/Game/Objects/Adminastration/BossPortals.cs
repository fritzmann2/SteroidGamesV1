using UnityEngine;
using Unity.Netcode;

public class BossPortals : NetworkBehaviour
{
    [Header("Teleport Ziel")]
    public Vector3 destinationCoordinate; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            NetworkObject netObj = other.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsOwner)
            {
                RequestTeleportServerRpc(netObj.NetworkObjectId);
            }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestTeleportServerRpc(ulong playerNetworkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out NetworkObject playerObj))
        {
            WorldGenerator worldGen = FindAnyObjectByType<WorldGenerator>();
            if (worldGen != null)
            {
                worldGen.TeleportToBoss(destinationCoordinate, playerObj.transform);
            }
            else
            {
                Debug.LogError("BossPortal: WorldGenerator nicht in der Szene gefunden!");
            }
        }
    }
}