using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class BossPortals : NetworkBehaviour
{
    [Header("Teleport Ziel")]
    public Vector3 destinationCoordinate; 
    private bool isLocalPlayerInZone = false;
    private NetworkObject localPlayerNetObj;
    private GameControls controls;

    public override void OnNetworkSpawn()
    {
        controls = new GameControls();
        controls.Enable();
    }

    public override void OnNetworkDespawn()
    {
        controls.Disable();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            NetworkObject netObj = other.GetComponent<NetworkObject>();
            
            if (netObj != null && netObj.IsOwner)
            {
                isLocalPlayerInZone = true;
                localPlayerNetObj = netObj;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            NetworkObject netObj = other.GetComponent<NetworkObject>();
            
            if (netObj != null && netObj.IsOwner)
            {
                isLocalPlayerInZone = false;
                localPlayerNetObj = null;
            }
        }
    }

    private void Update()
    {
        if (!isLocalPlayerInZone || localPlayerNetObj == null) return;

        bool teleportTriggered = false;

        if (Mouse.current != null && controls.Gameplay.RightMouse .IsPressed())
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(worldPosition);
            
            if (hit != null && hit.gameObject == gameObject)
            {
                teleportTriggered = true;
            }
        }

        if (Gamepad.current != null && controls.Gameplay.Teleport.IsPressed())
        {
            teleportTriggered = true;
        }

        if (teleportTriggered)
        {
            RequestTeleportServerRpc(localPlayerNetObj.NetworkObjectId);
            isLocalPlayerInZone = false;
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
                //worldGen.TeleportToBoss(destinationCoordinate, playerObj.transform);
            }
            else
            {
                Debug.LogError("BossPortal: WorldGenerator nicht in der Szene gefunden!");
            }
        }
    }
}