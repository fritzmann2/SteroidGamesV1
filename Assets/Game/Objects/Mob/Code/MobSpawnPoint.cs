using System.Collections.Generic;
using UnityEngine;

public class MobSpawnPoint : MonoBehaviour
{
    [Header("Debug Ansicht")]
    public Color gizmoColor = Color.red;
    public string possibleMobName;
    public bool isBossSpawner = false;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

    public string getMobName()
    {
        return possibleMobName;
    }
}


