using UnityEngine;

public class ShopNPC : StationaryNPC
{
    public void init(Vector2 _position, int _levelrequired)
    {
        position = _position;
        levelrequired = _levelrequired;
    }

    public void Interact(Transform playerTransform)
    {
        if (playerTransform.GetComponent<PlayerStats>().getLevel() >= levelrequired)
        {
            PauseManager.Instance.OpenShopUI();
        }
        else
        {
            Debug.Log("You need to be at least level " + levelrequired + " to access the shop.");
        }
    }
}
