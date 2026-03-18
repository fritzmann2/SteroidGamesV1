using UnityEngine;

public class PortalSpawner : MonoBehaviour
{
    public Color gizmoColor = Color.blue;

    public Vector3 teleportDestination;
    public int xCoordinate;
    public int yCoordinate;

    public void Reset()
    {
        teleportDestination = new Vector3(xCoordinate, yCoordinate, 0);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
