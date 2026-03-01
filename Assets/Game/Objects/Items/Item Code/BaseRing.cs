using UnityEngine;

public class BaseRing : InventoryItem
{
    [SerializeField] public AccessoryStats accessorystats;

    public void init(AccessoryStats _accessorystats)
    {
        accessorystats = _accessorystats;
        playerStats.UpdateStatsFromEquipment(5);
        playerStats.UpdateStatsFromEquipment(6);
    }
}
