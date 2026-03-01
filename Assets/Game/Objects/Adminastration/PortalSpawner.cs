using UnityEngine;

public class PortalSpawner : MonoBehaviour
{
    public Color gizmoColor = Color.blue;

    public Vector3 teleportDestination;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
