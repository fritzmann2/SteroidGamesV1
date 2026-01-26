using UnityEngine;

public class MobSpawnPoint : MonoBehaviour
{
    [Header("Debug Ansicht")]
    public Color gizmoColor = Color.red;

    // Das hier hilft dir, den Punkt im Editor zu sehen (zeichnet eine Kugel)
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}


