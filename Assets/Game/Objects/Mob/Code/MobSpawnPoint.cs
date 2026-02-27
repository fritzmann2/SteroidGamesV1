using System.Collections.Generic;
using UnityEngine;

public class MobSpawnPoint : MonoBehaviour
{
    [Header("Debug Ansicht")]
    public Color gizmoColor = Color.red;
    public List<string> possibleMobsNames = new List<string>();
    public bool isBossSpawner = false;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

    public string getRandomMobName()
    {
        if (possibleMobsNames.Count == 0) return null;

        int randomIndex = Random.Range(0, possibleMobsNames.Count);
        return possibleMobsNames[randomIndex];
    }
}


