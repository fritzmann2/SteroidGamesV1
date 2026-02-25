 using UnityEngine;

using Unity.Netcode;



public class Attackmanager : NetworkBehaviour

{

    [Header("Setup")]
    public Transform handHolder;
    public ItemInventory itemInventory;

    [Header("Input")]
    private GameControls controls;
    public GameObject weaponPrefab;
    public GameObject basicWeapon;

    private GameObject currentWeaponObject;
    private Weapon currentWeaponScript;
    private const int WEAPON_SLOT_INDEX = 4;

    void Awake()
    {
        controls = new GameControls();
    }

    private void OnDisable()
    {
        itemInventory.OnEquipmentChanged -= setWeapon;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner) controls.Enable();
        if (IsOwner || IsServer)
        {
            itemInventory = GetComponent<Inventory>().itemInventory;
            if (itemInventory != null)
            {
                itemInventory.equipmentSlots[WEAPON_SLOT_INDEX].OnEquipmentChanged += OnWeaponSlotChanged;
            }
            
            PlayerSaveHandler saveHandler = GetComponent<PlayerSaveHandler>();
            if (saveHandler != null) saveHandler.dataLoaded += setWeapon;
            itemInventory.OnEquipmentChanged += setWeapon;
            setWeapon();
        }
    }
    private void OnWeaponSlotChanged(int slotIndex)
    {
        setWeapon();
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner) 
        {
            controls.Disable();
            if (currentWeaponObject != null)
            {
                Destroy(currentWeaponObject);
            }
        }
        if (IsOwner || IsServer)
        {
            if (itemInventory != null)
            {
                itemInventory.equipmentSlots[WEAPON_SLOT_INDEX].OnEquipmentChanged -= OnWeaponSlotChanged;
            }
            PlayerSaveHandler saveHandler = GetComponent<PlayerSaveHandler>();
            if (saveHandler != null) saveHandler.dataLoaded -= setWeapon;

            if (currentWeaponObject != null)
            {
                Destroy(currentWeaponObject);
            }
        }
    }
    void Update()
    {
        if (!IsOwner) return;
        // Waffe ausrüsten/ablegen
        if (controls.Gameplay.SummonWeapon.triggered)
        {  
//            Debug.Log("Summoning or despawning weapon");
            bool shouldEquip = currentWeaponObject == null;
            EquipRequestServerRpc(shouldEquip ? 0 : -1);
        }
        // Angriffe ausführen und abfrage welche
        if (currentWeaponScript != null)
        {
            if (controls.Gameplay.Attack1.triggered || controls.Gameplay.Attack2.triggered || controls.Gameplay.Attack3.triggered || controls.Gameplay.Attack4.triggered)
            {
                if (currentWeaponScript.weaponstats == null)
                {
                    currentWeaponScript.weaponstats = new WeaponStats
                    {
                        weapondamage = 10f,
                        strength = 5f,
                        critChance = 20f,
                        critDamage = 50f
                    };
                }
               
            }
            if (controls.Gameplay.Attack1.triggered) currentWeaponScript.Attack1();
            else if (controls.Gameplay.Attack2.triggered) currentWeaponScript.Attack2();
            else if (controls.Gameplay.Attack3.triggered) currentWeaponScript.Attack3();
            else if (controls.Gameplay.Attack4.triggered) 
            {
//                Debug.Log("[Client] Attack 4 Taste WURDE GEDRÜCKT im Attackmanager!");
                currentWeaponScript.Attack4();
            }
            if (controls.Gameplay.Attack1.triggered || controls.Gameplay.Attack2.triggered || controls.Gameplay.Attack3.triggered || controls.Gameplay.Attack4.triggered)
            {
               /*
               animtime = currentWeaponScript.GetAnimationLength();
               Debug.Log("Animation time: " + animtime);
               */
            }
        }
    }
    [ServerRpc]
    private void EquipRequestServerRpc(int weaponIndex)
    {
        if (currentWeaponObject != null)
        {
            Destroy(currentWeaponObject);
            currentWeaponObject = null;
            currentWeaponScript = null;
        }

        if (weaponIndex >= 0 && weaponPrefab != null)
        {
            GameObject newWeapon = Instantiate(weaponPrefab, handHolder);
            
            newWeapon.transform.localPosition = Vector3.zero;
            newWeapon.transform.localRotation = Quaternion.identity;

            var netObj = newWeapon.GetComponent<NetworkObject>();
            netObj.Spawn();
            
            netObj.TrySetParent(this.NetworkObject);

            currentWeaponObject = netObj.gameObject;
            currentWeaponScript = netObj.GetComponent<Weapon>();
            
            currentWeaponScript.SetFollowTarget(this.handHolder);

            EquipClientRpc(netObj.NetworkObjectId);
        }
        else
        {
            // Debug.Log("Hand wird sichtbar (keine Waffe).");
        }
    }

    [ClientRpc]
    private void EquipClientRpc(ulong weaponNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(weaponNetworkId, out NetworkObject weaponNetObj))
        {            
            currentWeaponObject = weaponNetObj.gameObject;
            currentWeaponScript = weaponNetObj.GetComponent<Weapon>();
            currentWeaponScript.SetFollowTarget(this.handHolder);
        }
    }
    public void setWeapon()
    {
        if (itemInventory == null)
        {
            Debug.LogWarning("No itemInventory found");
            return;
        }
        if (itemInventory.equipmentSlots[4] == null)
        {
            Debug.LogWarning("No weapon Equiped");
            return;
        }
        var weaponSlot = itemInventory.equipmentSlots[4];
        if (weaponSlot.InventoryItemInstance != null && weaponSlot.InventoryItemInstance.itemData != null)
        {
            weaponPrefab = weaponSlot.InventoryItemInstance.itemData.itemObject;
        }
        else
        {
            weaponPrefab = basicWeapon;
        }
        if (currentWeaponObject != null)
        {
            EquipRequestServerRpc(0);
        }
    }
} 