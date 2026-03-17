using Unity.Netcode;
using UnityEngine.Events;

public class EventManager : NetworkBehaviour
{
    public UnityEvent<int> OnAddItem;

    public void AddItem(int id)
    {
        OnAddItem.Invoke(id);
    }
}
