using UnityEngine;

public class destroyobj : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckBreak(other.gameObject);
    }
    private void CheckBreak(GameObject obj)
    {
        if (obj.CompareTag("Player"))
        {
            if (obj.GetComponent<PlayerMovement>().isDashing)
            {
                // Zerstöre dieses Objekt
                Destroy(gameObject);
            }
        }
    }
}
