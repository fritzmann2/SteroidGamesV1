using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class DropItem : NetworkBehaviour
{
    private InventoryItemInstance inventoryItemInstance;
    private int amount;
    public SpriteRenderer image;
    
    [Header("Pickup Settings")]
    public float maxPickupDistance = 2f;
    private float timer = 10f;

    void Update()
    {
        if (IsServer)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else if (GetComponent<NetworkObject>().IsSpawned)
            {
                GetComponent<NetworkObject>().Despawn();
            }
        }

        if (Mouse.current.leftButton.wasPressedThisFrame || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame))
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            
            Collider2D hit = Physics2D.OverlapPoint(worldPosition);
            
            if (hit != null && hit.gameObject == gameObject)
            {
                HandlePickup();
            }
        }
    }

    private void HandlePickup()
    {
        if (NetworkManager.Singleton == null || NetworkManager.Singleton.LocalClient == null || NetworkManager.Singleton.LocalClient.PlayerObject == null) 
            return;

        GameObject localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        float distance = Vector2.Distance(transform.position, localPlayer.transform.position);

        if (distance <= maxPickupDistance)
        {
            Inventory playerInventory = localPlayer.GetComponent<Inventory>();
            if (playerInventory != null)
            {
                playerInventory.itemInventory.addItem(inventoryItemInstance, amount);
                PickupItemServerRpc();
            }
        }
    }

    public void init(InventoryItemInstance _inventoryItemInstance, int _amount)
    {
        inventoryItemInstance = _inventoryItemInstance;
        amount = _amount;
        if (image != null && inventoryItemInstance.itemData != null)
        {
            image.sprite = inventoryItemInstance.itemData.Icon;
            image.color = Color.white; 
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PickupItemServerRpc()
    {
        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn();
        }
    }
}